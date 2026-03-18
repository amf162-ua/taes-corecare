using CoreCare.Models;
using Microsoft.EntityFrameworkCore;
using Mono.Unix;
using System;
using System.Collections.Generic;
using System.Text;
namespace CoreCare.Data
{
    // por ahora solo contiene la tabla donde se guarda los resultados del benchark,
    // luego se añaden las cosas de IA y usuarios cuando se implementan
    public class CoreCareDbContext : DbContext
    {
        public DbSet<RegistroBenchmark> RegistrosBenchmark { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=corecare.db");

    }
}
