using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

// para generar carga sobre CPU, RAM and DISK durante el benchmark
// cada metodo va correr en un hilo separado
namespace CoreCare.Services
{
    public class StressWorker
    {
        private CancellationTokenSource? _cts;

        public void RunCpuStress()
        {

        }

        public void RunRamStress()
        {

        }

        public void RunDiskStress()
        {

        }
    }
}
