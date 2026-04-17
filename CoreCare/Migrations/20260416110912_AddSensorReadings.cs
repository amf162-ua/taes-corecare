using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreCare.Migrations
{
    /// <inheritdoc />
    public partial class AddSensorReadings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SensorReadings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Component = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<double>(type: "REAL", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RegistroBenchmarkId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SensorReadings_RegistrosBenchmark_RegistroBenchmarkId",
                        column: x => x.RegistroBenchmarkId,
                        principalTable: "RegistrosBenchmark",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SensorReadings_RegistroBenchmarkId",
                table: "SensorReadings",
                column: "RegistroBenchmarkId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SensorReadings");
        }
    }
}
