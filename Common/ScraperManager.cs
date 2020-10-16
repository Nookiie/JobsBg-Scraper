using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace JobsBgScraper.Common
{
    public class ScraperManager
    {
        private static readonly CancellationTokenSource cancellationToken = new CancellationTokenSource();
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

            // First probe on the site's uri, we want to find out how many pages we can scan
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
                var positionNodes = document.DocumentNode
                    .SelectNodes($"//*[contains(@class, '{GlobalConstants.HTML_JOB_CLASS_NAME}')]");

                var companyNodes = document.DocumentNode
                    .SelectNodes($"//*[contains(@class, '{GlobalConstants.HTML_COMPANY_CLASS_NAME}')]");

                var currentPage = document.DocumentNode
                    .SelectSingleNode($"//*[contains(@class, '{GlobalConstants.HTML_PAGE_LINK_CURRENT_CLASS_NAME}')]")
                    .InnerText
                    .Replace("[", "")
                    .Replace("]", "");

                foreach (var node in positionNodes)
                {
                    var position = node.InnerText.ToLower();

                    if (config.FirstConditionalJobKeyWords.Any())
                    {
                        foreach (var firstTerm in config.FirstConditionalJobKeyWords)
                        {
                            if (position.Contains(firstTerm.ToLower()))
                            {
                                if (!config.SecondConditionalJobKeyWords.Any())
                                {
                                    FindCompanyAndFormat(node, currentPage, position, classNodes);
                                }
                                else
                                {
                                    foreach (var secondTerm in config.SecondConditionalJobKeyWords)
                                    {
                                        if (position.Contains(secondTerm.ToLower()))
                                        {
                                            FindCompanyAndFormat(node, currentPage, position, classNodes);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    else if (config.SecondConditionalJobKeyWords.Any())
                    {
                        foreach (var secondTerm in config.SecondConditionalJobKeyWords)
                        {
                            if (position.Contains(secondTerm.ToLower()))
                            {
                                FindCompanyAndFormat(node, currentPage, position, classNodes);
                            }
                        }
                    }

                    else
                    {
                        FindCompanyAndFormat(node, currentPage, position, classNodes);
                    }
                }
            }

            PrintResultsJob(classNodes);
        }

        #region Helpers

        private void FindCompanyAndFormat(HtmlNode node, string currentPage, string position, List<JobNode> classNodes)
        {
            var companyNode = node
                .SelectNodes($"../../td/a[contains(@class, '{GlobalConstants.HTML_COMPANY_CLASS_NAME}')]");
            var company = companyNode[0].InnerText;

            int.TryParse(currentPage, out var currentPageInt);

            classNodes.Add(new JobNode(position, company, currentPageInt));
        }

        /// <summary>
        /// An initial probe on the site, 
        /// primarily done to check the MaxPageCount per the site's parameters
        /// </summary>
        /// <returns></returns>
        private async Task<int> GetMaxPageCountOnSiteProbe()
        {

            var uri = string.Format
                ($"https://www.jobs.bg/front_job_search.php?frompage=0&add_sh=1&categories%5B0%5D=15&location_sid={config.SelectedLocation}#paging");

            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(uri);
            var limit = doc
                .DocumentNode
                .SelectNodes($"//*[contains(@class, '{GlobalConstants.HTML_PAGE_LINK_CLASS_NAME}')]");

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

        /// <summary>
        /// Proper Formatting and printing to Console
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        private string ResultsToStringJob(List<JobNode> collection)
        {
            var sb = new StringBuilder();

            sb.Append("Languages:");

            if (!config.FirstConditionalJobKeyWords.Any())
            {
                sb.Append(" none");
            }
            else
            {
                foreach (var keyword in config.FirstConditionalJobKeyWords)
                {
                    sb.Append($" {keyword},");
                }
            }

            sb.Append("\nPositions:");

            if (!config.SecondConditionalJobKeyWords.Any())
            {
                sb.Append(" none");
            }
            else
            {
                foreach (var keyword in config.SecondConditionalJobKeyWords)
                {
                    sb.Append($" {keyword},");
                }
            }

            var location = Enum
                .GetName(typeof(Locations), config.SelectedLocation)
                .ToLower();

            sb.Append($"\nLocation: {location}");

            sb.Append("\n");

            if (collection.Count == 0)
            {
                return string.Format("There are no available jobs with matching criteria");
            }

            var text = string.Format($"Number of available jobs with matching criteria: {collection.Count} \n");

            sb.Append(text);
            sb.Append("\n\n");

            foreach (var item in collection)
            {
                sb.Append($"{item.Position}, {item.Company} on page {item.PageFound} \n");
            }

            return sb.ToString();
        }

        private void PrintResultsJob(List<JobNode> collection)
        {
            if (collection is null)
            {
                Console.WriteLine("Invalid Config Parameters");
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
