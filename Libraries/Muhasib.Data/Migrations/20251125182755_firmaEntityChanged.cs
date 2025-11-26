using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Muhasib.Data.Migrations
{
    /// <inheritdoc />
    public partial class firmaEntityChanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaliDonemler_Firmalar_FirmaId",
                table: "MaliDonemler");

            migrationBuilder.DropColumn(
                name: "DbOlusturulduMu",
                table: "MaliDonemler");

            migrationBuilder.AddForeignKey(
                name: "FK_MaliDonemler_Firmalar_FirmaId",
                table: "MaliDonemler",
                column: "FirmaId",
                principalTable: "Firmalar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaliDonemler_Firmalar_FirmaId",
                table: "MaliDonemler");

            migrationBuilder.AddColumn<bool>(
                name: "DbOlusturulduMu",
                table: "MaliDonemler",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_MaliDonemler_Firmalar_FirmaId",
                table: "MaliDonemler",
                column: "FirmaId",
                principalTable: "Firmalar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
