namespace View_Selection_Algorithms.Model
{
    public class Query
    {
        public int QueryNumber { get; set; }
        public double QueryFrequency { get; set; }
        public List<Tuple<string, string, string>> Join { get; set; }

        public List<Tuple<string, string>> SelectionCondition { get; set; }

        public List<Tuple<string, string>> ProjectionColumn { get; set; }
        public List<Tuple<string, string>> QueryProjectionColumn { get; set; }

        public List<string> BaseRelation { get; set; }

        public Query(int queryNumber, double queryFrequency, List<Tuple<string, string, string>> join, List<Tuple<string, string>> selectionCondition, List<Tuple<string, string>> projectionColumn, List<string> baseRelation, List<Tuple<string, string>> queryProjectionColumn)
        {
            this.QueryNumber = queryNumber;
            this.QueryFrequency = queryFrequency;
            this.Join = join;
            this.SelectionCondition = selectionCondition;
            this.ProjectionColumn = projectionColumn;
            this.BaseRelation = baseRelation;
            this.QueryProjectionColumn = queryProjectionColumn;
        }

    }
}
