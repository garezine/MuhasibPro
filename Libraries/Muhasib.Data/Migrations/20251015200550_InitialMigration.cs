using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Muhasib.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppVersiyonlar",
                columns: table => new
                {
                    MevcutVersiyon = table.Column<string>(type: "TEXT", nullable: false),
                    UygulamaSonGuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OncekiUygulamaVersiyon = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppVersiyonlar", x => x.MevcutVersiyon);
                });

            migrationBuilder.CreateTable(
                name: "Firmalar",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    FirmaKodu = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    KisaUnvani = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TamUnvani = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    YetkiliKisi = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Il = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Ilce = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Adres = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    PostaKodu = table.Column<string>(type: "TEXT", maxLength: 25, nullable: true),
                    Telefon1 = table.Column<string>(type: "TEXT", maxLength: 17, nullable: true),
                    Telefon2 = table.Column<string>(type: "TEXT", maxLength: 17, nullable: true),
                    VergiDairesi = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    VergiNo = table.Column<string>(type: "TEXT", maxLength: 11, nullable: true),
                    TCNo = table.Column<string>(type: "TEXT", maxLength: 11, nullable: true),
                    Web = table.Column<string>(type: "TEXT", maxLength: 75, nullable: true),
                    Eposta = table.Column<string>(type: "TEXT", maxLength: 75, nullable: true),
                    Logo = table.Column<byte[]>(type: "BLOB", nullable: true),
                    LogoOnizleme = table.Column<byte[]>(type: "BLOB", nullable: true),
                    PBu1 = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    PBu2 = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    KaydedenId = table.Column<long>(type: "INTEGER", nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GuncelleyenId = table.Column<long>(type: "INTEGER", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    ArananTerim = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Firmalar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KullaniciRoller",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    RolAdi = table.Column<string>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: false),
                    KaydedenId = table.Column<long>(type: "INTEGER", nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GuncelleyenId = table.Column<long>(type: "INTEGER", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    ArananTerim = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KullaniciRoller", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SistemLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false),
                    User = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Action = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 400, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    KaydedenId = table.Column<long>(type: "INTEGER", nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GuncelleyenId = table.Column<long>(type: "INTEGER", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    ArananTerim = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SistemLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SistemDbVersiyonlar",
                columns: table => new
                {
                    MevcutVersiyon = table.Column<string>(type: "TEXT", nullable: false),
                    MevcutDbVersiyon = table.Column<string>(type: "TEXT", nullable: false),
                    SistemDBSonGuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OncekiSistemDbVersiyon = table.Column<string>(type: "TEXT", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "MaliDonemler",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    FirmaId = table.Column<long>(type: "INTEGER", nullable: false),
                    MaliYil = table.Column<int>(type: "INTEGER", nullable: false),
                    KaydedenId = table.Column<long>(type: "INTEGER", nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GuncelleyenId = table.Column<long>(type: "INTEGER", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    ArananTerim = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaliDonemler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaliDonemler_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Kullanicilar",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    KullaniciAdi = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SifreHash = table.Column<string>(type: "TEXT", maxLength: 400, nullable: false),
                    Adi = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Soyadi = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Eposta = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RolId = table.Column<long>(type: "INTEGER", nullable: false),
                    Telefon = table.Column<string>(type: "TEXT", maxLength: 17, nullable: false),
                    Resim = table.Column<byte[]>(type: "BLOB", nullable: true),
                    ResimOnizleme = table.Column<byte[]>(type: "BLOB", nullable: true),
                    KaydedenId = table.Column<long>(type: "INTEGER", nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GuncelleyenId = table.Column<long>(type: "INTEGER", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    ArananTerim = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kullanicilar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kullanicilar_KullaniciRoller_RolId",
                        column: x => x.RolId,
                        principalTable: "KullaniciRoller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaliDonemDbler",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    FirmaId = table.Column<long>(type: "INTEGER", nullable: false),
                    MaliDonemId = table.Column<long>(type: "INTEGER", nullable: false),
                    DBName = table.Column<string>(type: "TEXT", nullable: false),
                    Directory = table.Column<string>(type: "TEXT", nullable: false),
                    DBPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    DatabaseType = table.Column<int>(type: "INTEGER", nullable: false),
                    KaydedenId = table.Column<long>(type: "INTEGER", nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GuncelleyenId = table.Column<long>(type: "INTEGER", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    ArananTerim = table.Column<string>(type: "TEXT", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "Hesaplar",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    KullaniciId = table.Column<long>(type: "INTEGER", nullable: false),
                    SonGirisTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    KaydedenId = table.Column<long>(type: "INTEGER", nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GuncelleyenId = table.Column<long>(type: "INTEGER", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    ArananTerim = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hesaplar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hesaplar_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "AppVersiyonlar",
                columns: new[] { "MevcutVersiyon", "OncekiUygulamaVersiyon", "UygulamaSonGuncellemeTarihi" },
                values: new object[] { "1.0.0", null, new DateTime(2025, 9, 22, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                table: "KullaniciRoller",
                columns: new[] { "Id", "Aciklama", "AktifMi", "ArananTerim", "GuncellemeTarihi", "GuncelleyenId", "KaydedenId", "KayitTarihi", "RolAdi" },
                values: new object[] { 1L, "Sistemin tüm özelliklerine erişim yetkisi.", true, null, null, null, 5413300800L, new DateTime(2025, 3, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Yönetici" });

            migrationBuilder.InsertData(
                table: "Kullanicilar",
                columns: new[] { "Id", "Adi", "AktifMi", "ArananTerim", "Eposta", "GuncellemeTarihi", "GuncelleyenId", "KaydedenId", "KayitTarihi", "KullaniciAdi", "Resim", "ResimOnizleme", "RolId", "SifreHash", "Soyadi", "Telefon" },
                values: new object[] { 241341L, "Ömer", true, "korkutomer, Ömer Korkut, Yönetici", "korkutomer@gmail.com", null, null, 5413300800L, new DateTime(2025, 3, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "korkutomer", null, null, 1L, "AQAAAAIAAYagAAAAECnYdlrjFiWFJc+FGeGDmvR87uz20oU/Z0K4JE9ddoF2VUnmHw0idEFX8UPOb4cpzQ==", "Korkut", "0 (541) 330 0800" });

            migrationBuilder.InsertData(
                table: "SistemDbVersiyonlar",
                columns: new[] { "MevcutVersiyon", "MevcutDbVersiyon", "OncekiSistemDbVersiyon", "SistemDBSonGuncellemeTarihi" },
                values: new object[] { "1.0.0", "1.0.0", null, new DateTime(2025, 9, 22, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.CreateIndex(
                name: "IX_Hesaplar_KullaniciId",
                table: "Hesaplar",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_RolId",
                table: "Kullanicilar",
                column: "RolId");

            migrationBuilder.CreateIndex(
                name: "IX_MaliDonemDbler_MaliDonemId",
                table: "MaliDonemDbler",
                column: "MaliDonemId");

            migrationBuilder.CreateIndex(
                name: "IX_MaliDonemler_FirmaId",
                table: "MaliDonemler",
                column: "FirmaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Hesaplar");

            migrationBuilder.DropTable(
                name: "MaliDonemDbler");

            migrationBuilder.DropTable(
                name: "SistemDbVersiyonlar");

            migrationBuilder.DropTable(
                name: "SistemLogs");

            migrationBuilder.DropTable(
                name: "Kullanicilar");

            migrationBuilder.DropTable(
                name: "MaliDonemler");

            migrationBuilder.DropTable(
                name: "AppVersiyonlar");

            migrationBuilder.DropTable(
                name: "KullaniciRoller");

            migrationBuilder.DropTable(
                name: "Firmalar");
        }
    }
}
