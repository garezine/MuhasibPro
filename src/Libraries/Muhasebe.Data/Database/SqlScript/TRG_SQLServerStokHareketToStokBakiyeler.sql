CREATE TRIGGER TRG_StokHareket_UpdateBakiyeler
ON StokHareketler
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- Etkilenen StokId'leri bul
    DECLARE @StokIds TABLE (StokId BIGINT);
    INSERT INTO @StokIds
    SELECT StokId FROM inserted
    UNION
    SELECT StokId FROM deleted;

    -- StokBakiyeler'i güncelle
    MERGE INTO StokBakiyeler AS target
    USING (
        SELECT 
            sh.StokId,
            SUM(CASE WHEN sh.GC = 1 THEN sh.Miktar ELSE 0 END) AS ToplamGiris,
            SUM(CASE WHEN sh.GC = 0 THEN sh.Miktar ELSE 0 END) AS ToplamCikis,
            MAX(CASE WHEN sh.GC = 1 THEN sh.BirimFiyat END) AS SonAlis,
            MAX(CASE WHEN sh.GC = 0 THEN sh.BirimFiyat END) AS SonSatis
        FROM StokHareketler sh
        WHERE sh.StokId IN (SELECT StokId FROM @StokIds)
        GROUP BY sh.StokId
    ) AS source
    ON target.StokId = source.StokId
    WHEN MATCHED THEN
        UPDATE SET 
            ToplamStokGiris = source.ToplamGiris,
            ToplamStokCikis = source.ToplamCikis,
            MevcutStokBakiye = source.ToplamGiris - source.ToplamCikis,
            SonAlisFiyat = COALESCE(source.SonAlis, target.SonAlisFiyat),
            SonSatisFiyat = COALESCE(source.SonSatis, target.SonSatisFiyat)
    WHEN NOT MATCHED THEN
        INSERT (StokId, ToplamStokGiris, ToplamStokCikis, MevcutStokBakiye, SonAlisFiyat, SonSatisFiyat)
        VALUES (source.StokId, source.ToplamGiris, source.ToplamCikis, source.ToplamGiris - source.ToplamCikis, source.SonAlis, source.SonSatis);
END;