using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using View_Selection_Algorithms.Data;
using View_Selection_Algorithms.Model;

namespace View_Selection_Algorithms.Service.MaterializedViewCreationLogic
{
    public class HybridMVPP
    {
        private DatabaseConnector connector = new DatabaseConnector();
        public Tuple<List<View>, List<string>, List<Query>> ChooseMaterializedViews(List<View> views, List<Query> queries, int amountgenerations)
        {
            var mvs = this._selectMaterializedViews(views, queries, amountgenerations);
            var result = new Tuple<List<View>, List<string>, List<Query>>(views, mvs, queries);
            return result;
        }
        private List<string> _selectMaterializedViews(List<View> views, List<Query> queries, int amountOfGenerations)
        {

            /* Genetic Algorithm  reference goldberg */

            // 1. Create overall search-space
            var viewOrder = this._createViewOrderString(views);
            // 2. Choose random popsize of n = amount of possible views
            var popSize = this._selectRandomLists(viewOrder.Count());
            this._materializeAllViews(viewOrder);

            // 3. Genetic algorithm generations with MaxGen = 3
            var maxGen = amountOfGenerations;
            // 4. final population
            var finalPopulation = new List<Tuple<List<int>, double>>();
            for (var i = 0; i < maxGen; i++)
            {
                // 4 Only calculate fitness for last generation and pick best one
                if (i == maxGen - 1)
                {
                    foreach (var individium in popSize)
                    {
                        var fitness = this._calculateFitness(viewOrder, individium, queries);
                        finalPopulation.Add(fitness);
                        break;
                    }
                }
                var newPopulation = new List<Tuple<List<int>, double>>();
                var newPopulationAfterTournamentSelection = new List<Tuple<List<int>, double>>();
                var newPopulationAfterCrossover = new List<Tuple<List<int>, double>>();
                // 3.1 Calculate fitness für each individium inthe current population
                foreach (var individium in popSize)
                {
                    var fitness = this._calculateFitness(viewOrder, individium, queries);
                    newPopulation.Add(fitness);
                }
                // 3.2 Tournament selection of size = 4
                for (var j = 0; j < newPopulation.Count; j++)
                {
                    var winner = this._tournamentSelection(newPopulation, 4);
                    newPopulationAfterTournamentSelection.Add(winner);
                }
                // 3.3 Crossover
                var populationLength = 0;
                if (newPopulationAfterTournamentSelection.Count() % 2 == 0)
                {
                    populationLength = newPopulationAfterTournamentSelection.Count() / 2;
                }
                else
                {
                    populationLength = newPopulationAfterTournamentSelection.Count() / 2 + 1;
                }
                for (var j = 0; j < populationLength; j++)
                {
                    newPopulationAfterCrossover.AddRange(this._crossover(newPopulationAfterTournamentSelection, viewOrder.Count()));
                }
                // 3.4 Mutation
                var mutatedList = this._mutation(newPopulationAfterCrossover);
                popSize = mutatedList.Select(x => x.Item1).ToList();

                Console.WriteLine($"Generation: {i}");
            }
            // 5 Select best configuration out of max generation
            var winnerOfLastGen = finalPopulation.OrderBy(x => x.Item2).Select(x => x.Item1).ToList().First();
            var winnerViews = new List<string>();
            for (var i = 0; i < winnerOfLastGen.Count(); i++)
            {
                if (winnerOfLastGen[i] == 1)
                {
                    winnerViews.Add(views[i].Name);
                }
            }
            this._dropAllViews(viewOrder);
            return winnerViews;
        }
        /* Mutation */
        private List<Tuple<List<int>, double>> _mutation(List<Tuple<List<int>, double>> population)
        {
            var result = new List<Tuple<List<int>, double>>();
            foreach (var individium in population)
            {
                var mutatedQueryString = this._changeRandomElement(individium.Item1);
                result.Add(new(mutatedQueryString, individium.Item2));
            }
            return result;
        }
        /* Crossover */
        private List<Tuple<List<int>, double>> _crossover(List<Tuple<List<int>, double>> population, int viewLength)
        {
            var random = new Random();
            var result = new List<Tuple<List<int>, double>>();
            for (var i = 0; i < 2; i++)
            {
                var randomIndex = random.Next(population.Count);
                result.Add(population[randomIndex]);
            }

            var crossoverPoint = random.Next(1, viewLength);
            var child1 = new List<int>(result[0].Item1.GetRange(0, crossoverPoint));
            child1.AddRange(result[1].Item1.GetRange(crossoverPoint, result[1].Item1.Count() - crossoverPoint));

            var child2 = new List<int>(result[1].Item1.GetRange(0, crossoverPoint));
            child2.AddRange(result[0].Item1.GetRange(crossoverPoint, result[0].Item1.Count() - crossoverPoint));

            var newResult = new List<Tuple<List<int>, double>>();
            newResult.Add(new(child1, result[0].Item2));
            newResult.Add(new(child2, result[1].Item2));
            return newResult;
        }
        /* Tournament Selection */
        private Tuple<List<int>, double> _tournamentSelection(List<Tuple<List<int>, double>> population, int size)
        {
            var random = new Random();
            var tournamentContestants = new List<Tuple<List<int>, double>>();

            for (var i = 0; i < size; i++)
            {
                var randomIndex = random.Next(population.Count);
                tournamentContestants.Add(population[randomIndex]);
            }
            tournamentContestants.Sort((x, y) => x.Item2.CompareTo(y.Item2));

            return tournamentContestants[0];
        }
        /* Fitness function */
        private Tuple<List<int>, double> _calculateFitness(List<Tuple<string, string, int>> views, List<int> selectedMvs, List<Query> queries)
        {
            var baseQueries = new Queries().GetQueries();
            var mvs = new List<Tuple<string, string, int>>();
            var viewMaintenanceCosts = 0.0;

            for (var i = 0; i < views.Count(); i++)
            {
                if (selectedMvs[i] == 1)
                {

                    mvs.Add(views[i]);
                }
            }

            // Calculate viewMaintenance Costs
            for (var i = 0; i < mvs.Count(); i++)
            {
                if (mvs[i].Item1.Contains("result"))
                {
                    var queryNumber = mvs[i].Item1.Split("result")[1].Split("view")[0];
                    var queryFrequency = queries.Where(x => x.QueryNumber == int.Parse(queryNumber)).Select(x => x.QueryFrequency).ToList().First();
                    var viewDefinition = $"{mvs[i].Item2.Split("AS")[1].Trim()}";
                    var viewMaintenanceCost = connector.SQLQueryCostConnector(viewDefinition);
                    viewMaintenanceCosts += viewMaintenanceCost;
                }
                else
                {
                    var viewDefinition = $"{mvs[i].Item2.Split("AS")[1].Trim()}";
                    var viewMaintenanceCost = connector.SQLQueryCostConnector(viewDefinition);
                    viewMaintenanceCosts += viewMaintenanceCost;
                }
            }
            // Calculate Query Processing costs
            var sumQueryProcessingCosts = 0.0;
            mvs.Reverse();

            foreach (var baseQuery in baseQueries)
            {
                var newQuery = baseQuery.Item1;
                newQuery = newQuery.Replace("  ", " ");
                newQuery = newQuery.Replace("\r", "").Replace("\n", " ");
                var queryNumber = baseQuery.Item2;
                var queryFrequency = baseQuery.Item3;
                foreach (var mv in mvs)
                {

                    // View consists of the resultView for the query
                    if (mv.Item1.Contains("result"))
                    {
                        var resultViewNumber = int.Parse(mv.Item1.Split("result")[1].Split("view")[0]);
                        if (resultViewNumber == queryNumber)
                        {
                            newQuery = $"SELECT * FROM {mv.Item1}";

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
                    var tables = Regex.Replace(mv.Item1.Replace("view", "").Replace("result", ""), @"\d", "").Trim().Split("_").ToList();
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
                            newQuery = newQuery.Replace(table + ", ", mv.Item1 + ", ").Replace(table + " ", mv.Item1 + " ").Replace(table + ".", mv.Item1 + ".");
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
                            var newQueryTablesToString = String.Join(", ", newQueryTables);
                            var replacedFromPart = $" {newQueryTablesToString} ";
                            newQuery = newQuery.Replace(newQueryBaseRelations, replacedFromPart);
                        }
                    }
                }
                var queryProcessingCost = queryFrequency + connector.SQLQueryCostConnector(newQuery);
                sumQueryProcessingCosts += queryProcessingCost;
            }
            var fitness = sumQueryProcessingCosts + viewMaintenanceCosts;
            var result = new Tuple<List<int>, double>(selectedMvs, fitness);
            return result;

        }
        /* Helper Methods */
        private void _materializeAllViews(List<Tuple<string, string, int>> views)
        {
            for (var i = 0; i < views.Count(); i++)
            {

                var mvQuery = $"CREATE MATERIALIZED VIEW {views[i].Item1} AS {views[i].Item2.Split("AS")[1].Trim()}";
                connector.SQLQueryTableConnector(mvQuery);
            }
        }
        private void _dropAllViews(List<Tuple<string, string, int>> views)
        {
            views.Reverse();
            foreach (var view in views)
            {
                connector.SQLQueryTableConnector($"DROP MATERIALIZED VIEW IF EXISTS {view.Item1.ToLower()};");
            }
        }
        private List<int> _changeRandomElement(List<int> view)
        {
            var queries = new Queries().GetQueries().Count();
            var random = new Random();
            var randomIndex = random.Next(0, view.Count() - queries - 1);
            var oldValue = view[randomIndex];

            var modifiedView = new List<int>(view);

            if (oldValue == 0)
            {
                modifiedView[randomIndex] = 1;
            }
            else if (oldValue == 1)
            {
                modifiedView[randomIndex] = 0;
            }

            return modifiedView;
        }
        private List<List<int>> _selectRandomLists(int numberOfListsToSelect)
        {
            var queries = new Queries().GetQueries().Count();
            var result = new List<List<int>>();
            var random = new Random();

            for (var i = 0; i < numberOfListsToSelect; i++)
            {
                var binaryList = new List<int>();
                var onesCount = 0;

                for (var j = 0; j < numberOfListsToSelect; j++)
                {

                    var randomBinary = random.Next(2);

                    if (randomBinary == 1 && j <= (numberOfListsToSelect - queries))
                    {
                        binaryList.Add(randomBinary);
                        onesCount++;
                    }
                    else
                    {
                        binaryList.Add(0);
                    }
                }

                result.Add(binaryList);
            }

            return result;
        }
        private List<Tuple<string, string, int>> _createViewOrderString(List<View> views)
        {
            var result = new List<Tuple<string, string, int>>();
            var counter = 1;
            foreach (var view in views)
            {
                result.Add(new(view.Name, view.Definition, counter));
                counter++;
            }
            return result;

        }
    }
}
