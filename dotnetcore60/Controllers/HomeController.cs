using dotnetcore60.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace dotnetcore60.Controllers
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
            return View();
        }

        public IActionResult Privacy()
        {
            throw new ApplicationException("Find this in trace file");
            return View();
        }

        public IActionResult Slow()
        {
            Thread.Sleep(5000);
            return View();
        }

        public IActionResult HighSleep()
        {
            Thread.Sleep(215 * 1000);
            return View();
        }

        public IActionResult NestedExceptionCrash()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(CrashMe));
            return View();
        }

        void CrashMe(object obj)
        {
            try
            {
                RaiseEventVoidAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("The CrashMe function crashed with an inner exception", ex);
            }
        }

        private void RaiseEventVoidAsync()
        {
            throw new Exception("Error!");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}