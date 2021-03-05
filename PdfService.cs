using System;
using System.IO;
using System.Threading;
using System.Timers;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Timer = System.Timers.Timer;

namespace PdfCutting
{
    internal class PdfService
    {
        private Timer _timer;
        private string _sourcePath;
        private string _destinationPath;
        private readonly AsyncProducerConsumerQueue<string> _asyncProducerConsumerQueue;
            
        public PdfService(string sourcePath, string destinationPath)
        {
            _sourcePath = sourcePath;
            _destinationPath = destinationPath;
            _asyncProducerConsumerQueue = new AsyncProducerConsumerQueue<string>(CutPdfs);
            _timer = new Timer(10000);
            _timer.Elapsed += TimerOnElapsed;
            _timer.AutoReset = true;
        }
            
        public void Start()
        {
            try
            {
                AddFilesToQueue();
                Thread.Sleep(10000);
                _timer.Start();
                Console.WriteLine("Process started");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
            
        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            AddFilesToQueue();
        }
    
        private void CutPdfs(string filePathName)
        {
            PdfDocument inputDocument = PdfReader.Open(filePathName, PdfDocumentOpenMode.Import);
            string name = Path.GetFileNameWithoutExtension(filePathName);
            for (int i = 0; i < inputDocument.PageCount; i++)
            {
                PdfDocument outputDocument = new PdfDocument();
                outputDocument.Version = inputDocument.Version;
                outputDocument.Info.Title =
                    String.Format("Page {0} of {1}", i + 1, inputDocument.Info.Title);
                outputDocument.Info.Creator = inputDocument.Info.Creator;
                outputDocument.AddPage(inputDocument.Pages[i]);
                outputDocument.Save(Path.Combine(_destinationPath, $"{i}_{name}.pdf"));
            }
    
            try
            {
                string originalName = Path.GetFileNameWithoutExtension(filePathName);
                string historyPath = Path.Combine(Path.GetDirectoryName(filePathName), "History");
                File.Move(filePathName, historyPath + "\\" + originalName + ".pdf");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
            
        private void AddFilesToQueue()
        {
            string[] files = Directory.GetFiles(_sourcePath, "*.pdf");
            foreach (string file in files)
            {
                string path = Path.GetDirectoryName(file);
                string filename = Path.GetFileNameWithoutExtension(file);
                string tmpFileName = Path.Combine(path, filename + ".tmp");
                File.Move(file, tmpFileName);
                _asyncProducerConsumerQueue.Produce(tmpFileName);
            }
        }
    }
}