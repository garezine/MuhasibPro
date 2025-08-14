namespace Muhasebe.Data.DataContext.DataContextFactory
{
    public interface IAppDbContextFactory
    {
        AppDbContext CreateDbContext();
    }
}
