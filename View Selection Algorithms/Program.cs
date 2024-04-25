using View_Selection_Algorithms.MVPPMainLogic;

public class Program
{

    public static void Main()
    {
       
        var mainLogic = new MainLogic();
        mainLogic.Sequence(5);
        
        
        Console.WriteLine("Finish");
        Console.ReadKey();
    }

}
    

        
