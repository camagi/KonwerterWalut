using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace CurrencyConverter
{
     class ExchangeDBcontext : DbContext
    {
        public DbSet<Exchange> Exchanges { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=HAPE;Initial Catalog=CurrencyExchangeDB;Integrated Security=True;TrustServerCertificate=True");
        }
    }
}
