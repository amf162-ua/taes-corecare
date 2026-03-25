using CoreCare.Models;
using Microsoft.EntityFrameworkCore;
using Mono.Unix;
using System;
using System.Collections.Generic;
using System.Text;
namespace CoreCare.Data
{
    // por ahora solo contiene la tabla donde se guarda los resultados del benchark y users
    public class CoreCareDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<RegistroBenchmark> RegistrosBenchmark { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=corecare.db");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(u => u.Plan)
                .HasConversion<string>();
        }

    }
}
