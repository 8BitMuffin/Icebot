using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using HtmlAgilityPack;
using ScrapySharp.Network;
using ScrapySharp.Extensions;
using System.Reflection;

namespace IceBot
{
    class LegionTDService
    {
        private static string applicationName = "Icebot";
        private static string apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY", EnvironmentVariableTarget.User);
        private static SheetsService service;
        private static SpreadsheetsResource.ValuesResource.GetRequest request;
        public static readonly string[] AllowedCommands = { "lvlinfo", "stats" };
        private string _command;
        public string Command
        {
            get
            {
                return _command;
            }
            private set
            {
                switch (value)
                {
                    case "lvlinfo": _command = "GetLevelInfo"; break;
                    case "stats": _command = "GetPlayerStats"; break;
                }
            }
        }
        public object Parameter { get; set; }
        public delegate Task<string[]> LtdServiceHandler();
        public LtdServiceHandler GetData;
        public LegionTDService(string command, string parameter)
        {
            // Initialize configuration variables
            service = new SheetsService(new BaseClientService.Initializer()
            {
                ApplicationName = applicationName,
                ApiKey = apiKey,
            });
            Command = command;
            Parameter = parameter;
            GetData = (LtdServiceHandler)Delegate.CreateDelegate(typeof(LtdServiceHandler), this, Command);
        }

        public async Task<string[]> GetLevelInfo()
        {
            // Define request parameters.
            string spreadsheetId = "12SP0TPb1ih6QZyRUav9Ed8cSNtcc10EGMt7KIGDDhQ0";
            string range = "Levels!A3:F34";
            request = service.Spreadsheets.Values.Get(spreadsheetId, range);
            ValueRange response = await request.ExecuteAsync();
            IList<IList<object>> values = response.Values;
            if (values != null && values.Count > 0)
            {
                Console.WriteLine("legion td level info");
                foreach (var row in values)
                {

                    Console.WriteLine(String.Join<object>(" ", row));

                }
            }
            else
            {
                Console.WriteLine("No data found.");
            }

            var result = values.Where(row => (string)row[0] == (string)this.Parameter).SelectMany(row => row).Cast<string>().ToArray();
            var header = String.Format("{0,-10}{1,-20}{2,-10}{3,-10}{4,-15}{5,-10}\n", "Level", "Name", "Count", "Bounty", "CompletionG", "TotalG");
            var content = String.Format("{0,-10}{1,-20}{2,-10}{3,-10}{4,-15}{5,-10}", result[0], result[1], result[2], result[3], result[4], result[5]);

            return new string[] { header, content };

        }

        private async Task<string[]> GetPlayerStats()
        {
            Uri baseUri = new Uri("http://stats.onligamez.ru/");
            string queryString = $"u={this.Parameter}&s=w3arena&st=6";
            ScrapingBrowser browser = new ScrapingBrowser()
            {
                AllowAutoRedirect = true,
                AllowMetaRedirect = true,
            };
            WebPage page = await browser.NavigateToPageAsync(baseUri, HttpVerb.Get, queryString);
            var row = page.Html.CssSelect("div.panel-body .table tr").First();
            var values = new List<int>();

            foreach (var cell in row.CssSelect("td"))
            {
                string title = cell.CssSelect("b").First().InnerHtml;
                int value = Convert.ToInt32(cell.CssSelect("b").Last().InnerHtml.Replace(",", ""));
                values.Add(value);
            }

            Dictionary<string, int> stats = new Dictionary<string, int>()
            {
                {"Points", values[0] },
                {"Total Games", values[1] },
                {"Wins", values[2] },
                {"Losses", values[3] }
            };

            var header = String.Format("#{0}\n\n{1,-10}{2,-20}{3,-10}{4,-10}\n", this.Parameter, "Points", "Total Games", "Wins", "Losses");
            var content = String.Format("{0,-10}{1,-20}{2,-10}{3,-10}", stats["Points"], stats["Total Games"], stats["Wins"], stats["Losses"]);

            return new string[] { header, content };
        }

    }
}
