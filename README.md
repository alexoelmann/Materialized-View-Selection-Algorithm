# Benchmark of View Selection Algorithmn

This repository contains the source code for the evaluation platform presented in the bachelorthesis "Benchmark of View Selection Algorithms".  
As part of this paper, we re-implemented 2 View Selection Algorithms ([references](docs/References.md) listed below):

Deterministic MVPP [1] [2]  
Hybrid MVPP [3]

The implementations of the algorithms can be found under `BusinessLogic/MaterializedViewSelectionLogic`.  
The query workload can be found and edited under `Data/Queries`.

Our current implementation only allow SQL-queries with the restriction of the GROUP BY, ORDER-clause and Subqueries.  
In future work this may be enhanced and optimized.

## Database and Cost Estimation
For our database we use [PostgreSQL](https://www.postgresql.org/). For the measures discussed in this work we utilize the costs and time estimator of PostgreSQL.  
To connect to a PostgreSQL database fill the value for the variable connectionString which can be found under `Service/DatabaseConnector`.  
The form of this variable must look like this: **"Host=host;Username=username;Password=password;Database=database;CommandTimeout=0"**

## Usage
The application is written in C# (.NET Version 6.0) and its recommended to use [Visual Studio](https://visualstudio.microsoft.com/) as the IDE.  
We used the Visual Studio version 17.3.6.  
For the communication between PostgreSQL and the C# application it is mandatory to install the package [Npgsql](https://www.npgsql.org/) in our NuGet-packages.
To run the application just set the desired amount of generations and the name of the output csv inside the paramater of the function `mainLogic.Sequence(int NumGeneration, string outputFileName)`.

## References
[1]  Yang, J., Karlapalem, K., Li, Q.: A framework for designing materialized
views in data warehousing environment. In: Proceedings of 17th
International Conference on Distributed Computing Systems. pp. 458–465
(1997)
[2] Yang, J., Karlapalem, K., Li, Q.: Algorithms for materialized view design in
data warehousing environment. In VLDB pp. 136–145 (02 1970)  
[3] Zhang, C., Yao, X., Yang, J.: An evolutionary approach to materialized
views selection in a data warehouse environment. Systems, Man, and
Cybernetics, Part C: Applications and Reviews, IEEE Transactions on 31,
282 – 294 (09 2001)


