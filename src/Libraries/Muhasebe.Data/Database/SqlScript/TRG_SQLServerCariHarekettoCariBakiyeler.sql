CREATE TRIGGER TRG_CariHareket_UpdateBakiyeler
ON CariHareketler
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- Etkilenen CariId'leri bul
    DECLARE @CariIds TABLE (CariId BIGINT);
    INSERT INTO @CariIds
    SELECT CariId FROM inserted
    UNION
    SELECT CariId FROM deleted;

    -- CariBakiyeler'i güncelle
    MERGE INTO CariBakiyeler AS target
    USING (
        SELECT 
            ch.CariId,
            SUM(CASE WHEN ch.GC = 1 THEN ch.IslemTutari ELSE 0 END) AS Borc,
            SUM(CASE WHEN ch.GC = 0 THEN ch.IslemTutari ELSE 0 END) AS Alacak
        FROM CariHareketler ch
        WHERE ch.CariId IN (SELECT CariId FROM @CariIds)
        GROUP BY ch.CariId
    ) AS source
    ON target.CariId = source.CariId
    WHEN MATCHED THEN
        UPDATE SET 
            Borc = source.Borc,
            Alacak = source.Alacak,
            Bakiye = source.Borc - source.Alacak,
            BakiyeTipi = CASE 
                            WHEN (source.Borc - source.Alacak) > 0 THEN 'Borçlu'
                            WHEN (source.Borc - source.Alacak) < 0 THEN 'Alacaklı'
                            ELSE 'Nötr'
                         END
    WHEN NOT MATCHED THEN
        INSERT (CariId, Borc, Alacak, Bakiye, BakiyeTipi)
        VALUES (source.CariId, source.Borc, source.Alacak, source.Borc - source.Alacak, 
                CASE 
                    WHEN (source.Borc - source.Alacak) > 0 THEN 'Borçlu'
                    WHEN (source.Borc - source.Alacak) < 0 THEN 'Alacaklı'
                    ELSE 'Nötr'
                END);
END;