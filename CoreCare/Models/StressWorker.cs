using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

// Genera carga artificial sobre CPU, RAM y disco durante el benchmark.
// Cada método corre en un hilo separado para no bloquear la UI.
namespace CoreCare.Models
{
    public class StressWorker
    {
        private CancellationTokenSource? _cts;

        private CancellationToken GetOrCreateToken()
        {
            if (_cts == null || _cts.IsCancellationRequested)
            {
                _cts = new CancellationTokenSource();
            }

            return _cts.Token;
        }

        public void RunCpuStress(int seconds) //controla la duracion del estres, para no tenemos que llamar el stop manuelmente luego
        {
            var token = GetOrCreateToken();
            var end = DateTime.Now.AddSeconds(seconds);

            Task.Run(() =>
            {
                var random = new Random();
                while (DateTime.Now < end && !token.IsCancellationRequested)
                {
                    _ = Math.Sqrt(random.NextDouble());
                }
            }, token);
        }

        public void RunRamStress(int seconds)
        {
            var token = GetOrCreateToken();

            Task.Run(async () =>
            {
                var end = DateTime.Now.AddSeconds(seconds);
                var blocks = new List<byte[]>();

                try
                {
                    while (DateTime.Now < end && !token.IsCancellationRequested)
                    {
                        blocks.Add(new byte[16 * 1024 * 1024]);

                        if (blocks.Count >= 16)
                        {
                            await Task.Delay(250, token);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    blocks.Clear();
                    GC.Collect();
                }
            }, token);
        }

        public void RunDiskStress(int seconds)
        {
            var token = GetOrCreateToken();
            var path = Path.GetTempFileName();

            Task.Run(() =>
            {
                var end = DateTime.Now.AddSeconds(seconds);

                try
                {
                    while (DateTime.Now < end && !token.IsCancellationRequested)
                    {
                        File.WriteAllBytes(path, new byte[1024 * 1024]);
                    }
                }
                finally
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
            }, token);
        }

        public void Stop() => _cts?.Cancel();
    }
}
