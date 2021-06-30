using System.Collections.Generic;

namespace JobsBgScraper.Entities
{
    public class ScraperJsonConfig
    {
        public string Url { get; set; }

        public string Location { get; set; }

        public IEnumerable<string> FirstConditionalKeyWords { get; set; }

        public IEnumerable<string> SecondConditionalKeyWords { get; set; }
    }
}
