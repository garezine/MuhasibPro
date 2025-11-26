using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Muhasib.Data.Migrations
{
    /// <inheritdoc />
    public partial class maliDonemDbRemove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaliDonemDbler");

            migrationBuilder.AddColumn<string>(
                name: "DBName",
                table: "MaliDonemler",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DBPath",
                table: "MaliDonemler",
                type: "TEXT",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DatabaseType",
                table: "MaliDonemler",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Directory",
                table: "MaliDonemler",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DBName",
                table: "MaliDonemler");

            migrationBuilder.DropColumn(
                name: "DBPath",
                table: "MaliDonemler");

            migrationBuilder.DropColumn(
                name: "DatabaseType",
                table: "MaliDonemler");

            migrationBuilder.DropColumn(
                name: "Directory",
                table: "MaliDonemler");

            migrationBuilder.CreateTable(
                name: "MaliDonemDbler",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    MaliDonemId = table.Column<long>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    ArananTerim = table.Column<string>(type: "TEXT", nullable: true),
                    DBName = table.Column<string>(type: "TEXT", nullable: false),
                    DBPath = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    DatabaseType = table.Column<int>(type: "INTEGER", nullable: false),
                    Directory = table.Column<string>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GuncelleyenId = table.Column<long>(type: "INTEGER", nullable: true),
                    KaydedenId = table.Column<long>(type: "INTEGER", nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaliDonemDbler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaliDonemDbler_MaliDonemler_MaliDonemId",
                        column: x => x.MaliDonemId,
                        principalTable: "MaliDonemler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaliDonemDbler_MaliDonemId",
                table: "MaliDonemDbler",
                column: "MaliDonemId",
                unique: true);
        }
    }
}
