using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp15.Model
{
    public class ModelDb : DbContext
    {
        public ModelDb(): base("name=default")
        {

        }
        public DbSet<User> Users  { get; set; }
        public DbSet<Files> Files { get; set; }
        public DbSet<InfoPc> InfoPcs { get; set; }
        public DbSet<LogText> LogTexts { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
            .Property(u => u.email)
            .HasColumnName("email");
        }
    }
}
