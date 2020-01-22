using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AcfunTools.CommentXuMing.Web.Models;
using AcfunTools.CommentXuMing.Model;

namespace AcfunTools.CommentXuMing.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly CommentXuMingDbContext _dbcontext;

        public HomeController(ILogger<HomeController> logger, CommentXuMingDbContext dbcontext)
        {
            _logger = logger;
            _dbcontext = dbcontext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Search(SreachViewModel sreach)
        {
            if (!ModelState.IsValid) return Ok(new { });

            var result = _dbcontext.Comments.FirstOrDefault(c => c.AcNo == sreach.AcNo && c.Floor == sreach.Floor);
            if (result == null)
            {
                return Ok(new { });
            }

            return Ok(new
            {
                msg = "",
                data = result
            });
        }

        public IActionResult Admin()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
