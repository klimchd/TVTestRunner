using Microsoft.Extensions.Configuration;

namespace TVTestRunner
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
           .Build();

            var inputs = config.GetSection("Inputs").Get<List<Input>>();
            var delayBetweenTestsMs = config.GetSection("DelayBetweenTestsMs").Get<int>();
            Console.WriteLine("Loaded Inputs from appsettings.json:");
            foreach (var input in inputs)
            {
                Console.WriteLine(input);
                //Console.WriteLine($"Min: {input.Min}, Max: {input.Max}, Step: {input.Step}");
                //Console.WriteLine("Values: " + string.Join(", ", input.Values ?? input.GenerateValues()));
            }

            var inputValues = inputs.ToDictionary(input => input.Name, input => input.Values ?? input.GenerateValues());
            var totalCombinations = inputValues.Values.Select(arr => arr.Length).Aggregate(1, (a, b) => a * b);
            Console.WriteLine($"Total number of combinations: {totalCombinations}");

            var tr = new TestRunner();
            await tr.Init(delayBetweenTestsMs);
            await tr.Run(inputValues);
        }
    }
}