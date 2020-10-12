using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace JobsBgScraper.Common
{
    // Parameters are located Here!

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


        public static readonly int MaxPageCount = 10;
        public static readonly int ItemCountPerPage = 15;
        public static readonly int MaxItemCountOnJobsBg = MaxPageCount * ItemCountPerPage;

        // Automatically generates all page clones of the jobs.bg domain per the parameters above
        public static IEnumerable<string> JobSiteUrls
        {
            get
            {
                var siteLists = new List<string>();

                for (var counter = 0; counter < MaxItemCountOnJobsBg; counter += ItemCountPerPage)
                {
                    siteLists.Add(string.Format
                        ($"https://www.jobs.bg/front_job_search.php?frompage={counter}&add_sh=1&categories%5B0%5D=15&location_sid=2#paging"));
                }

                return siteLists;
            }
        }
    }
}
