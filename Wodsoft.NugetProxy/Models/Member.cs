using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Wodsoft.NugetProxy.Models
{
    public class Member : UserBase
    {
        public virtual string Username { get; set; }
    }
}