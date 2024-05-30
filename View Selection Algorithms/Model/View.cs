namespace View_Selection_Algorithms.Model
{
    public class View
    {
        public string Name { get; set; }
        public string Definition { get; set; }
        public double QueryProcessingCost { get; set; }
        public double StorageCost { get; set; }

        public View(string name, string definition, double queryProcessingCost, double storageCost)
        {
            this.Name = name;
            this.Definition = definition;
            this.QueryProcessingCost = queryProcessingCost;
            this.StorageCost = storageCost;
        }
    }
}