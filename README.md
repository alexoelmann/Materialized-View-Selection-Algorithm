# Benchmark of View Selection Algorithmn

This repository contains the source code for the evaluation platform presented in the bachelorthesis "Benchmark of View Selection Algorithms".  
As part of this paper, we re-implemented 2 View Selection Algorithms (references listed below):

Deterministic MVPP [1]  
Hybrid MVPP [2]

The implementations of the algorithms can be found under BusinessLogic/MaterializedViewSelectionLogic.  
The query workload can be found and edited under Data/Queries

Our current implementation only allow SQL-queries with the restriction of the GROUP BY, ORDER-clause and Subqueries.  
In future work this may be enhzanced and optimized.

## Database and Cost Estimation
For our database we use PostgreSQL. For the measures discussed in this work we utilize the costs and time estimator of PostgreSQL.  
To connect to a PostgreSQL database fill the connectionString which can be found under Service/DatabaseConnector.  
The form of this variable must look like this: "Host=host;Username=username;Password=password;Database=database;CommandTimeout=0"

## Usage
To run the application just set the desired amount of generations and the name of the output csv inside the paramater of the function mainLogic.Sequence().

## References
[1]  
[2]

