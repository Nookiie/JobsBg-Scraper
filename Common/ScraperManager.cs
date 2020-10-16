using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JobsBgScraper.Common
{
    public class ScraperManager
    {
        private readonly CancellationTokenSource cancellationToken = new CancellationTokenSource();
        private static readonly ScraperConfig config = new ScraperConfig();

        public async Task<IEnumerable<HtmlDocument>> GetHtmlDocumentsJob()
        {
            if (!IsScraperConfigValid())
            {
                Console.WriteLine("Operation aborted!");
                return null;
            }

            cancellationToken.Token.ThrowIfCancellationRequested();
            var web = new HtmlWeb();
            var docs = new List<HtmlDocument>();

            config.MaxPageCount = await GetMaxPageCountOnSiteProbe();

            foreach (var site in config.JobSiteUrls)
            {
                docs.Add(await web.LoadFromWebAsync(site));
                cancellationToken.Token.ThrowIfCancellationRequested();
            }

            return docs;
        }

        public void GetScrapeResultsAndAlertJob(IEnumerable<HtmlDocument> documents)
        {
            if (documents is null)
            {
                return;
            }

            var classNodes = new List<JobNode>();
            foreach (var document in documents)
            {
                var positionNodes = document.DocumentNode.SelectNodes("//*[contains(@class, 'joblink')]");
                var companyNodes = document.DocumentNode.SelectNodes("//*[contains(@class, 'company_link')]");

                foreach (var node in positionNodes)
                {
                    var position = node.InnerText.ToLower();
                    string company = null;

                    foreach (var firstTerm in config.FirstConditionalJobKeyWords)
                    {
                        if (position.Contains(firstTerm.ToLower()))
                        {
                            foreach (var secondTerm in config.SecondConditionalJobKeyWords)
                            {
                                if (position.Contains(secondTerm.ToLower()))
                                {
                                    var companyNode = node.SelectNodes("../../td/a[contains(@class, 'company_link')]");
                                    company = companyNode[0].InnerText;

                                    FormatNodesJob(position, company, classNodes);
                                }
                            }
                        }
                    }
                }
            }

            PrintResultsJob(classNodes);
            // SaveAsJSON(classNodes);
        }

        #region Helpers

        private async Task<int> GetMaxPageCountOnSiteProbe()
        {
            var url = string.Format
                ($"https://www.jobs.bg/front_job_search.php?frompage=0&add_sh=1&categories%5B0%5D=15&location_sid={config.SelectedLocation}#paging");

            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url);
            var limit = doc.DocumentNode.SelectNodes("//*[contains(@class, 'pathlink')]");

            return int.Parse(limit[limit.Count() - 2].InnerText);
        }

        private bool IsScraperConfigValid()
        {
            if (config.JobSiteUrls is null)
            {
                Console.WriteLine("No JobSite URLs detected.");
                return false;
            }

            return true;
        }

        private void FormatNodesJob(string position, string company, List<JobNode> classNodes)
        {
            classNodes.Add(new JobNode(position, company));
        }

        private string ResultsToStringJob(List<JobNode> collection)
        {
            if (collection.Count == 0)
            {
                return string.Format("There are no available jobs with matching criteria");
            }

            var text = string.Format($"Number of available jobs with matching criteria: {collection.Count} \n");
            var sb = new StringBuilder();

            sb.Append(text);
            sb.Append("\n\n");

            foreach (var item in collection)
            {
                sb.Append($"{item.Position}, {item.Company} \n");
            }

            return sb.ToString();
        }

        private void PrintResultsJob(List<JobNode> collection)
        {
            if (collection is null)
            {
                Console.WriteLine("Invalid Helper Parameters");
                return;
            }

            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine(ResultsToStringJob(collection));
        }

        private void ImportSettingsJob()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Unused functionality of saving history of results from findings
        /// </summary>
        /// <param name="classNodes"></param>
        private void SaveAsJSONJob(List<JobNode> classNodes)
        {
            string json = JsonConvert.SerializeObject(classNodes.ToArray());

            File.WriteAllText(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + "/History/history.json", json);
        }

        #endregion
    }
}
