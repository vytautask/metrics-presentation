using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Prometheus;
using sample_app.Models;

namespace sample_app.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            WebClient client = new WebClient();
            var stopwatch = Stopwatch.StartNew();
            var resultString = client.DownloadString("http://www.engelbert-strauss.de");
            stopwatch.Stop();

            var summary = Metrics.CreateSummary("main_page_download_time", "Ms it took to download home page of Engelbert-Strauss.de");
            summary.Observe(stopwatch.ElapsedMilliseconds);

            ViewBag.DownloadTook = stopwatch.ElapsedMilliseconds;

            return View();
        }

        private Counter GetSimpleCounter() {
            return Metrics.CreateCounter("increase_counter", "Counter from Sample Metrics application", 
                labelNames: new []{ "button_color", "button_type"});
        }

        public IActionResult IncreaseCounter(string color, string type) 
        {
            var counter = GetSimpleCounter();
            counter.Labels(color, type).Inc();

            return RedirectToAction("Index");
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
