using System;
using System.Collections.Concurrent;
using System.Threading;

namespace PdfCutting
{
    public class AsyncProducerConsumerQueue<T> : IDisposable
    {
        private readonly Action<T> _consumer;
        private readonly BlockingCollection<T> _queue;
        private readonly CancellationTokenSource _cancelTokenSrc;

        public AsyncProducerConsumerQueue(Action<T> consumer)
        {
            _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
            _queue = new BlockingCollection<T>(new ConcurrentQueue<T>());
            _cancelTokenSrc = new CancellationTokenSource();

            new Thread(() => ConsumeLoop(_cancelTokenSrc.Token)).Start();
        }

        public void Dispose()
        {
            _queue?.Dispose();
            _cancelTokenSrc.Cancel();
            _cancelTokenSrc?.Dispose();
        }

        public void Produce(T value)
        {
            try
            {
                _queue.Add(value);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void ConsumeLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var item = _queue.Take(cancellationToken);
                    _consumer(item);
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Error in ConsumeLoop: " + exception.Message);
                }
            }
            _queue.Dispose();
        }
    }
}