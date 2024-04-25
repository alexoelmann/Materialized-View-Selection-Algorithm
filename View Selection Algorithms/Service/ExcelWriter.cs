namespace View_Selection_Algorithms.Service
{
    public class ExcelWriter
    {
        public void CreateBenchmarkFile(List<Tuple<int, string, double, double, double, double, string>> results, TimeSpan executionTimeDeterministic, TimeSpan executionTimeHybrid)
        {
            //Path to Output folder
            var outputFolderPath = AppDomain.CurrentDomain.BaseDirectory.Split("bin")[0] + "Output\\";

            if (!Directory.Exists(outputFolderPath))
            {
                Directory.CreateDirectory(outputFolderPath);
            }
            var csvFilePathBenchmark = Path.Combine(outputFolderPath, "benchmarkTest.csv");
            //write to csv
            using (var writer = new StreamWriter(csvFilePathBenchmark))
            {

                writer.WriteLine("QueryNumber:Materialized Views:QueryProcessingCosts:QueryProcessingTime in ms:Maintenance Costs:Storage Costs:TimeSpan:Algorithm");

                foreach (var row in results)
                {
                    if (row.Item7 == "Deterministic MVPP")
                    {
                        writer.WriteLine($"{row.Item1}:{row.Item2}:{row.Item3}:{row.Item4}:{row.Item5}:{row.Item6}:{executionTimeDeterministic.TotalSeconds}:{row.Item7}");
                        continue;
                    }
                    if (row.Item7 == "Hybrid MVPP")
                    {
                        writer.WriteLine($"{row.Item1}:{row.Item2}:{row.Item3}:{row.Item4}:{row.Item5}:{row.Item6}:{executionTimeHybrid.TotalSeconds}:{row.Item7}");
                        continue;
                    }
                    else
                    {
                        writer.WriteLine($"{row.Item1}:{row.Item2}:{row.Item3}:{row.Item4}:{row.Item5}:{row.Item6}:{TimeSpan.FromSeconds(0).TotalSeconds}:{row.Item7}");
                        continue;
                    }
                }
            }
        }
    }
}
