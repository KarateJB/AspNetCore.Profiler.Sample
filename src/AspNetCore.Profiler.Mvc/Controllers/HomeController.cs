using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AspNetCore.Profiler.Mvc.Models;

namespace AspNetCore.Profiler.Mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Update to get JWT from your Auth Server
            this.Response.Cookies.Append("access-token", "eyJhbGciOiJSUzI1NiIsImtpZCI6IjhQZnR4dkktckFoR05rc1lqaE5mMmciLCJ0eXAiOiJhdCtqd3QifQ.eyJuYmYiOjE1ODcwNzI0NzksImV4cCI6MTU4NzA3NjA3OSwiaXNzIjoiaHR0cHM6Ly9sb2NhbGhvc3Q6NjAwMSIsImF1ZCI6Ik15QmFja2VuZEFwaTEiLCJjbGllbnRfaWQiOiJNeUJhY2tlbmQiLCJzdWIiOiJqYmxpbiIsImF1dGhfdGltZSI6MTU4NzA3MjQ3OSwiaWRwIjoibG9jYWwiLCJlbWFpbCI6ImpibGluQGZha2UuY29tIiwianRpIjoia0dTc1lKRVluaXRja1hyOTI4M3NHUSIsInNjb3BlIjpbIk15QmFja2VuZEFwaTEiLCJvZmZsaW5lX2FjY2VzcyJdLCJhbXIiOlsicHdkIl19.xuXi7NLCS0vEZyAvObEck4HTmvHafAVYJMHUCPUPjinfA6e0dk4xfdZ_Hh2EQBpXAztVIFV_bBrTFYCvwd4-RvLp3yim5dRQHYcciM5iY51h7Mocm2JVOiapinKLFN0WM-MZlSOXpfQZwF-B30zku9EuxWSm481SQXUQ_kvYdkRVE8wckNZTnDG6ILIrZoxbUu-4gHG539uMiLaRrlKZdsdSGAHok-0LiCIcrq4dMKNl93dzB_XxZnWDcYzQo_66DayDK4ZkM4VOIYpfN0uE5Oq-VX0s9gtqLyqZbzul9mJNeDJoOp0C7pirLh06bX0Q1AT_jNUP_ZJ7K1Pje5tHug");
            return View();
        }

        public IActionResult Privacy()
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
