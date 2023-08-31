using dotnetcore60.Models;
using dotnetcore60.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace dotnetcore60.Controllers
{
    public static class FileSizeFormatter
    {
        // Load all suffixes in an array  
        static readonly string[] suffixes =
        { "Bytes", "KB", "MB", "GB", "TB", "PB" };
        public static string FormatSize(Int64 bytes)
        {
            int counter = 0;
            decimal number = (decimal)bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number = number / 1024;
                counter++;
            }
            return string.Format("{0:n1}{1}", number, suffixes[counter]);
        }
    }

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ISimulator _simulator;

        public HomeController(ILogger<HomeController> logger, ISimulator simulator)
        {
            _logger = logger;
            _simulator = simulator;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            _simulator.ThrowException();
            return View();
        }

        public IActionResult ThrowPIIException()
        {
            _simulator.ThrowPIIException();
            return View();
        }

        public IActionResult Slow()
        {
            _simulator.SyncLowSleep();
            return View();
        }

        public IActionResult HighSleep()
        {
            _simulator.SyncHighSleep();
            return View();
        }

        public async Task<IActionResult> SlowOutboundHttpServiceAsync()
        {
            await _simulator.SlowOutboundHttpServiceAsync();
            return View();
        }
        public async Task<IActionResult> SlowDatabaseConnectionAsync()
        {
            await _simulator.SlowDatabaseConnectionAsync();
            return View();
        }
        public async Task<IActionResult> SleepAsync()
        {
            await _simulator.SleepAsync();
            return View();
        }
        public IActionResult SlowOutboundServiceSync()
        {
            _simulator.SlowOutboundService();
            return View();
        }

        public IActionResult SlowDatabaseConnectionSync()
        {
            _simulator.SlowDatabaseConnection();
            return View();
        }

        public IActionResult NestedExceptionCrash()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(CrashMe));
            return View();
        }

        public IActionResult LeakMemoryLow()
        {
            _simulator.LeakMemoryLow();
            return View();
        }

        public IActionResult LeakMemoryHigh()
        {
            _simulator.LeakMemoryHigh();
            return View();
        }

        public IActionResult ThrowStackOverflow()
        {
            _simulator.CauseStackOverflow();
            return View();
        }

        public IActionResult GetContainerMemory()
        {
            string usageInBytes;
            try
            {
                string path = "/sys/fs/cgroup/memory/memory.usage_in_bytes";
                usageInBytes = System.IO.File.ReadAllText(path);

                if (long.TryParse(usageInBytes, out long memoryUsage))
                {
                    usageInBytes = FileSizeFormatter.FormatSize(memoryUsage);
                }
            }
            catch (Exception ex)
            {
                usageInBytes = ex.Message;
            }

            return View("GetContainerMemory", usageInBytes);
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