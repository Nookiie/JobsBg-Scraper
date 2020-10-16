using AngleSharp.Common;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace JobsBgScraper.Common
{
    // Parameters are located here!

    /// <summary>
    /// Custom Parameters for the ScraperManager
    /// </summary>
    public class ScraperConfig
    {
        private static readonly int DEFAULT_ITEM_COUNT_PER_PAGE = 15;

        // Only change values within this region
        #region Editable

        // Programming Language Search Parameters
        public IEnumerable<string> FirstConditionalJobKeyWords { get; } = new List<string>()
        {"c#", ".net"};

        // Position Level Search Parameters
        public IEnumerable<string> SecondConditionalJobKeyWords { get; } = new List<string>()
        {"intern", "senior"};

        // Location to analyse jobs in
        public int SelectedLocation { get; set; } = (int)Locations.Sofia;

        #endregion

        public int MaxPageCount { get; set; }
        public int ItemCountPerPage { get; set; } = DEFAULT_ITEM_COUNT_PER_PAGE;
        public int MaxItemCountOnJobsBg
        {
            get => MaxPageCount * ItemCountPerPage;
        }

        // Automatically generates all page iterations of the jobs.bg domain per the parameters above
        public IEnumerable<string> JobSiteUrls
        {
            get
            {
                for (var counter = 0; counter < MaxItemCountOnJobsBg; counter += ItemCountPerPage)
                {
                    yield return string.Format
                        ($"https://www.jobs.bg/front_job_search.php?frompage={counter}&add_sh=1&categories%5B0%5D=15&location_sid={SelectedLocation}#paging");
                }
            }
            set { }
        }
    }
}
