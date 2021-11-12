using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Proxies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BigLab.Entities
{
    public class Context: DbContext
    {
        public DbSet<UserInfo> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Game> Games { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseLazyLoadingProxies().UseSqlServer(@"Server=localhost,1433;Database=BigLab;User=sa;Password=@Passw0rd;");
        }

        public Context()
        {
            Database.EnsureCreated();
        }
    }
}
