using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Wodsoft.NugetProxy.Models
{
    public class DataContext : DbContext
    {
        public DataContext()
        {
            Configuration.ValidateOnSaveEnabled = false;
        }

        public DbSet<Page> Page { get; set; }
    }
}