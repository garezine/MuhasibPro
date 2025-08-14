using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Muhasebe.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateManagerAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UpdateSettings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    AutoCheckOnStartup = table.Column<bool>(type: "INTEGER", nullable: false),
                    AutoDownload = table.Column<bool>(type: "INTEGER", nullable: false),
                    AutoInstall = table.Column<bool>(type: "INTEGER", nullable: false),
                    CheckIntervalHours = table.Column<int>(type: "INTEGER", nullable: false),
                    IncludeBetaVersions = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastCheckDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ShowNotifications = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdateChannel = table.Column<string>(type: "TEXT", nullable: false),
                    KaydedenId = table.Column<long>(type: "INTEGER", nullable: false),
                    KayitTarihi = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    GuncelleyenId = table.Column<long>(type: "INTEGER", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SearchTerms = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UpdateSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UpdateSettings");
        }
    }
}
