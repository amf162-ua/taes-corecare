using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreCare.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegistrosBenchmark",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CpuLoad = table.Column<float>(type: "REAL", nullable: false),
                    CpuTemp = table.Column<float>(type: "REAL", nullable: false),
                    CpuClock = table.Column<float>(type: "REAL", nullable: false),
                    GpuTemp = table.Column<float>(type: "REAL", nullable: false),
                    GpuLoad = table.Column<float>(type: "REAL", nullable: false),
                    RamUsed = table.Column<float>(type: "REAL", nullable: false),
                    RamLoad = table.Column<float>(type: "REAL", nullable: false),
                    DiskLoad = table.Column<float>(type: "REAL", nullable: false),
                    DiskReadRate = table.Column<float>(type: "REAL", nullable: false),
                    DiskWriteRate = table.Column<float>(type: "REAL", nullable: false),
                    Score = table.Column<float>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosBenchmark", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrosBenchmark");
        }
    }
}
