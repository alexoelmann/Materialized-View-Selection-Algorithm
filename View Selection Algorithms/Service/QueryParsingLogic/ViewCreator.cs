using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using View_Selection_Algorithms.Model;

namespace View_Selection_Algorithms.Service.QueryParsingLogic
{
    public class ViewCreator
    {
        private DatabaseConnector connector = new DatabaseConnector();
        public List<View> GenerateAllViews(List<Query> queries)
        {
            var result = new List<View>();
            var baseSelectionViews = this._createBaseSelectionViews(queries);
            var baseProjectionViews = this._createBaseProjectionViews(queries,baseSelectionViews);
            var joinViews = this._createJoinViews(queries,baseProjectionViews);
            var resultViews = this._createAllResultViews(queries,joinViews,baseProjectionViews);
            result.AddRange(baseSelectionViews);
            result.AddRange(baseProjectionViews);
            result.AddRange(joinViews);
            
            result = this._calculateQueryCostAndStorageCost(result,resultViews,baseSelectionViews,baseProjectionViews,joinViews);
            return result;
        }
        private List<View> _calculateQueryCostAndStorageCost(List<View> allViews, List<View> resultViews, List<View> baseSelectionViews, List<View> baseProjectionViews, List <View> joinViews)
        {
            var result = new List<View>();
            var viewsOrdered=allViews.OrderBy(x => x.Name.Length).ToList();
            viewsOrdered.AddRange(resultViews.OrderBy(x => x.Name.Length).ToList());

            var viewsOrderedReversed = resultViews.OrderByDescending(x => x.Name.Length).ToList();
            viewsOrderedReversed.AddRange(joinViews.OrderByDescending(x => x.Name.Length).ToList());
            viewsOrderedReversed.AddRange(baseProjectionViews.OrderByDescending(x => x.Name.Length).ToList());
            viewsOrderedReversed.AddRange(baseSelectionViews.OrderByDescending(x => x.Name.Length).ToList());

            foreach (var view in viewsOrdered)
            {
                
                var queryProcessingCost=connector.SQLQueryCostConnector(view.Definition.Split("AS")[1].Trim());
                var mvQuery = $"CREATE MATERIALIZED VIEW {view.Name} AS {view.Definition.Split("AS")[1].Trim()} WITH DATA";
                connector.SQLQueryTableConnector(mvQuery);
                var storageCost=connector.GetMVStorageCost(view.Name.ToLower());
                connector.SQLQueryTableConnector($"DROP MATERIALIZED VIEW IF EXISTS {view.Name.ToLower()};");
                var viewDefinition = $"CREATE VIEW {view.Name} AS {view.Definition.Split("AS")[1].Trim()}";
                connector.SQLQueryTableConnector(viewDefinition);
                var newView = new View(view.Name, view.Definition,queryProcessingCost,storageCost);
                result.Add(newView);

            }
            foreach (var view in viewsOrderedReversed)
            {
                connector.SQLQueryTableConnector($"DROP VIEW IF EXISTS {view.Name.ToLower()};");
            }
                return result;
        }
        private List<View> _createAllResultViews(List<Query> queries, List<View> joinViews,List<View> baseProjectionViews)
        {
            var resultViews = new List<View>();
            foreach (var query in queries)
            {
                //Query has no prior joinView
                if (!query.Join.Any())
                {
                    foreach(var projectionView in baseProjectionViews)
                    {
                        if (projectionView.Name.Split("2")[0].Trim() == query.BaseRelation.First())
                        {
                            var queryProjections = String.Join(" , ", query.QueryProjectionColumn.Select(x => x.Item2).ToList());
                            
                                queryProjections = queryProjections
                                     .Replace(query.BaseRelation.First() + ".", projectionView.Name + ".");
                            
                            var viewName = $"Result{query.QueryNumber}view";
                            var viewQuery = $"SELECT {queryProjections} FROM {projectionView.Name}";
                            var viewDefinition = $"CREATE VIEW {viewName} AS {viewQuery}";
                            var view = new View(viewName, viewDefinition, 0.0, 0.0);
                            resultViews.Add(view);
                        }
                    }
                }
                var tables = query.BaseRelation;
                tables.Sort();

                foreach(var join in joinViews)
                {
                    var joinViewtables = join.Name.Split("view")[0].Split("_").ToList();
                    joinViewtables.Sort();
                    if (joinViewtables.SequenceEqual(tables))
                    {
                        var queryProjections = String.Join(" , ", query.QueryProjectionColumn.Select(x => x.Item2).ToList());
                        foreach (var table in tables)
                        {
                           queryProjections = queryProjections
                                .Replace(table+".",join.Name+".");
                        }
                        var viewName = $"Result{query.QueryNumber}view";
                        var viewQuery = $"SELECT {queryProjections} FROM {join.Name}";
                        var viewDefinition = $"CREATE VIEW {viewName} AS {viewQuery}";
                        var view = new View(viewName, viewDefinition, 0.0, 0.0);
                        resultViews.Add(view);
                    }

                }


            }
                return resultViews;
        }
        private List<View> _createJoinViews(List<Query> queries, List<View> allBaseProjectionViews)
        {
            var joinViews = new List<View>();
            foreach (var query in queries)
            {
                if (!query.Join.Any())
                {
                    continue;
                }
                else
                {
                    var queryJoinViews = new List<View>();
                    foreach (var join in query.Join)
                    {
                 
                        //prior join view already exists
                        if (queryJoinViews.Count() > 0)
                        {
                            var priorQueryJoinViews = queryJoinViews.Where(x => x.Name.Contains(join.Item1)).ToList().OrderByDescending(x => x.Name).First();
                            var table1 = priorQueryJoinViews.Name;
                            var table2 = allBaseProjectionViews.Where(x => x.Name.Contains(join.Item2)).Select(x => x.Name).First();
                            var joinCondition = join.Item3.Replace(join.Item1 + ",", table1 + ",").Replace(join.Item2 + " ", table2 + " ")
                                .Replace(join.Item1 + ".", table1 + ".").Replace(join.Item2 + ".", table2 + ".");
                            var viewName = $"{table1.Split("view")[0]}_{join.Item2}view";
                            var viewQuery = $"SELECT * FROM {table1}, {table2} WHERE {joinCondition}";
                            var viewDefinition = $"CREATE VIEW {viewName} AS {viewQuery}";
                            var view = new View(viewName, viewDefinition, 0.0, 0.0);
                            queryJoinViews.Add(view);
                        }
                        else
                        {
                            var table1 = "";
                            var table2 = "";
                            var joinCondition = "";
                            foreach (var projectionView in allBaseProjectionViews)
                            {

                                if (projectionView.Name.Split("2")[0].Trim() == join.Item1)
                                {
                                    table1 = projectionView.Name;
                                }
                                else if (projectionView.Name.Split("2")[0].Trim() == join.Item2)
                                {
                                    table2 = projectionView.Name;
                                }

                            }
                            joinCondition = join.Item3.Replace(join.Item1, table1).Replace(join.Item2, table2);
                            var viewName = $"{join.Item1}_{join.Item2}view";
                            var viewQuery = $"SELECT * FROM {table1}, {table2} WHERE {joinCondition}";
                            var viewDefinition = $"CREATE VIEW {viewName} AS {viewQuery}";
                            var view = new View(viewName, viewDefinition, 0.0, 0.0);
                            queryJoinViews.Add(view);
                        }
                        joinViews.AddRange(queryJoinViews);
                    }
                }
            }
            joinViews = joinViews.DistinctBy(x => x.Name).ToList();
            return joinViews;
        }
            
