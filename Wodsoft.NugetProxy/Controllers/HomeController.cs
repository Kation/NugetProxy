using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Wodsoft.NugetProxy.Models;
using System.Data.Entity;
using System.IO;

namespace Wodsoft.NugetProxy.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            DataContext context = new DataContext();
            ViewBag.PageCount = await context.Page.CountAsync();
            ViewBag.PackageCount = new DirectoryInfo(Server.MapPath("~/Packages")).GetFiles().Length;
            return View();
        }

        public ActionResult Help()
        {
            return View();
        }
    }
}