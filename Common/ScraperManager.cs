using HtmlAgilityPack;
using JobsBgScraper.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace JobsBgScraper.Common
{
    public class ScraperManager
    {
        private static readonly CancellationTokenSource cancellationToken = new CancellationTokenSource();
        private static readonly ScraperConfig config = new ScraperConfig();
        private static readonly string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "../../../../Input/scraperConfig.json";

        /// <summary>
        /// A method to return all HTML content from the required web pages by using URIs from <see cref="ScraperConfig.JobSiteUrls"/> <br></br>
        /// Also sets the <see cref="ScraperConfig.MaxPageCount"/>
        /// </summary>
        /// <returns> All HTML code from web pages from <see cref="ScraperConfig.JobSiteUrls"/></returns>
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
            var tasks = new List<Task<HtmlDocument>>();

            // First probe on the site's uri, we want to find out how many pages we can scan
            config.MaxPageCount = await GetMaxPageCountOnSiteProbe();

            foreach (var site in config.JobSiteUrls)
            {
                tasks.Add(web.LoadFromWebAsync(site));
                cancellationToken.Token.ThrowIfCancellationRequested();
            }

            docs.AddRange(await Task.WhenAll(tasks));
            await SetScraperConfig();

            return docs;
        }

        /// <summary>
        /// The Main Filter Method that performs all required filtering operations <br></br> Compiles the requested HTML documents with the help from multiple Helper methods <br></br>
        /// </summary>
        /// <param name="documents">HTML Documents, used from <see cref="GetHtmlDocumentsJob"/></param>
        /// <exception cref="ArgumentNullException">The HTML Documents could not be found, check <see cref="GetHtmlDocumentsJob"/></exception>
        public void GetScrapeResultsAndAlertJob(IEnumerable<HtmlDocument> documents)
        {
            if (documents is null)
            {
                return;
            }

            var classNodes = new List<JobNode>();

            Parallel.ForEach(documents, (document) =>
            {
                var positionNodes = document.DocumentNode
                    .SelectNodes($"//*[contains(@class, '{GlobalConstants.HTML_JOB_CLASS_NAME}')]");

                var companies = document.DocumentNode
                    .SelectNodes($"../../td/a[contains(@class, '{GlobalConstants.HTML_COMPANY_CLASS_NAME}')]");

                var currentPageString = document.DocumentNode
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
                                    FindCompanyAndFormat(node, currentPageString, position, classNodes);
                                }
                                else
                                {
                                    foreach (var secondTerm in config.SecondConditionalJobKeyWords)
                                    {
                                        if (position.Contains(secondTerm.ToLower()))
                                        {
                                            FindCompanyAndFormat(node, currentPageString, position, classNodes);
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
                                FindCompanyAndFormat(node, currentPageString, position, classNodes);
                            }
                        }
                    }
                    else
                    {
                        FindCompanyAndFormat(node, currentPageString, position, classNodes);
                    }
                }
            });

            PrintResultsJob(classNodes);
        }

        #region Helpers
        /// <summary>
        /// Invoked when a job position within the parameters has been found<br></br>Selects and formats the company text, current page and adds it to the JobNode List
        /// </summary>
        /// <param name="node">The HTML document, to get the company text from</param>
        /// <param name="currentPage">The current page the iteration is on, in string format</param>
        /// <param name="position">The job position text</param>
        /// <param name="classNodes">The JobNode list, that contains all found job positions in class format</param>
        private void FindCompanyAndFormat(HtmlNode node, string currentPage, string position, List<JobNode> classNodes)
        {
            var company = node
                .SelectNodes($"../../td/a[contains(@class, '{GlobalConstants.HTML_COMPANY_CLASS_NAME}')]")
                [0].InnerText;

            int.TryParse(currentPage, out var currentPageInt);

            classNodes.Add(new JobNode(position, company, currentPageInt));
        }

        /// <summary>
        /// An initial probe on the site, to find out the Max Page Count, which we can then use to automatically scan all job positions
        /// </summary>
        /// <returns>The Max Page Number of the Web Page, per the URL parameters, pointed from <see cref="ScraperConfig.JobSiteUrls"/></returns>
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

        private async Task SetScraperConfig()
        {
            var jsonFileConfig = JsonSerializer.Deserialize<ScraperJsonConfig>(await File.ReadAllTextAsync(path), new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            config.FirstConditionalJobKeyWords = jsonFileConfig.FirstConditionalKeyWords;
            config.SecondConditionalJobKeyWords = jsonFileConfig.SecondConditionalKeyWords;
            config.SelectedLocation = jsonFileConfig.Location switch
            {
                "plovdiv" => (int)Locations.Plovdiv,
                "sofia" => (int)Locations.Sofia,
                "varna" => (int)Locations.Varna,
                "burgas" => (int)Locations.Burgas,
                _ => (int)Locations.Plovdiv
            };
        }

        /// <summary>
        /// Performs a check if <see cref="ScraperConfig"/> values are not null
        /// </summary>
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
        /// Proper Formatting and Configuration to String with <paramref name="collection"/>
        /// </summary>
        /// <param name="collection">List of the Collected Job Node Items</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="collection"/> is not configured properly</exception>
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
                sb.Append("\nThere are no available jobs with matching criteria");
                return sb.ToString();
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

        /// <summary>
        /// Printing to Console, uses <see cref="ResultsToStringJob(List{JobNode})"/>
        /// </summary>
        /// <param name="collection">List of the Collected Job Nodes</param>
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

        #endregion
    }
}
