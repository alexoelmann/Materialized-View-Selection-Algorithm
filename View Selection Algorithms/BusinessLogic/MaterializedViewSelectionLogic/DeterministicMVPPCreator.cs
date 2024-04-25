using System.Text.RegularExpressions;
using View_Selection_Algorithms.Model;

namespace View_Selection_Algorithms.Service.MaterializedViewCreationLogic
{
    public class DeterministicMVPPCreator
    {
        public Tuple<List<string>, List<View>, List<Query>> ChooseMaterializedViews(List<View> views, List<Query> queries)
        {
            var weights = this._calculateWeights(views,queries);
            var mvs = this._selectMaterializedViews(weights);
            return new(mvs,views,queries);
        }
        private List<string> _selectMaterializedViews(List<Tuple<string, string,double, double, double>> weights)
        {
            /*MV Selection algorithm*/
           
            // 1. create LV: weight>0 and set list for Materialized Views
            var lv = weights.Where(x => x.Item5 > 0).ToList();
            var mvsCost = new List<Tuple<string,double,string>>();
            // 2. sort weights descending
            var sortedLv = lv.OrderByDescending(x => x.Item5).ThenByDescending(x => x.Item2.Length).ToList();
            // 3. Calculate Cs-values
            for (var i = 0; i < sortedLv.Count(); i++)
            {
                var cs = 0.0;
                // 3.1 First value has no descendent views
                if (i == 0)
                    cs = sortedLv[i].Item5;
                // 3.2 First element is negative so new views can be created
                if (i > 0 && mvsCost.Count() == 0)
                    break;
                // 3.3 Next views 
                if (i > 0 && mvsCost.Count > 0)
                {
                    cs = sortedLv[i].Item5;
                    // 3.3.1 Check if current view already has descendant/parents Materialized Views
                    foreach (var mv in mvsCost)
                    {
                        // 3.3.2 base selection and projection view with same stats only projection view should be materialized
                        if (Regex.Replace(sortedLv[i].Item1.Split("view")[0], @"\d", "") == Regex.Replace(mv.Item1.Split("view")[0], @"\d", "") 
                            && (sortedLv[i].Item4* sortedLv[i].Item3) == mv.Item2)
                        {
                            cs = 0.0;
                            break;
                        }
                        if (sortedLv[i].Item2.Contains(mv.Item1) || mv.Item3.Contains(sortedLv[i].Item1) 
                        // 3.3.2 base selection and projection view with same stats only projection view should be materialized
                            || mv.Item3.Contains(Regex.Replace(sortedLv[i].Item1.Split("view")[0], @"\d", "")))
                        {
                            cs -= mv.Item2;
                        }
                    }
                }
                    // 4. If Cs Value>0 then the view gets materialized
                    if (cs > 0.0)
                    mvsCost.Add(new(sortedLv[i].Item1, (sortedLv[i].Item4 * sortedLv[i].Item3), sortedLv[i].Item2));
                continue;


            }

            var mvs = mvsCost.Select(x => x.Item1).ToList();
            return mvs;
        }
        private List<Tuple<string,string,double,double,double>> _calculateWeights(List<View> views, List<Query> queries)
        {
            var result = new List<Tuple<string, string,double, double, double>>();

            foreach (var view in views)
            {
                if (view.Name.Contains("result"))
                {
                    var queryNumber = view.Name.Split("result")[1].Split("view")[0];
                    var queryFrequency = queries.Where(x => x.QueryNumber == int.Parse(queryNumber)).Select(x => x.QueryFrequency).ToList().First();
                    var weight = (queryFrequency * view.QueryProcessingCost) - (1 * view.QueryProcessingCost);
                    result.Add(new(view.Name, view.Definition,queryFrequency, view.QueryProcessingCost, weight));
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
                                if((query.QueryNumber == 10 && (tables.Contains("customer") && tables.Contains("orders")))){
                                    counter--;
                                }
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
                   
                    result.Add(new(view.Name, view.Definition,sumOfQueryFrequencies, view.QueryProcessingCost, weight));
                }
            }
            return result;
        }
    }
}
