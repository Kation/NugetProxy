using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wodsoft.NugetProxy.Models;

namespace Wodsoft.NugetProxy.Areas.Admin.Controllers
{
    public class SourceController : EntityController<Source>
    {
        public SourceController(IEntityContextBuilder builder) : base(builder) { }
    }
}