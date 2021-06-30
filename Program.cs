using System;
using System.Threading.Tasks;
using JobsBgScraper.Common;

namespace JobsBgScraper
{
    class Program
    {
        public static void Main(string[] args)
        {
            var sm = new ScraperManager();
            sm.GetScrapeResultsAndAlertJob(sm.GetHtmlDocumentsJob().Result);

            Console.ReadKey();
        }
    }
}
