namespace View_Selection_Algorithms.Data
{
    public class Queries
    {
        public List<Tuple<string, int, double>> GetQueries()
        {
            var result = new List<Tuple<string, int, double>>();

            // Query 1
            result.Add(new Tuple<string, int, double>(
                @"SELECT lineitem.l_returnflag, lineitem.l_quantity
      FROM lineitem
      WHERE lineitem.l_shipmode = 'MAIL';",
                1,
                1));

            // Query 2
            result.Add(new Tuple<string, int, double>(
                @"SELECT part.p_name, supplier.s_name
          FROM part, partsupp, supplier
          WHERE part.p_partkey = partsupp.ps_partkey
          AND partsupp.ps_suppkey = supplier.s_suppkey
          AND part.p_mfgr = 'Manufacturer#1';",
                2,
                1));

            // Query 3
            result.Add(new Tuple<string, int, double>(
                @"SELECT customer.c_custkey, customer.c_name, orders.o_orderkey, orders.o_orderdate
          FROM orders, customer
          WHERE orders.o_custkey = customer.c_custkey
          AND customer.c_nationkey = 21
          AND orders.o_orderstatus = 'F';",
                3,
                1));

            // Query 4
            result.Add(new Tuple<string, int, double>(
                @"SELECT part.p_name, partsupp.ps_supplycost
          FROM part, partsupp
          WHERE part.p_partkey = partsupp.ps_partkey
          AND part.p_mfgr = 'Manufacturer#1';",
                4,
                1));

            // Query 5
            result.Add(new Tuple<string, int, double>(
                @"SELECT orders.o_orderkey, orders.o_orderdate, lineitem.l_partkey
          FROM orders, lineitem, supplier
          WHERE orders.o_orderkey = lineitem.l_orderkey
          AND lineitem.l_partkey = supplier.s_suppkey
          AND orders.o_orderstatus = 'F'
          AND lineitem.l_shipmode = 'MAIL';",
                5,
                1));

            // Query 6
            result.Add(new Tuple<string, int, double>(
                @"SELECT customer.c_custkey, customer.c_name, customer.c_nationkey
      FROM customer
      WHERE customer.c_nationkey = 10;",
                6,
                1));

            // Query 7
            result.Add(new Tuple<string, int, double>(
                @"SELECT orders.o_orderkey, orders.o_orderdate, lineitem.l_partkey
          FROM orders, lineitem, part
          WHERE orders.o_orderkey = lineitem.l_orderkey
          AND lineitem.l_partkey = part.p_partkey
          AND part.p_name LIKE '%blue%'
          AND lineitem.l_shipmode = 'MAIL'
          AND orders.o_orderpriority = '2-HIGH';",
                7,
                1));

            // Query 8
            result.Add(new Tuple<string, int, double>(
                @"SELECT customer.c_custkey, customer.c_name
      FROM customer
      WHERE customer.c_mktsegment = 'MACHINERY';",
                8,
                1));

            // Query 9
            result.Add(new Tuple<string, int, double>(
                @"SELECT part.p_name, supplier.s_suppkey, partsupp.ps_availqty
          FROM part, partsupp, supplier
          WHERE part.p_partkey = partsupp.ps_partkey
          AND part.p_name LIKE '%blue%'
          AND partsupp.ps_suppkey = supplier.s_suppkey;",
                9,
                1));

            // Query 10
            result.Add(new Tuple<string, int, double>(
                @"SELECT customer.c_custkey, customer.c_name, orders.o_orderkey, orders.o_orderdate
          FROM orders, customer, lineitem, part
          WHERE orders.o_custkey = customer.c_custkey
          AND orders.o_orderkey = lineitem.l_orderkey
          AND lineitem.l_partkey = part.p_partkey
          AND part.p_name = 'Laptop'
          AND lineitem.l_quantity > 50.00
          AND orders.o_orderpriority = '2-HIGH'
          AND customer.c_mktsegment = 'MACHINERY';",
                10,
                1));

            // Query 11
            result.Add(new Tuple<string, int, double>(
                @"SELECT orders.o_orderkey, orders.o_orderdate
          FROM orders, customer
          WHERE orders.o_custkey = customer.c_custkey
          AND customer.c_nationkey = 21
          AND orders.o_orderpriority = '2-HIGH';",
                11,
                1));

            // Query 12
            result.Add(new Tuple<string, int, double>(
                @"SELECT orders.o_orderkey, orders.o_orderdate, customer.c_name
          FROM orders, customer
          WHERE orders.o_custkey = customer.c_custkey
          AND orders.o_orderdate >= '2023-01-01'
          AND customer.c_mktsegment = 'MACHINERY';",
                12,
                1));

            // Query 13
            result.Add(new Tuple<string, int, double>(
                @"SELECT supplier.s_suppkey, supplier.s_name, partsupp.ps_partkey, part.p_name
          FROM part, partsupp, supplier
          WHERE part.p_partkey = partsupp.ps_partkey
          AND partsupp.ps_suppkey = supplier.s_suppkey
          AND part.p_mfgr = 'Manufacturer#4';",
                13,
                1));

            // Query 14
            result.Add(new Tuple<string, int, double>(
                @"SELECT customer.c_custkey, customer.c_name, orders.o_orderkey, orders.o_orderstatus
          FROM orders, customer
          WHERE orders.o_custkey = customer.c_custkey
          AND orders.o_orderstatus = 'SHIPPED'
          AND customer.c_mktsegment = 'MACHINERY';",
                14,
                1));

            // Query 15
            result.Add(new Tuple<string, int, double>(
                @"SELECT orders.o_orderkey, orders.o_orderdate, lineitem.l_partkey, part.p_brand
          FROM orders, lineitem, part
          WHERE orders.o_orderkey = lineitem.l_orderkey
          AND lineitem.l_partkey = part.p_partkey
          AND part.p_brand = 'Brand#44'
          AND lineitem.l_quantity > 50.00
          AND orders.o_orderstatus = 'F';",
                15,
                1));

            // Query 16
            result.Add(new Tuple<string, int, double>(
                @"SELECT customer.c_custkey, customer.c_name, orders.o_orderkey, lineitem.l_partkey
          FROM orders, customer, lineitem, part
          WHERE orders.o_custkey = customer.c_custkey
          AND orders.o_orderkey = lineitem.l_orderkey
          AND lineitem.l_partkey = part.p_partkey
          AND part.p_mfgr = 'Manufacturer#2'
          AND lineitem.l_quantity > 50.00
          AND orders.o_orderstatus = 'SHIPPED'
          AND customer.c_mktsegment = 'BUILDING';",
                16,
                1));

            // Query 17
            result.Add(new Tuple<string, int, double>(
                @"SELECT part.p_partkey, part.p_name, partsupp.ps_suppkey, partsupp.ps_supplycost
          FROM part, partsupp, supplier
          WHERE part.p_partkey = partsupp.ps_partkey
          AND partsupp.ps_suppkey = supplier.s_suppkey
          AND part.p_brand = 'Brand#44';",
                17,
                1));

            // Query 18
            result.Add(new Tuple<string, int, double>(
                @"SELECT supplier.s_suppkey, supplier.s_name, partsupp.ps_partkey, part.p_name, supplier.s_address
          FROM part, partsupp, supplier
          WHERE part.p_partkey = partsupp.ps_partkey
          AND partsupp.ps_suppkey = supplier.s_suppkey;",
                18,
                1));

            // Query 19
            result.Add(new Tuple<string, int, double>(
                @"SELECT customer.c_custkey, customer.c_name, orders.o_orderkey, lineitem.l_partkey, part.p_brand
          FROM orders, customer, lineitem, part
          WHERE orders.o_custkey = customer.c_custkey
          AND orders.o_orderkey = lineitem.l_orderkey
          AND lineitem.l_partkey = part.p_partkey
          AND part.p_brand = 'Brand#24'
          AND lineitem.l_quantity > 50.00
          AND orders.o_orderstatus = 'SHIPPED'
          AND customer.c_mktsegment = 'MACHINERY';",
                19,
                1));

            // Query 20
            result.Add(new Tuple<string, int, double>(
                @"SELECT customer.c_custkey, customer.c_name, orders.o_orderkey, orders.o_orderdate
      FROM orders, customer
      WHERE orders.o_custkey = customer.c_custkey
      AND orders.o_orderdate >= '2023-01-01'
      AND customer.c_nationkey = 10;",
                20,
                1));

            return result;
        }
    }
}
