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
    /// Custom Parameters in use for the <see cref="ScraperManager"/>
    /// </summary>
    public class ScraperConfig
    {
        // Only change values within this region
        #region Editable

        // Programming Language Search Parameters
        public IEnumerable<string> FirstConditionalJobKeyWords { get; } = new List<string>()
        {};

        // Position Level Search Parameters
        public IEnumerable<string> SecondConditionalJobKeyWords { get; } = new List<string>()
        {"intern", "trainee", "junior"};

        // Location to analyse jobs in
        public int SelectedLocation { get; set; } = (int)Locations.Plovdiv;

        #endregion

        public int MaxPageCount { get; set; }
        public int ItemCountPerPage { get; set; } = GlobalConstants.DEFAULT_ITEM_COUNT_PER_PAGE;
        public int MaxItemCountOnJobsBg
        {
            get => MaxPageCount * ItemCountPerPage;
        }

        ///<summary> Automatically generates all page iterations of the jobs.bg domain per public parameters 
        ///from <see cref="ScraperConfig"/> </summary>
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
        }
    }
}
