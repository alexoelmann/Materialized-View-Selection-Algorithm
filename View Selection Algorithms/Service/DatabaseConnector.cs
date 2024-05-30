using Npgsql;
using System.Globalization;
using System.Text.RegularExpressions;


namespace View_Selection_Algorithms.Service
{
    public class DatabaseConnector
    {
        private readonly string connectionString = "";
        public string SQLQueryTableConnector(string sqlQuery)
        {
            var result = string.Empty;
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (NpgsqlCommand command = new NpgsqlCommand(sqlQuery, connection))
                    {
                        command.ExecuteNonQuery();
                        Console.WriteLine("CREATE MATERIALIZED VIEW successfully executed.");
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }

            }
            return result;
        }
        public double SQLQueryCostConnector(string sqlQuery)
        {

            var result = 0.0;
            var jsonResult = string.Empty;
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    var explainQuery = $"EXPLAIN   {sqlQuery}";
                    using (NpgsqlCommand command = new NpgsqlCommand(explainQuery, connection))
                    {
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                jsonResult = reader.GetString(0);
                                break;

                                ;
                            }

                        }
                    }

                }

                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }

            }
            result = _getQueryProcessingCost(jsonResult);
            return result;
        }
        public double SQLQueryTimeConnector(string sqlQuery)
        {
            var result = 0.0;
            var jsonResult = string.Empty;
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    var explainQuery = $"EXPLAIN  ANALYZE {sqlQuery}";
                    using (NpgsqlCommand command = new NpgsqlCommand(explainQuery, connection))
                    {
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                jsonResult = reader.GetString(0);
                                break;

                                ;
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            result = _getQueryProcessingTime(jsonResult);
            return result;
        }
        private double _getQueryProcessingTime(string jsonPlan)
        {
            var result = 0.0;
            var timePattern = @"actual time=([\d.]+)..([\d.]+)";
            var match = Regex.Match(jsonPlan, timePattern);

            if (match.Success)
            {
                var actualPattern = @"actual time=\d+\.\d+..\d+\.(\d+)";
                var matchActual = Regex.Match(match.Groups[0].Value, actualPattern);

                if (matchActual.Success)
                {
                    var timeString = matchActual.Groups[0].Value;
                    timeString = timeString.Split("..")[1].Split(".")[0];

                    if (double.TryParse(timeString, out double timeValue))
                    {
                        result = timeValue;
                    }
                }
            }
            return result;
        }
        public double GetMVStorageCost(string viewName)
        {
            var storageCost = 0.0;

            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();

                    var sql = $"SELECT pg_total_relation_size(quote_ident('{viewName}')) AS storage_cost;";

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        var result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            storageCost = Convert.ToInt64(result);
                            return storageCost;
                        }
                        else
                        {
                            storageCost = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            return storageCost;
        }
        private double _getQueryProcessingCost(string jsonPlan)
        {
            var regex = new Regex(@"(\d+\.{2})(\d+\.\d{2})");
            var match = regex.Match(jsonPlan);
            var cost = match.Groups[2].Value;

            var cultureInfo = new CultureInfo("en-US");
            var parsedValue = double.Parse(cost, cultureInfo);

            return parsedValue;
        }
    }
}

