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
    public static class ScraperConfig
    {
        // Only change values within this region!
        #region Editable

        private static readonly int DEFAULT_MAX_PAGE_COUNT = 10;
        private static readonly int DEFAULT_ITEM_COUNT_PER_PAGE = 15;

        // Programming Language Search Parameters
        public static IEnumerable<string> FirstConditionalJobKeyWords { get; } = new List<string>()
        {"c#", ".net"};

        // Position Level Search Parameters
        public static IEnumerable<string> SecondConditionalJobKeyWords { get; } = new List<string>()
        {"intern", "junior"};

        #endregion

        public static int MaxPageCount
        {
            get => DEFAULT_MAX_PAGE_COUNT >= 0 ? DEFAULT_MAX_PAGE_COUNT : 0;
        }
        public static int ItemCountPerPage
        {
            get => DEFAULT_ITEM_COUNT_PER_PAGE >= 0 ? DEFAULT_ITEM_COUNT_PER_PAGE : 0;

        }
        public static int MaxItemCountOnJobsBg
        {
            get => MaxPageCount * ItemCountPerPage;
        }

        // Automatically generates all page iterations of the jobs.bg domain per the parameters above
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
