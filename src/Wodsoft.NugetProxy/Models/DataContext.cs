using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wodsoft.NugetProxy.Models
{
    public class DataContext : DbContext
    {
        public DbSet<Page> Page { get; set; }
    }
}
