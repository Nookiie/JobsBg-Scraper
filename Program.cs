using System;
using JobsBgScraper.Common;

namespace JobsBgScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            ScraperManager sm = new ScraperManager();
            sm.GetScrapeResultsAndAlertJob(sm.GetHtmlDocumentsJob().Result);

            Console.ReadKey();
        }
    }
}
