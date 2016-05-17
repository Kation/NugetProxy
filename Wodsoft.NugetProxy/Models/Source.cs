using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Wodsoft.NugetProxy.Models
{
    public class Source : EntityBase
    {
        public virtual string Name { get; set; }

        public virtual string Url { get; set; }

        public virtual bool IsEnabled { get; set; }

        public virtual int Priority { get; set; }
    }
}