using System.Text.RegularExpressions;
using View_Selection_Algorithms.Data;
using View_Selection_Algorithms.Model;

namespace View_Selection_Algorithms.Service.QueryParsingLogic
{
    public class QueryParser
    {
        private string JoinPattern = @"^\s*\w+\.\w+\s*=\s*\w+\.\w+\s*$";
        public List<Query> ExtractAllQueryParts()
        {
        
            var queries = new Queries().GetQueries();
            var result = new List<Query>();
            foreach (var query in queries)
            {
                var extractedQuery = this._extractQueryPart(query);
                result.Add(extractedQuery);
            }  
            return result;
        }
        private Query _extractQueryPart(Tuple<string, int, double> baseQuery)
        {
            //var extractedQuery = new Query();
            var query = baseQuery.Item1;
            var whereStatements = this._extractWhereStatements(query);
            var joins = this._extractJoinStatements(query);
            //filter out relevant columuns for queries: in select, in selection and in joins
            var projections = this._extractProjectionColumuns(query, whereStatements, joins);
            var baseRelations =this._extractBaseRelations(query);
            var queryProjections = this._extractQueryProjectionColumuns(query);
            var queryNumber = baseQuery.Item2;
            var queryFrequency = baseQuery.Item3;

            var parsedQuery = new Query(queryNumber, queryFrequency, joins, whereStatements, projections, baseRelations,queryProjections);

            return parsedQuery;
            



        }
        private List<Tuple<string, string>> _extractQueryProjectionColumuns(string query)
        {
            var result = new List<Tuple<string, string>>();
            var projectionPartIndex = query.IndexOf("SELECT") + 6;
            var projectionEndIndex = query.IndexOf("FROM");

            var projectionsPart = query.Substring(projectionPartIndex, projectionEndIndex - projectionPartIndex);

            if (!projectionsPart.Contains(","))
            {
                var oneTable = projectionsPart.Split(".")[0].Trim();
                var oneColumn = projectionsPart.Trim();
                result.Add(new Tuple<string, string>(oneTable, oneColumn));
                return result;
            }
            //More than one projection columns
            var projections = projectionsPart.Split(",").ToList();
            string table;
            string column;
            foreach (var projection in projections)
            {
                table = projection.Split(".")[0].Trim();
                column = projection.Trim();
                result.Add(new Tuple<string, string>(table, column));
            }
            return result;
        }
        private List<Tuple<string,string>> _extractProjectionColumuns(string query, List<Tuple<string, string>> whereStatements, List<Tuple<string, string, string>> joins)
        {
            var result = new List<Tuple<string, string>>();
            var projectionPartIndex = query.IndexOf("SELECT") + 6;
            var projectionEndIndex = query.IndexOf("FROM");

            var projectionsPart = query.Substring(projectionPartIndex, projectionEndIndex - projectionPartIndex);

            //More than one projection columns
            var projections = projectionsPart.Split(",").ToList();
            string table;
            string column;
            foreach(var projection in projections)
            {
                table = projection.Split(".")[0].Trim();
                column = projection.Trim();
                result.Add(new Tuple<string, string>(table, column));
            }
            //get projections from selections
            foreach(var selection in whereStatements)
            {
                table = selection.Item1;
                column = selection.Item2.Split(" ")[0];
                result.Add(new Tuple<string, string>(table, column));
            }
            foreach(var join in joins)
            {
                table = join.Item1;
                column = join.Item3.Split("=")[0].Trim();
                result.Add(new Tuple<string, string>(table, column));
                table = join.Item2;
                column = join.Item3.Split("=")[1].Trim();
                result.Add(new Tuple<string, string>(table, column));
            }
            result = result.Distinct().ToList();
            return result;

        }
        private List<string> _extractBaseRelations(string query)
        {
            var result = new List<string>();
            var fromPartIndex = query.IndexOf("FROM") + 4;
            var queryEndIndex = query.IndexOf("WHERE");

            if (queryEndIndex == -1)
            {
                queryEndIndex = query.IndexOf(";");
            }
            var baseRelations = query.Substring(fromPartIndex, queryEndIndex - fromPartIndex);
            //More than one one tables
            if (baseRelations.Contains(","))
            {
                var tables = baseRelations.Split(',');
                foreach(var table in tables)
                {
                    result.Add(table.Trim());
                }
            }
            else
            {
                result.Add(baseRelations.Trim());
            }
            return result;

        }

        private List<Tuple<string, string>> _extractWhereStatements(string query)
        {
            var result = new List<Tuple<string, string>>();
            var wherePartIndex = query.IndexOf("WHERE") + 5;
            var queryEndIndex = query.IndexOf(";");
            if (wherePartIndex == -1)
            {
                return null;
            }
            else
            {
                string table;
                string selection;
                var whereClause = query.Substring(wherePartIndex, queryEndIndex - wherePartIndex).Trim();
                //Query contains more than one selection statement
                if (whereClause.Contains("AND"))
                {
                    var whereClauseSplitted = whereClause.Split("AND");
                    var whereStatement = whereClauseSplitted.Where(x => !Regex.IsMatch(x, JoinPattern)).ToList();

                    
                    foreach(var statement in whereStatement)
                    {
                        table = statement.Split(".")[0].Trim();
                        selection= statement.Trim();
                        result.Add(new Tuple<string, string>(table, selection));
                    }

                }
                //Query contains only one selection statement
                else
                {
                    if(!Regex.IsMatch(whereClause, JoinPattern))
                    {
                        table = whereClause.Split(".")[0].Trim();
                        selection = whereClause.Trim();
                        result.Add(new Tuple<string, string>(table, selection));
                    }
                }
            }
            return result;
        }
        private  List<Tuple<string, string, string>> _extractJoinStatements(string query)
        {
            var result = new List<Tuple<string, string, string>>();
            var wherePartIndex = query.IndexOf("FROM") + 4;
            var queryEndIndex = query.IndexOf(";");
            if (wherePartIndex == -1)
            {
                return null;
            }
            else
            {
                string table1;
                string table2;
                string join;
                var whereClause = query.Substring(wherePartIndex, queryEndIndex - wherePartIndex).Trim();
                //Query contains more than one selection statement
                if (whereClause.Contains("AND"))
                {
                    var whereClauseSplitted = whereClause.Split("AND");
                    whereClauseSplitted[0] = whereClauseSplitted[0].Split("WHERE")[1].Trim();
                    var whereStatement = whereClauseSplitted.Where(x => Regex.IsMatch(x, JoinPattern)).ToList();


                    foreach (var statement in whereStatement)
                    {
                        table1 = statement.Split("=")[0].Trim().Split(".")[0].Trim();
                        table2 = statement.Split("=")[1].Trim().Split(".")[0].Trim();
                        join = statement.Trim();
                        result.Add(new Tuple<string, string,string>(table1, table2,join));
                    }

                }
                //Query contains only one selection statement
                else
                {
                    var joinPart = whereClause.Split("WHERE")[1].Trim();
                    if (Regex.IsMatch(joinPart, JoinPattern))
                    {
                        table1 = joinPart.Split("=")[0].Trim().Split(".")[0].Trim();
                        table2 = joinPart.Split("=")[1].Trim().Split(".")[0].Trim();
                        result.Add(new Tuple<string, string, string>(table1, table2, joinPart));
                    }
                }
            }
            return result;
        }
    }
}
