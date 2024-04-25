using System.Text.RegularExpressions;
using View_Selection_Algorithms.Data;
using View_Selection_Algorithms.Model;
using View_Selection_Algorithms.Service;

namespace View_Selection_Algorithms.BusinessLogic
{
    public class MeasureCalculator
    {
        private DatabaseConnector connector = new DatabaseConnector();
        public List<Tuple<int, string, double, double, double, double, string>> CalculateMeasures(Tuple<List<string>, List<View>, string> materializedViews, List<Query> queries)
        {
            var result = new List<Tuple<int, string, double, double, double, double, string>>();
            if (materializedViews.Item3 == "No Algorithm")
            {
                result = _calculateMeasuresNoViews();
            }
            else if (materializedViews.Item3 == "All Queries Materialized")
            {
                result = _calculateMeasuresAllQueriesMaterialized();
            }
            else
            {
                result = _calculateMeasuresWithAlgorithm(materializedViews, queries);
            }
            return result;
        }
        private List<Tuple<int, string, double, double, double, double, string>> _calculateMeasuresWithAlgorithm(Tuple<List<string>, List<View>, string> materializedViews, List<Query> queries)
        {
            var baseQueries = new Queries().GetQueries();
            var result = new List<Tuple<int, string, double, double, double, double, string>>();
            var mvs = materializedViews.Item2.Where(x => materializedViews.Item1.Contains(x.Name)).ToList();

            // Materialize Views and calculate storageCosts
            var storageCosts = 0.0;
            foreach (var view in materializedViews.Item2)
            {
                //Materialized View
                if (mvs.Where(x => x.Name == view.Name).Any())
                {
                    var mvQuery = $"CREATE MATERIALIZED VIEW {view.Name} AS {view.Definition.Split("AS")[1].Trim()} WITH DATA";
                    connector.SQLQueryTableConnector(mvQuery);
                    storageCosts += connector.GetMVStorageCost(view.Name);
                }
                else
                {
                    var viewQuery = $"CREATE VIEW {view.Name} AS {view.Definition.Split("AS")[1].Trim()}";
                    connector.SQLQueryTableConnector(viewQuery);
                }
            }
            //ViewMaintenance
            var viewMaintenanceCosts = 0.0;
            for (var i = 0; i < mvs.Count(); i++)
            {
                if (mvs[i].Name.Contains("result"))
                {
                    var queryNumber = mvs[i].Name.Split("result")[1].Split("view")[0];
                    var queryFrequency = queries.Where(x => x.QueryNumber == int.Parse(queryNumber)).Select(x => x.QueryFrequency).ToList().First();
                    var viewDefinition = $"{mvs[i].Definition.Split("AS")[1].Trim()}";
                    var viewMaintenanceCost = connector.SQLQueryCostConnector(viewDefinition);
                    viewMaintenanceCosts += queryFrequency * viewMaintenanceCost;
                }
                else
                {
                    var tables = Regex.Replace(mvs[i].Name.Replace("view", "").Replace("result", ""), @"\d", "").Trim().Split("_").ToList();
                    var tablesCount = tables.Count();
                    // get the sum of all query frequencies this view uses
                    var sumOfQueryFrequencies = 0.0;
                    foreach (var query in queries)
                    {
                        var counter = 0;
                        foreach (var baseRelation in query.BaseRelation)
                        {
                            if (tables.Contains(baseRelation))
                            {
                                counter++;
                            }
                        }

                        if (counter == tablesCount)
                        {
                            sumOfQueryFrequencies += query.QueryFrequency;
                        }
                    }
                    var viewDefinition = $"{mvs[i].Definition.Split("AS")[1].Trim()}";
                    var viewMaintenanceCost = connector.SQLQueryCostConnector(viewDefinition);
                    viewMaintenanceCosts += sumOfQueryFrequencies * viewMaintenanceCost;
                }
            }
            //QueryProcessing Cost and speed

            foreach (var query in baseQueries)
            {
                var newQuery = query.Item1;
                newQuery = newQuery.Replace("  ", " ");
                newQuery = newQuery.Replace("\r", "").Replace("\n", " ");
                var queryNumber = query.Item2;
                var queryFrequency = query.Item3;
                foreach (var mv in mvs)
                {

                    // View consists of the resultView for the query
                    if (mv.Name.Contains("result"))
                    {
                        var resultViewNumber = int.Parse(mv.Name.Split("result")[1].Split("view")[0]);
                        if (resultViewNumber == queryNumber)
                        {
                            newQuery = $"SELECT * FROM {mv.Name}";

                            break;
                        }
                        // View consists of the resultView but not for the query
                        if (resultViewNumber != queryNumber)
                        {
                            continue;
                        }
                    }
                    // not resultView
                    var parsedQueries = queries.Where(x => x.QueryNumber == queryNumber).ToList().First();
                    var tables = Regex.Replace(mv.Name.Replace("view", "").Replace("result", ""), @"\d", "").Trim().Split("_").ToList();
                    var tablesCount = tables.Count();
                    var counter = 0;
                    foreach (var baseRelation in parsedQueries.BaseRelation)
                    {
                        if (tables.Contains(baseRelation))
                        {
                            counter++;
                        }
                    }
                    // if all tables of the views are inside the query then we can use it
                    if (counter == tablesCount)
                    {
                        foreach (var table in tables)
                        {
                            newQuery = newQuery.Replace(table + ", ", mv.Name + ", ").Replace(table + " ", mv.Name + " ").Replace(table + ".", mv.Name + ".");
                        }
                        //Error check if FROM-Clause has one table more than one time
                        var fromPartIndex = newQuery.IndexOf("FROM") + 4;
                        var queryEndIndex = newQuery.IndexOf("WHERE");

                        if (queryEndIndex == -1)
                        {
                            queryEndIndex = newQuery.IndexOf(";");
                        }
                        var newQueryBaseRelations = newQuery.Substring(fromPartIndex, queryEndIndex - fromPartIndex);
                        //More than one one tables
                        var newQueryTables = new List<string>();
                        if (newQueryBaseRelations.Contains(","))
                        {
                            newQueryTables = newQueryBaseRelations.Split(",").ToList().Select(x => x.Trim()).ToList().Distinct().ToList();
                            var newQueryTablesToString = string.Join(", ", newQueryTables);
                            var replacedFromPart = $" {newQueryTablesToString} ";
                            newQuery = newQuery.Replace(newQueryBaseRelations, replacedFromPart);
                        }
                    }
                }
                var queryProcessingCost = queryFrequency + connector.SQLQueryCostConnector(newQuery);
                var queryProcessingTime = queryFrequency + connector.SQLQueryTimeConnector(newQuery);
                result.Add(new(query.Item2, string.Join(", ", materializedViews.Item1), queryProcessingCost, queryProcessingTime, viewMaintenanceCosts, storageCosts, materializedViews.Item3));
            }
            //Drop views again
            materializedViews.Item2.Reverse();
            foreach (var view in materializedViews.Item2)
            {
                connector.SQLQueryTableConnector($"DROP VIEW IF EXISTS {view.Name.ToLower()};");
                connector.SQLQueryTableConnector($"DROP MATERIALIZED VIEW IF EXISTS {view.Name.ToLower()};");
            }
            materializedViews.Item2.Reverse();
            return result;

        }
        private List<Tuple<int, string, double, double, double, double, string>> _calculateMeasuresNoViews()
        {
            var baseQueries = new Queries().GetQueries();
            var result = new List<Tuple<int, string, double, double, double, double, string>>();

            foreach (var query in baseQueries)
            {
                var queryProcessingCost = connector.SQLQueryCostConnector(query.Item1);
                var queryNumber = query.Item2;
                var storageCost = 0.0;
                var viewMaintenanceCosts = 0.0;
                var queryProcessingTime = connector.SQLQueryTimeConnector(query.Item1);
                var mvs = "No";
                var algorithm = "No";
                result.Add(new(queryNumber, mvs, queryProcessingCost, queryProcessingTime, viewMaintenanceCosts, storageCost, algorithm));
            }
            return result;
        }
        private List<Tuple<int, string, double, double, double, double, string>> _calculateMeasuresAllQueriesMaterialized()
        {
            var baseQueries = new Queries().GetQueries();
            var result = new List<Tuple<int, string, double, double, double, double, string>>();

            foreach (var query in baseQueries)
            {
                var mvQuery = $"CREATE MATERIALIZED VIEW view{query.Item2} AS {query.Item1.Replace(";", "")} WITH DATA";
                connector.SQLQueryTableConnector(mvQuery);
                var queryProcessingCost = connector.SQLQueryCostConnector($"SELECT * FROM view{query.Item2}");
                var queryNumber = query.Item2;
                var storageCost = connector.GetMVStorageCost($"view{query.Item2}");
                var viewMaintenanceCosts = query.Item3 * connector.SQLQueryCostConnector($"{query.Item1}");
                var queryProcessingTime = connector.SQLQueryTimeConnector($"SELECT * FROM view{query.Item2}");
                var mvs = "All Queries";
                var algorithm = "All Queries Materialized";
                result.Add(new(queryNumber, mvs, queryProcessingCost, queryProcessingTime, viewMaintenanceCosts, storageCost, algorithm));
                connector.SQLQueryTableConnector($"DROP MATERIALIZED VIEW IF EXISTS  view{query.Item2};");
            }
            var sumStorageCost = 0.0;
            var sumViewMaintenanceCost = 0.0;
            foreach (var r in result)
            {
                sumStorageCost += r.Item6;
                sumViewMaintenanceCost += r.Item5;
            }
            var newResult = new List<Tuple<int, string, double, double, double, double, string>>();
            foreach (var r in result)
            {
                newResult.Add(new(r.Item1, r.Item2, r.Item3, r.Item4, sumViewMaintenanceCost, sumStorageCost, r.Item7));
            }
            result = newResult;
            return result;
        }
    }
}
