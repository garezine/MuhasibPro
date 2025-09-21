using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Muhasebe.Data.Migrations
{
    /// <inheritdoc />
    public partial class initialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppVersiyon",
                columns: table => new
                {
                    Versiyon = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppVersiyon", x => x.Versiyon);
                });

            migrationBuilder.CreateTable(
                name: "DbYedekAl",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    FirmaId = table.Column<long>(type: "INTEGER", nullable: false),
                    DonemId = table.Column<long>(type: "INTEGER", nullable: false),
                    YedeklemeAraligi = table.Column<string>(type: "TEXT", nullable: false),
                    SonrakiYedekTarih = table.Column<DateTime>(type: "TEXT", nullable: false),
                    YedeklemeSaati = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbYedekAl", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Degerler",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    SonSecilenSirket = table.Column<long>(type: "INTEGER", nullable: false),
                    SonSecilenDonem = table.Column<long>(type: "INTEGER", nullable: false),
                    SonSecilenKullanici = table.Column<long>(type: "INTEGER", nullable: false),
                    KaydedenId = table.Column<long>(type: "INTEGER", nullable: false),
                    KayitTarihi = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    GuncelleyenId = table.Column<long>(type: "INTEGER", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    ArananTerim = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Degerler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DevirLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    Tarih = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SirketKisaUnvan = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SirketTamUnvan = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    KaynakDonem = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    HedefDonem = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    DevirNotu = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    SirketId = table.Column<long>(type: "INTEGER", nullable: false),
                    Moduller = table.Column<string>(type: "TEXT", nullable: false),
                    KaydedenId = table.Column<long>(type: "INTEGER", nullable: false),
                    KayitTarihi = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    GuncelleyenId = table.Column<long>(type: "INTEGER", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    ArananTerim = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevirLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DevirLogII",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    DevirAciklama = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    A1 = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    A2 = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    A3 = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    A4 = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    DevirYili = table.Column<int>(type: "INTEGER", nullable: false),
                    KaydedenId = table.Column<long>(type: "INTEGER", nullable: false),
                    KayitTarihi = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    GuncelleyenId = table.Column<long>(type: "INTEGER", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    ArananTerim = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevirLogII", x => x.Id);
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
                    KayitTarihi = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    GuncelleyenId = table.Column<long>(type: "INTEGER", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    ArananTerim = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Firmalar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KullaniciRol",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    RolAdi = table.Column<string>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: false),
                    KaydedenId = table.Column<long>(type: "INTEGER", nullable: false),
                    KayitTarihi = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    GuncelleyenId = table.Column<long>(type: "INTEGER", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    ArananTerim = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KullaniciRol", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModulSecim",
                columns: table => new
                {
                    KullaniciId = table.Column<long>(type: "INTEGER", nullable: false),
                    mdl_cari_y1 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_cari_y2 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_cari_y3 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_cari_y4 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_kasa_y1 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_kasa_y2 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_kasa_y3 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_kasa_y4 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_cek_y1 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_cek_y2 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_cek_y3 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_cek_y4 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_senet_y1 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_senet_y2 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_senet_y3 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_senet_y4 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_personel_y1 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_personel_y2 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_personel_y3 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_personel_y4 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_banka_y1 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_banka_y2 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_banka_y3 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_banka_y4 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_stok_y1 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_stok_y2 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_stok_y3 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_stok_y4 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_teklif_y1 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_teklif_y2 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_teklif_y3 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_teklif_y4 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_siparis_y1 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_siparis_y2 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_siparis_y3 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_siparis_y4 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_ajanda_y1 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_ajanda_y2 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_ajanda_y3 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_ajanda_y4 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_istatistikler_y1 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_istatistikler_y2 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_istatistikler_y3 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_istatistikler_y4 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_raporlar_y1 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_raporlar_y2 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_raporlar_y3 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_raporlar_y4 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_ayarlar_y1 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_ayarlar_y2 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_ayarlar_y3 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_ayarlar_y4 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_hatirlatma_y1 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_hatirlatma_y2 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_hatirlatma_y3 = table.Column<bool>(type: "INTEGER", nullable: false),
                    mdl_hatirlatma_y4 = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModulSecim", x => x.KullaniciId);
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
                    KayitTarihi = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    GuncelleyenId = table.Column<long>(type: "INTEGER", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    ArananTerim = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SistemLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CalismaDonemler",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    FirmaId = table.Column<long>(type: "INTEGER", nullable: false),
                    MaliDonemler = table.Column<int>(type: "INTEGER", nullable: false),
                    KaydedenId = table.Column<long>(type: "INTEGER", nullable: false),
                    KayitTarihi = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    GuncelleyenId = table.Column<long>(type: "INTEGER", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    ArananTerim = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalismaDonemler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalismaDonemler_Firmalar_FirmaId",
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
                    KayitTarihi = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    GuncelleyenId = table.Column<long>(type: "INTEGER", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    ArananTerim = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kullanicilar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kullanicilar_KullaniciRol_RolId",
                        column: x => x.RolId,
                        principalTable: "KullaniciRol",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DonemDBSec",
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
                    KayitTarihi = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    GuncelleyenId = table.Column<long>(type: "INTEGER", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    ArananTerim = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonemDBSec", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DonemDBSec_CalismaDonemler_MaliDonemId",
                        column: x => x.MaliDonemId,
                        principalTable: "CalismaDonemler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Hesaplar",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    KullaniciId = table.Column<long>(type: "INTEGER", nullable: false),
                    FirmaId = table.Column<long>(type: "INTEGER", nullable: false),
                    DonemId = table.Column<long>(type: "INTEGER", nullable: false),
                    SonGirisTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    KaydedenId = table.Column<long>(type: "INTEGER", nullable: false),
                    KayitTarihi = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
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
                table: "KullaniciRol",
                columns: new[] { "Id", "Aciklama", "AktifMi", "ArananTerim", "GuncellemeTarihi", "GuncelleyenId", "KaydedenId", "KayitTarihi", "RolAdi" },
                values: new object[] { 1L, "Sistemin tüm özelliklerine erişim yetkisi.", true, null, null, null, 5413300800L, new DateTimeOffset(new DateTime(2025, 3, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 3, 0, 0, 0)), "Yönetici" });

            migrationBuilder.InsertData(
                table: "Kullanicilar",
                columns: new[] { "Id", "Adi", "AktifMi", "ArananTerim", "Eposta", "GuncellemeTarihi", "GuncelleyenId", "KaydedenId", "KayitTarihi", "KullaniciAdi", "Resim", "ResimOnizleme", "RolId", "SifreHash", "Soyadi", "Telefon" },
                values: new object[] { 241341L, "Ömer", true, "korkutomer, Ömer Korkut, Yönetici", "korkutomer@gmail.com", null, null, 5413300800L, new DateTimeOffset(new DateTime(2025, 3, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 3, 0, 0, 0)), "korkutomer", null, null, 1L, "AQAAAAIAAYagAAAAECnYdlrjFiWFJc+FGeGDmvR87uz20oU/Z0K4JE9ddoF2VUnmHw0idEFX8UPOb4cpzQ==", "Korkut", "0 (541) 330 0800" });

            migrationBuilder.CreateIndex(
                name: "IX_CalismaDonemler_FirmaId",
                table: "CalismaDonemler",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_DonemDBSec_MaliDonemId",
                table: "DonemDBSec",
                column: "MaliDonemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hesaplar_KullaniciId",
                table: "Hesaplar",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_RolId",
                table: "Kullanicilar",
                column: "RolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppVersiyon");

            migrationBuilder.DropTable(
                name: "DbYedekAl");

            migrationBuilder.DropTable(
                name: "Degerler");

            migrationBuilder.DropTable(
                name: "DevirLog");

            migrationBuilder.DropTable(
                name: "DevirLogII");

            migrationBuilder.DropTable(
                name: "DonemDBSec");

            migrationBuilder.DropTable(
                name: "Hesaplar");

            migrationBuilder.DropTable(
                name: "ModulSecim");

            migrationBuilder.DropTable(
                name: "SistemLogs");

            migrationBuilder.DropTable(
                name: "CalismaDonemler");

            migrationBuilder.DropTable(
                name: "Kullanicilar");

            migrationBuilder.DropTable(
                name: "Firmalar");

            migrationBuilder.DropTable(
                name: "KullaniciRol");
        }
    }
}
