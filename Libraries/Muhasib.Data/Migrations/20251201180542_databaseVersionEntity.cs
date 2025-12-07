using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Muhasib.Data.Migrations
{
    /// <inheritdoc />
    public partial class databaseVersionEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SistemDbVersiyonlar");

            migrationBuilder.RenameColumn(
                name: "UygulamaSonGuncellemeTarihi",
                table: "AppVersiyonlar",
                newName: "CurrentAppVersionLastUpdate");

            migrationBuilder.RenameColumn(
                name: "OncekiUygulamaVersiyon",
                table: "AppVersiyonlar",
                newName: "PreviousAppVersiyon");

            migrationBuilder.RenameColumn(
                name: "MevcutVersiyon",
                table: "AppVersiyonlar",
                newName: "CurrentAppVersion");

            migrationBuilder.CreateTable(
                name: "AppDbVersiyonlar",
                columns: table => new
                {
                    CurrentAppVersion = table.Column<string>(type: "TEXT", nullable: false),
                    CurrentDatabaseVersion = table.Column<string>(type: "TEXT", nullable: false),
                    CurrentDatabaseLastUpdate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PreviousDatabaseVersion = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppDbVersiyonlar", x => x.CurrentAppVersion);
                    table.ForeignKey(
                        name: "FK_AppDbVersiyonlar_AppVersiyonlar_CurrentAppVersion",
                        column: x => x.CurrentAppVersion,
                        principalTable: "AppVersiyonlar",
                        principalColumn: "CurrentAppVersion",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AppDbVersiyonlar",
                columns: new[] { "CurrentAppVersion", "CurrentDatabaseLastUpdate", "CurrentDatabaseVersion", "PreviousDatabaseVersion" },
                values: new object[] { "1.0.0", new DateTime(2025, 9, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "1.0.0", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppDbVersiyonlar");

            migrationBuilder.RenameColumn(
                name: "PreviousAppVersiyon",
                table: "AppVersiyonlar",
                newName: "OncekiUygulamaVersiyon");

            migrationBuilder.RenameColumn(
                name: "CurrentAppVersionLastUpdate",
                table: "AppVersiyonlar",
                newName: "UygulamaSonGuncellemeTarihi");

            migrationBuilder.RenameColumn(
                name: "CurrentAppVersion",
                table: "AppVersiyonlar",
                newName: "MevcutVersiyon");

            migrationBuilder.CreateTable(
                name: "SistemDbVersiyonlar",
                columns: table => new
                {
                    MevcutVersiyon = table.Column<string>(type: "TEXT", nullable: false),
                    MevcutDbVersiyon = table.Column<string>(type: "TEXT", nullable: false),
                    OncekiSistemDbVersiyon = table.Column<string>(type: "TEXT", nullable: true),
                    SistemDBSonGuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SistemDbVersiyonlar", x => x.MevcutVersiyon);
                    table.ForeignKey(
                        name: "FK_SistemDbVersiyonlar_AppVersiyonlar_MevcutVersiyon",
                        column: x => x.MevcutVersiyon,
                        principalTable: "AppVersiyonlar",
                        principalColumn: "MevcutVersiyon",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "SistemDbVersiyonlar",
                columns: new[] { "MevcutVersiyon", "MevcutDbVersiyon", "OncekiSistemDbVersiyon", "SistemDBSonGuncellemeTarihi" },
                values: new object[] { "1.0.0", "1.0.0", null, new DateTime(2025, 9, 22, 0, 0, 0, 0, DateTimeKind.Unspecified) });
        }
    }
}
