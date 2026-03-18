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

        public void RunCpuStress(int seconds) //controla la duracion del estres, para no tenemos que llamar el stop manuelmente luego
        {
            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            var end = DateTime.Now.AddSeconds(seconds);
            Task.Run(() =>
            {
                while (DateTime.Now < end && !token.IsCancellationRequested)
                    Math.Sqrt(new Random().NextDouble());
            }, token);
        }

        public void RunRamStress(int seconds)
        {
            _cts = new CancellationTokenSource();
            Task.Delay(seconds * 1000, _cts.Token).ContinueWith(_ => { });
            var block = new byte[256 * 1024 * 1024];
            Task.Delay(seconds * 1000).Wait();
            GC.Collect();
        }

        public void RunDiskStress(int seconds)
        {
            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            var path = Path.GetTempFileName();
            Task.Run(() =>
            {
                var end = DateTime.Now.AddSeconds(seconds);
                while (DateTime.Now < end && !token.IsCancellationRequested)
                    File.WriteAllBytes(path, new byte[1024 * 1024]);
                File.Delete(path);
            }, token);
        }

        public void Stop() => _cts?.Cancel();
    }
}
