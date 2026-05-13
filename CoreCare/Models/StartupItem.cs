using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCare.Models
{
    public class StartupItem
    {
        public String Name { get; set; }
        public String Path { get; set; } // Ruta del ejecutable que se lanza
        public bool IsEnabled { get; set; } = true;
        public string Publisher { get; set; } = "Desconocido";
    }
}
