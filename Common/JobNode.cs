namespace JobsBgScraper.Common
{
    public class JobNode
    {
        public JobNode(string position, string company, int? pageFound)
        {
            this.Position = position;
            this.Company = company;
            this.PageFound = pageFound;
        }

        public JobNode(string position)
        {
            this.Position = position;
        }

        public int? PageFound { get; set; }

        public string Position { get; set; }

        public string Company { get; set; }
    }
}
