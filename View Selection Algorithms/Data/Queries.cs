namespace View_Selection_Algorithms.Data
{
    public class Queries
    {
        public List<Tuple<string, int, double>> GetQueries()
        {
            var result = new List<Tuple<string, int, double>>();
            result.Add(this.sqlQuery1);
            result.Add(this.sqlQuery10);
            result.Add(this.sqlQuery3);
            result.Add(this.sqlQuery12);
            return result;
        }
        private Tuple<string, int, double> sqlQuery1 = new Tuple<string, int, double>(
            @"SELECT
                              lineitem.l_returnflag,
                              lineitem.l_quantity

                            FROM

                              lineitem

                            WHERE

                             lineitem.l_shipmode in ('MAIL', 'SHIP')
                           ;", 1, 1);
        private Tuple<string, int, double> sqlQuery10 = new Tuple<string, int, double>(
            @"SELECT

                              

                              customer.c_custkey,  
customer.c_name, 
lineitem.l_extendedprice * (1 - lineitem.l_discount ) as revenue, 
customer.c_acctbal,  nation.n_name,  
customer.c_address,  customer.c_phone, 
customer.c_comment,
orders.o_orderdate

                            FROM

                              customer,

                              orders,

                              lineitem,

                              nation


                            WHERE

                              lineitem.l_orderkey = orders.o_orderkey

                              AND orders.o_custkey = customer.c_custkey
                          
                              AND lineitem.l_shipmode in ('MAIL', 'SHIP')

                             AND customer.c_nationkey = nation.n_nationkey

                            ;", 10, 1);

        private Tuple<string, int, double> sqlQuery3 = new Tuple<string, int, double>(
            @"SELECT
    
orders.o_orderdate, orders.o_shippriority,
customer.c_name
FROM
    customer,
    orders

WHERE  
    
     customer.c_custkey = orders.o_custkey
;", 3, 1);
        private Tuple<string, int, double> sqlQuery12 = new Tuple<string, int, double>(
            @"SELECT
    lineitem.l_shipmode,
    orders.o_orderdate 
FROM
    orders,
    lineitem
WHERE
    lineitem.l_orderkey = orders.o_orderkey
    AND lineitem.l_shipmode in ('MAIL', 'SHIP')
    
;", 12, 1);
    }
}
