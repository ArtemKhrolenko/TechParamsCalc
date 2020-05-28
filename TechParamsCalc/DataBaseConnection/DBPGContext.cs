using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Configuration;
using System.Data.Entity.ModelConfiguration.Conventions;


namespace TechParamsCalc.DataBaseConnection
{
    public class DBPGContext : DbContext
    {
        public DBPGContext() : base(nameOrConnectionString: "Default") { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {            
            modelBuilder.HasDefaultSchema("public");
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>(); //Убирает добавление буквы s в конец названия таблицы
            base.OnModelCreating(modelBuilder);
        }
        
        
        public DbSet<Capacity.CapacityContent> capacityDescs { get; set; }        
        public DbSet<Density.DensityContent> densityDescs { get; set; }
        public DbSet<Content.ContentContent> contentDescs { get; set; }
        public DbSet<ServerConnections.AuthorizedHost> contentAutorizedServers { get; set; }
        public DbSet<Level.Tank> tanks { get; set; }
        public DbSet<Level.TankContent> tankContents { get; set; }



    }
}
