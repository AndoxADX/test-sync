using Microsoft.EntityFrameworkCore;

namespace TodoApi.Models
{
    public class TodoContext : DbContext
    {
        public TodoContext(DbContextOptions<TodoContext> options)
            : base(options)
        {
        }

        public DbSet<ReqTransaction> reqtransactions { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        // public DbSet<PlatformAccount> Accounts { get; set; }
        // public DbSet<Trx> Transaction { get; set; }
    }
}