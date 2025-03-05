namespace TVTestRunner
{
    public class Input
    {
        public string Name { get; set; }
        public double Max { get; set; }
        public double Min { get; set; }
        public double Step { get; set; }
        public double[] Values { get; set; }

        public double[] GenerateValues()
        {
            var list = new List<double>();
            for (double val = Min; val <= Max; val += Step)
            {
                list.Add(val);
            }

            return list.ToArray();
        }

        public override string? ToString()
        {
            return $"{Name}: {string.Join(", ", Values ?? GenerateValues())}";
        }
    }
}