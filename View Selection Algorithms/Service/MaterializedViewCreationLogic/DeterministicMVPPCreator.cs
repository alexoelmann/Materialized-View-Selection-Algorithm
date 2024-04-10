using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using View_Selection_Algorithms.Data;
using View_Selection_Algorithms.Model;
using View_Selection_Algorithms.Service.QueryParsingLogic;

namespace View_Selection_Algorithms.Service.MaterializedViewCreationLogic
{
    public class DeterministicMVPPCreator
    {
        public Tuple<List<string>, List<View>> ChooseMaterializedViews(List<View> views, List<Query> queries)
        {
            var weights = this._calculateWeights(views,queries);
            return null;
        }

        private List<Tuple<string,string,double,double>> _calculateWeights(List<View> views, List<Query> queries)
        {
            var result = new List<Tuple<string, string, double, double>>();

            foreach (var view in views)
            {
                if (view.Name.Contains("result"))
                {
                    var queryNumber = view.Name.Split("result")[1].Split("view")[0];
                    var queryFrequency = queries.Where(x => x.QueryNumber == int.Parse(queryNumber)).Select(x => x.QueryFrequency).ToList().First();
                    var weight = (queryFrequency * view.QueryProcessingCost) - (1 * view.QueryProcessingCost);
                    result.Add(new(view.Name, view.Definition, view.QueryProcessingCost, weight));
                }
                else
                {
                    var tables = Regex.Replace(view.Name.Replace("view", "").Replace("result", ""), @"\d", "").Trim().Split("_").ToList();
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
                    //calculate weight Note: the 1 denotes the update frequency of base relations of this view
                    var weight = (sumOfQueryFrequencies * view.QueryProcessingCost) - (1 * view.QueryProcessingCost);
                    result.Add(new(view.Name, view.Definition, view.QueryProcessingCost, weight));
                }
            }
            return result;
        }
    }
}
