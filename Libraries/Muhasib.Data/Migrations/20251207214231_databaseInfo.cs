using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Muhasib.Data.Migrations
{
    /// <inheritdoc />
    public partial class databaseInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DatabaseName",
                table: "AppDbVersiyonlar",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "AppDbVersiyonlar",
                keyColumn: "CurrentAppVersion",
                keyValue: "1.0.0",
                column: "DatabaseName",
                value: "Sistem.db");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DatabaseName",
                table: "AppDbVersiyonlar");
        }
    }
}
