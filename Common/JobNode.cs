using System;
using System.Collections.Generic;
using System.Text;

namespace JobsBgScraper.Common
{
    public class JobNode
    {
        public JobNode(string position, string company)
        {
            this.Position = position;
            this.Company = company;
        }

        public JobNode(string position)
        {
            this.Position = position;
        }

        public string Position { get; set; }

        public string Company { get; set; }
    }
}
