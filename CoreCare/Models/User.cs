using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCare.Models
{
    public enum TipoPlan
    {
        Basico,
        Premium
    }
    public class User
    {
        public int Id { get; set; }
        public string name { get; set; } = string.Empty;
        public string username { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public TipoPlan Plan { get; set; } = TipoPlan.Basico;
        public DateTime createdAt { get; set; }
        public bool IsActive { get; set; } = true;

        public List<RegistroBenchmark> BenchmarkHistory { get; set; } = new();

    }
}
