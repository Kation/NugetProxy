using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Wodsoft.NugetProxy.Models;

namespace Wodsoft.NugetProxy.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            DataContext context = HttpContext.RequestServices.GetRequiredService<DataContext>();
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
