using System;

namespace PdfCutting
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string destinationPath;
            string sourcePath;
            if (args.Length == 1)
            {
                destinationPath = args[0];
                sourcePath = Environment.CurrentDirectory;
            }
            else if (args.Length == 2)
            {
                sourcePath = args[0];
                destinationPath = args[1];
            }
            else
            {
                Console.WriteLine("Invalid number of parameters");
                return;
            }

            PdfService pdfService = new PdfService(sourcePath, destinationPath);
            pdfService.Start();
        }
    }
}