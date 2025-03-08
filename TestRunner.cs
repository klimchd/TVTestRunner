using Microsoft.Playwright;
using System.Diagnostics;

namespace TVTestRunner
{
    internal class TestRunner
    {
        private int minIterationTimeMs;

        private IPage page;
        private StreamWriter writer = new StreamWriter($"{DateTime.UtcNow.ToString("yyyyMMdd_HHmmss")}-results.csv");
        private List<string> resultLables = new List<string> { "Total P&L", "Max equity drawdown", "Total trades", "Profitable trades", "Profit factor" };

        public async Task Init(int minIterationTimeMs)
        {
            this.minIterationTimeMs = minIterationTimeMs;

            Console.WriteLine("Connecting to the Chrome instance...");
            try
            {
                var pl = await Playwright.CreateAsync();
                var browser = await pl.Chromium.ConnectOverCDPAsync("http://localhost:9222");
                Console.WriteLine("Connected.");

                foreach (var ctx in browser.Contexts)
                {
                    page = ctx.Pages?.FirstOrDefault(p => p.Url.Contains("www.tradingview.com"));
                    if (page != null)
                    {
                        break;
                    }
                }
            }
            catch (PlaywrightException ex)
            {
                Console.WriteLine("Couldn't connect to Chrome. Make sure you are running Chrome with parameter: --remote-debugging-port=9222");
                Console.WriteLine(ex.Message);
            }

        }

        public async Task Run(Dictionary<string, double[]> inputValues)
        {
            if (page == null)
            {
                Console.WriteLine("No tradingview page found. Make sure it's opened in Chrome.");
                return;
            }

            Console.WriteLine("Found tradingview page:");
            Console.WriteLine(page.Url);

            string header = string.Join(',', inputValues.Keys.Concat(resultLables));
            Console.WriteLine(header);
            writer.WriteLine(header);
            writer.Flush();
            // Generate and pass all combinations
            await GenerateCombinations(inputValues, new Dictionary<string, double>(), inputValues.Keys.ToList());
            writer.Close();
            writer.Dispose();
        }

        async Task GenerateCombinations(Dictionary<string, double[]> inputDict, Dictionary<string, double> current, List<string> keys, int index = 0)
        {
            if (index == keys.Count)
            {
                await RunIteration(new Dictionary<string, double>(current));
                return;
            }

            string key = keys[index];
            foreach (var value in inputDict[key])
            {
                current[key] = value;
                await GenerateCombinations(inputDict, current, keys, index + 1);
            }
        }

        public async Task RunIteration(Dictionary<string, double> inputs)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            var bottomArea = page.Locator("#bottom-area");

            var strategyReportMenu = bottomArea.Locator("button[data-strategy-title$='report']");
            //handle the case when strategy tester pane is minimized
            //if (await strategyDrop.IsHiddenAsync())
            //{
            //    await page.Locator("[data-name='backtesting']").ClickAsync();
            //    //await page.GetByRole(AriaRole.Button, new() { Name = "Open Strategy Tester" }).ClickAsync();
            //}

            await strategyReportMenu.ClickAsync();

            var settingsBtn = page.GetByRole(AriaRole.Menuitem, new() { Name = "Settings…" }).First;
            await settingsBtn.ClickAsync();


            var settingsDialog = page.GetByRole(AriaRole.Dialog);
            foreach (var kv in inputs)
            {
                await FillInput(settingsDialog, kv);
            }

            var okBtn = settingsDialog.Locator("button[data-name=\"submit-button\"]");
            await okBtn.ClickAsync();

            var generateBtn = bottomArea.GetByRole(AriaRole.Button, new() { Name = "Generate report" });
            if (await generateBtn.IsEnabledAsync())
            {
                await generateBtn.ClickAsync();
            }

            var results = new List<string>();
            foreach (var label in resultLables)
            {
                var result = await bottomArea.Locator($"//div[contains(text(), '{label}')]/../following-sibling::div[1]/div[1]").TextContentAsync();
                //a coma in values would break csv format
                results.Add(result.Replace(",", ""));
            }

            var resultsRow = string.Join(',', inputs.Values) + "," + string.Join(",", results);
            Console.WriteLine(resultsRow);
            writer.WriteLine(resultsRow);
            writer.Flush();

            stopwatch.Stop();
            Console.WriteLine($"{DateTime.Now.ToLongTimeString()}\t {stopwatch.ElapsedMilliseconds} ms");
            // checking if the iteration was too fast and need to wait to avoid TV ban
            var delayMs = minIterationTimeMs - (int)stopwatch.ElapsedMilliseconds;
            if (delayMs > 0)
            {
                await Task.Delay(delayMs);
            }
        }

        private static async Task FillInput(ILocator settingsDialog, KeyValuePair<string, double> input)
        {
            await settingsDialog.Locator($"css=div:has(> :text('{input.Key}')) + div input").FillAsync(input.Value.ToString());
        }
    }
}
