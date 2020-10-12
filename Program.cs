using System;
using JobsBgScraper.Common;

namespace JobsBgScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            ScraperManager.GetScrapeResultsAndAlertJob(ScraperManager.ScrapeWebsiteJob().Result);

            Console.ReadKey();
        }
    }
}
