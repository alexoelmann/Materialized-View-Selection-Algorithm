using View_Selection_Algorithms.Service.QueryParsingLogic;

public class Program
{

    public static void Main()
    {

        var parser = new QueryParser();
        var extractedQueryParts=parser.ExtractAllQueryParts();
        var viewCreator =new ViewCreator();
        viewCreator.GenerateAllViews(extractedQueryParts);
        Console.WriteLine("Finish");
        Console.ReadKey();
    }

}
    

        
