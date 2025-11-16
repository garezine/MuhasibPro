namespace Muhasib.Domain.Enum
{
    public enum StatusMessageType
    {
        Info,       // Normal bilgi - Mavi
        Success,    // Başarı - Yeşil
        Warning,    // Uyarı - Turuncu
        Error,          // ❌ - Kırmızı
        Edit,           // ✏ - Mavi (Kalem)
        Saving,         // 💾 - Mavi (Disk)
        Deleting,       // 🗑 - Turuncu (Çöp kutusu)
        MultipleSelect,  // ☑ - Mavi (Checkbox)
        Refreshing      // 🔄 - Mavi (Refresh) - YENİ
    }
}
