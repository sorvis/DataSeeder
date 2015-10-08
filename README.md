# DataSeeder
Insert sample data to your SQL database with ease! 

#Usage: DataSeeder <action> options

##Global options:

   **OPTION       TYPE     DESCRIPTION**
   -Help (-H)   switch   Show usage

##Actions:
###Seed 

| OPTION                 | TYPE    | POSITION | DESCRIPTION |
| ---------------------- | ------- | -------- | ----------- |
| -Provider (-p)         | string* | 1        |             |
| -ConnectionString (-c) | string* | 2        |             |
| -InputFile (-f)        | string* | 3        |             |

###Dump 

| OPTION                   | TYPE       | POSITION | DESCRIPTION    |
| ------------------------ | ---------- | -------- | -------------- |
| -Provider (-p)           | string*    | 1        |                |
| -ConnectionString (-c)   | string*    | 2        |                |
| -TableName (-T)          | string*    | 3        |                |
| -Condition (-w)          | string     | NA       |                |
| -Columns (-s)            | string[]   | NA       |                |
| -OutputFile (-f)         | string*    | NA       |                |
| -Append (-A)             | switch*    | NA       | [default=True] |

#Supported providers
* Oracle.ManagedDataAccess.Client (throws error because of bad SQL query)

#Unsupported providers

