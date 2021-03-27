using System;
using consoleAPp.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace consoleAPp.Data
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext()
        {

        }
        public DbSet<Departament> Departaments { get; set; }
        public DbSet<Employee> Employees { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            const string strConnection = "Server=DESKTOP-BC276D3\\SQLEXPRESS;Database=Mod1;User Id=iislog;Password=iislog;Trusted_Connection=True; pooling=true;";

            optionsBuilder.UseSqlServer(strConnection, p => p.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                          .EnableSensitiveDataLogging()
                          .LogTo(Console.WriteLine, LogLevel.Information);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // modelBuilder.Entity<Departament>().HasQueryFilter(x => !x.Excluido);
        }

    }
}