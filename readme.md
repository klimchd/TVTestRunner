## Tradingview auto backtesting
The app allows to specify values for inputs of a strategy and run all possible combinations.
The results written to .csv file, that can be reviewed in Excel or Google sheets.

### Usage:
1. Create Chrome shortcut with added  `--remote-debugging-port=9222` after the path
    i.e. "C:\Program Files\Google\Chrome\Application\chrome.exe" --remote-debugging-port=9222
2. Close all Chrome instances and run this shortcut. This opens Chrome in debug mode that allows to connect to it's tabs. 
3. Open tradingview page (for example https://www.tradingview.com/chart/XoBl9mTZ/) and select required coin, strategy and date range. The script will take the first tradingview tab, so open only one such tab.
4. In the application folder edit `appsettings.json` file. Here you need specify the exact names of the inputs and it's ranges or exact values. The values should be numeric.
5. Run TVTestRunner.exe in the console. It should readd the inputs, connect to the browser and run tests for all combinations. 