using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace JobsBgScraper.Common
{
    // Parameters are located here!

    /// <summary>
    /// Custom Parameters for the ScraperManager
    /// </summary>
    public static class ScraperHelpers
    {
       
        // Programming Language Search Parameters
        public static IEnumerable<string> FirstConditionalJobKeyWords { get; } = new List<string>()
        {"c#", ".net"};

        // Position Level Search Parameters
        public static IEnumerable<string> SecondConditionalJobKeyWords { get; } = new List<string>()
        {"intern", "junior"};

        private static readonly int maxPageCount = 10;
        private static readonly int itemCountPerPage = 15;

        public static int MaxPageCount
        {
            get => maxPageCount >= 0 ? maxPageCount : 0;
        }

        public static int ItemCountPerPage
        {
            get => itemCountPerPage >= 0 ? itemCountPerPage : 0;
        }

        public static int MaxItemCountOnJobsBg
        {
            get => MaxPageCount * ItemCountPerPage;
        }

        // Automatically generates all page clones of the jobs.bg domain per the parameters above
        public static IEnumerable<string> JobSiteUrls
        {
            get
            {
                for (var counter = 0; counter < MaxItemCountOnJobsBg; counter += ItemCountPerPage)
                {
                    yield return string.Format
                        ($"https://www.jobs.bg/front_job_search.php?frompage={counter}&add_sh=1&categories%5B0%5D=15&location_sid=2#paging");
                }
            }
        }
    }
}
