namespace View_Selection_Algorithms.Service.Measure
{
    public class ExcelWriter
    {
        public void CreateBenchmarkFile(List<Tuple<int, string, double, double, double, double, string>> results)
        {
            //Path to Output folder
            var outputFolderPath = AppDomain.CurrentDomain.BaseDirectory.Split("bin")[0] + "Output\\";

            if (!Directory.Exists(outputFolderPath))
            {
                Directory.CreateDirectory(outputFolderPath);
            }
            var csvFilePathBenchmark = Path.Combine(outputFolderPath, "benchmark.csv");
            //write to csv
            using (var writer = new StreamWriter(csvFilePathBenchmark))
            {

                writer.WriteLine("QueryNumber:Materialized Views:QueryProcessingCosts:QueryProcessingTime in ms:Maintenance Costs:Storage Costs:Algorithm");

                foreach(var row in results)
                {
                    writer.WriteLine($"{row.Item1}:{row.Item2}:{row.Item3}:{row.Item4}:{row.Item5}:{row.Item6}:{row.Item7}");
                }
            }
            }
    }
}
