using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Pagamentos.Infrastructure.Context;

namespace Pagamentos.UnitTests.Repositories.Infra;
public static class EfSqliteInMemoryFactory
{
    public static (DbContextOptions<PagamentoContext> options, SqliteConnection conn) Create()
    {
        var conn = new SqliteConnection("DataSource=:memory:");
        conn.Open();

        var options = new DbContextOptionsBuilder<PagamentoContext>()
            .UseSqlite(conn)
            .EnableSensitiveDataLogging()
            .Options;

        using (var ctx = new PagamentoContext(options))
        {
            ctx.Database.EnsureCreated();
        }

        return (options, conn);
    }
}
