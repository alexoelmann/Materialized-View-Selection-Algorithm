using View_Selection_Algorithms.Model;
using View_Selection_Algorithms.Service.MaterializedViewCreationLogic;
using View_Selection_Algorithms.Service.Measure;
using View_Selection_Algorithms.Service.QueryParsingLogic;

namespace View_Selection_Algorithms.MVPPMainLogic
{
    public class MainLogic
    {
        public void Sequence()
        {
            Console.WriteLine("Start query parsing");
            var parser = new QueryParser();
            var extractedQueryParts = parser.ExtractAllQueryParts();
            Console.WriteLine("Start generating views");
            var viewCreator = new ViewCreator();
            var views = viewCreator.GenerateAllViews(extractedQueryParts);
            Console.WriteLine("Start deterministic MVPP");
            var deterministicMCreator = new DeterministicMVPPCreator();
            var deterministicMvs=deterministicMCreator.ChooseMaterializedViews(views,extractedQueryParts);
            Console.WriteLine("Start hybrid MVPP");
            var hybridMVPP = new HybridMVPP();
            var hybridMvs=hybridMVPP.ChooseMaterializedViews(views, extractedQueryParts);
            Console.WriteLine("Start calculating Measures");
            var measureCalculator = new MeasureCalculator();

            var benchmarkResult = new List<Tuple<int, string, double, double, double, double, string>>();

            // No Algorithm
            var item1No=new Tuple<List<string>, List<View>, string>(null, null, "No Algorithm");
            var item2No = deterministicMvs.Item3;
            benchmarkResult.AddRange(measureCalculator.CalculateMeasures(item1No,item2No));
            // All Queries Materialized
            var item1All = new Tuple<List<string>, List<View>, string>(null, null, "All Queries Materialized");
            var item2All = deterministicMvs.Item3;
            benchmarkResult.AddRange(measureCalculator.CalculateMeasures(item1All, item2All));
            // Deterministic
            var item1Deterministic = new Tuple<List<string>, List<View>, string>(deterministicMvs.Item1, deterministicMvs.Item2, "Deterministic MVPP");
            var item2Deterministic = deterministicMvs.Item3;
            benchmarkResult.AddRange(measureCalculator.CalculateMeasures(item1Deterministic, item2Deterministic));
            // Hybrid
            var item1Hybrid = new Tuple<List<string>, List<View>, string>(hybridMvs.Item2, hybridMvs.Item1, "Hybrid MVPP");
            var item2Hybrid = hybridMvs.Item3;
            benchmarkResult.AddRange(measureCalculator.CalculateMeasures(item1Hybrid, item2Hybrid));

            //write to excel
            Console.WriteLine("Start writing to excel");
            var excelWriter = new ExcelWriter();
            excelWriter.CreateBenchmarkFile(benchmarkResult);
        }
    }
}
