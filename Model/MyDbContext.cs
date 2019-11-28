using Microsoft.EntityFrameworkCore;

namespace JpGoods.Model
{
    public class MyDbContext : DbContext
    {
        public DbSet<Goods> Goods { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=blogging.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Goods>().HasIndex(g=>g.GoodsNo).IsUnique();
        }
    }

}