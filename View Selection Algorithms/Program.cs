using View_Selection_Algorithms.Service.MaterializedViewCreationLogic;
using View_Selection_Algorithms.Service.QueryParsingLogic;

public class Program
{

    public static void Main()
    {

        var parser = new QueryParser();
        var extractedQueryParts=parser.ExtractAllQueryParts();
        var viewCreator =new ViewCreator();
        var views=viewCreator.GenerateAllViews(extractedQueryParts);
        var deterministicMCreator = new DeterministicMVPPCreator();
        deterministicMCreator.ChooseMaterializedViews(views,extractedQueryParts);
        Console.WriteLine("Finish");
        Console.ReadKey();
    }

}
    

        
