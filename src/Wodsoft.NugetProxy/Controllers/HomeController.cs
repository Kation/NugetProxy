using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Wodsoft.NugetProxy.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Wodsoft.NugetProxy.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            DataContext context = new DataContext();
            ViewBag.PageCount = await context.Page.CountAsync();
            ViewBag.PackageCount = new DirectoryInfo("Packages").GetFiles().Length;
            return View();
        }

        public IActionResult Help()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
