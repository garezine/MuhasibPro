namespace MuhasibPro.Core.Infrastructure.Extensions
{
    public static class GuidExtensions
    {
        public static string ToShortString(this Guid guid, int length = 8)
        {
            return guid.ToString("N")[..length];
        }
    }
}
