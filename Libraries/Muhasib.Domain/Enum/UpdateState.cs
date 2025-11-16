namespace Muhasib.Domain.Enum
{
    public enum UpdateState
    {
        Idle,           // Güncel
        Checking,       // Kontrol ediliyor
        UpdateAvailable, // Güncelleme mevcut
        Downloading,    // İndiriliyor
        Downloaded,     // İndirildi, kuruluma hazır
        Installing,     // Kuruluyor
        RestartRequired, // Yeniden başlatma gerekli
        Error           // Hata
    }
}
