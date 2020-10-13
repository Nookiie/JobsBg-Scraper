using System;
using JobsBgScraper.Common;

namespace JobsBgScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            ScraperManager.GetScrapeResultsAndAlertJob(ScraperManager.GetHtmlDocumentsJob().Result);

            Console.ReadKey();
        }
    }
}
