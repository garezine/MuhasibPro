using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Muhasib.Data.Migrations
{
    /// <inheritdoc />
    public partial class dbTabloEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MaliDonemDbler_MaliDonemId",
                table: "MaliDonemDbler");

            migrationBuilder.DropColumn(
                name: "FirmaId",
                table: "MaliDonemDbler");

            migrationBuilder.AddColumn<bool>(
                name: "DbOlusturulduMu",
                table: "MaliDonemler",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_MaliDonemDbler_MaliDonemId",
                table: "MaliDonemDbler",
                column: "MaliDonemId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MaliDonemDbler_MaliDonemId",
                table: "MaliDonemDbler");

            migrationBuilder.DropColumn(
                name: "DbOlusturulduMu",
                table: "MaliDonemler");

            migrationBuilder.AddColumn<long>(
                name: "FirmaId",
                table: "MaliDonemDbler",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_MaliDonemDbler_MaliDonemId",
                table: "MaliDonemDbler",
                column: "MaliDonemId");
        }
    }
}