        private List<View> _createBaseProjectionViews(List<Query> queries, List<View> allBaseSelectionViews)
        {
            var baseProjectionViews = new List<View>();
            var allBaseRelations = queries.Select(x => x.BaseRelation).ToList().Distinct().SelectMany(x => x).Distinct().ToList();

            var allProjections = queries.Select(x => x.ProjectionColumn).ToList().Distinct().SelectMany(x => x).ToList();
            foreach (var baseRelation in allBaseRelations)
            {
                var baseRelationProjections = allProjections.Where(x => x.Item1 == baseRelation).Select(x => x.Item2).Distinct().ToList();
                //get tables from calculation columuns
                for(var i=0; i < baseRelationProjections.Count(); i++)
                {
                    var baseProjectionsFromCalculationColumun = this._getColumnsFromExpression(baseRelationProjections[i]);
                    //TODO: fix condition to read columuns
                    if (baseProjectionsFromCalculationColumun != null && baseRelationProjections[i].Contains("(")) {
                        baseRelationProjections.Remove(baseRelationProjections[i]);
                        baseRelationProjections.AddRange(baseProjectionsFromCalculationColumun);
                    }
                }
                string projection;
                if (baseRelationProjections.Count() == 1)
                {
                    projection = baseRelationProjections.First();
                }
                else
                {
                    projection = String.Join(" , ", baseRelationProjections);
                    
                }
                //selection view for this table exists
                if (allBaseSelectionViews.Where(x => x.Name.Split("1")[0] == baseRelation).Any())
                    {

                        foreach (var selection in allBaseSelectionViews)
                        {
                            if (selection.Name.Split("1")[0].Trim() == baseRelation)
                            {
                                var viewName = $"{baseRelation}2view";                                                           
                                var viewQuery = $"SELECT {projection} FROM {baseRelation}";
                                viewQuery = viewQuery.Replace(baseRelation, selection.Name);
                                var viewDefinition = $"CREATE VIEW {viewName} AS {viewQuery}";
                                var view = new View(viewName, viewDefinition, 0.0, 0.0);
                                baseProjectionViews.Add(view);
                            }
                        }
                    }
                    else{
                    var viewName = $"{baseRelation}2view";
                    var viewQuery = $"SELECT {projection} FROM {baseRelation}";
                    var viewDefinition = $"CREATE VIEW {viewName} AS {viewQuery}";                   
                    var view = new View(viewName, viewDefinition, 0.0, 0.0);
                    baseProjectionViews.Add(view);

                }
            }
            return baseProjectionViews;
        }
            private List<View> _createBaseSelectionViews(List<Query> queries)
        {
            var baseSelectionViews = new List<View>();
            var allBaseRelations = queries.Select(x => x.BaseRelation).ToList().Distinct().SelectMany(x => x).Distinct().ToList();

            //Create BaseTable Selection Views
            var allSelections = queries.Select(x => x.WhereCondition).ToList().Distinct().SelectMany(x => x).ToList();

            foreach (var baseRelation in allBaseRelations)
            {
                var baseRelationSelection = allSelections.Where(x => x.Item1 == baseRelation).Select(x => x.Item2).Distinct().ToList();
                if (baseRelationSelection.Any())
                {
                    string selection;
                    if (baseRelationSelection.Count() == 1)
                    {
                        selection = baseRelationSelection.First();
                    }
                    else
                    {
                        selection = String.Join(" OR ", baseRelationSelection);
                    }
                    var viewName = $"{baseRelation}1view";
                    var viewQuery = $"SELECT * FROM {baseRelation} WHERE {selection}";
                    var viewDefinition = $"CREATE VIEW {viewName} AS {viewQuery}";
                    var view = new View(viewName, viewDefinition, 0.0, 0.0);
                    baseSelectionViews.Add(view);

                }

            }
            return baseSelectionViews;
        }
        private List<string> _getColumnsFromExpression(string expression)
        {
            var columns = new List<string>();

            var regex = new Regex(@"\b\w+\.\w+\b");
            MatchCollection matches = regex.Matches(expression);

            foreach (Match match in matches)
            {
                columns.Add(match.Value);
            }

            return columns;
        }
    }
}
