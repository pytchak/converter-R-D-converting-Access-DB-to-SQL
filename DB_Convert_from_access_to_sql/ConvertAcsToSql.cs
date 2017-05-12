using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Data;
using System.Data.Common;
using System.Collections;

namespace DB_Convert_from_access_to_sql
{
    class ConvertAcsToSql
    {
        List<string> TableNames;
        int numberTable = 0;
        string connectionOleString;
        string connectionSqlString;

        public ConvertAcsToSql(string wayToDBOle, string wayToDBSql)
        {
            connectionOleString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + wayToDBOle;
            connectionSqlString = @"Data Source=" + wayToDBSql + "; Initial Catalog=testDB;Integrated Security=True";
            TableNames = GetTables(connectionOleString, out numberTable);
            for (int i = 0; i < numberTable; i++)
            {
                CreateSqlTable(connectionSqlString, TableNames[i]);
                Convert(connectionOleString, connectionSqlString, TableNames[i]);
            }
        }
        public void Check(object a)
        {
            Console.WriteLine(a);
        }

        public List<string> GetTables(string connectionString, out int numberTable)
        {
            numberTable = 0;
            DbProviderFactory factory = DbProviderFactories.GetFactory("System.Data.OleDb");
            DataTable userTables = null;
            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connectionString;
                string[] restrictions = new string[4];
                restrictions[3] = "Table";
                connection.Open();
                userTables = connection.GetSchema("Tables", restrictions);
            }
            List<string> TableNames = new List<string>();
            for (int i = 0; i < userTables.Rows.Count; i++)
            {
                TableNames.Add(userTables.Rows[i][2].ToString());
                numberTable++;
                //Check(TableNames[i]);
            }
            return TableNames;
        }

        public void CreateSqlTable(string connectionString, string tableName)
        {
            string commandSqlNameType = GetTypeColumn(connectionOleString, tableName);
            SqlConnection con = new SqlConnection(connectionString);
            string sql = "create table " + tableName + commandSqlNameType;
            SqlCommand cmd = new SqlCommand(sql, con);
            con.Open();
            cmd.ExecuteReader();
            con.Close();
        }
        public string GetTypeColumn(string connectionOleString, string tableName)
        {
            using (OleDbConnection connectionOle = new OleDbConnection(connectionOleString))
            {
                OleDbCommand commandOle = new OleDbCommand("Select * from " + tableName, connectionOle);
                connectionOle.Open();
                OleDbDataReader readerOle = commandOle.ExecuteReader();
                string commandForCreatTable = "(id int identity(1,1) primary key,";
                for (int c = 1; c < readerOle.VisibleFieldCount; c++)
                {
                    System.Type type = readerOle.GetFieldType(c);
                    string temp = type.Name;
                    if (temp.Contains("Int"))
                    {
                        commandForCreatTable = commandForCreatTable + readerOle.GetName(c) + " int,";
                    }
                    /*
                    if (temp.Contains("Boolean"))
                    {
                  
                    }*/
                    if (temp.Contains("String"))
                    {
                        commandForCreatTable = commandForCreatTable + readerOle.GetName(c) + " nvarchar(50),";
                    }
                }
                commandForCreatTable = commandForCreatTable.Remove(commandForCreatTable.Length - 1, 1);
                commandForCreatTable = commandForCreatTable + ")";
                return commandForCreatTable;
            }
        }
        /*
        public string GetCommandForInsertDate(string connectionOleString, string tableName)
        {
            using (OleDbConnection connectionOle = new OleDbConnection(connectionOleString))
            {
                OleDbCommand commandOle = new OleDbCommand("Select * from " + tableName, connectionOle);
                connectionOle.Open();
                OleDbDataReader readerOle = commandOle.ExecuteReader();
                string commandForInsertDate = "INSERT INTO " + tableName + " ( ";
                for (int c = 1; c < readerOle.VisibleFieldCount; c++)
                {
                    commandForInsertDate = commandForInsertDate + readerOle.GetName(c) + " , ";
                }
                commandForInsertDate = commandForInsertDate.Remove(commandForInsertDate.Length - 2, 1);
                commandForInsertDate = commandForInsertDate + " ) VALUES ( ";
                return commandForInsertDate;
            }
        }
        */
        public void Convert(string connectionOleString, string connectionSqlString, string tableName)
        {
            using (SqlConnection connectionsql = new SqlConnection(connectionSqlString))
            {
                using (OleDbConnection connectionOle = new OleDbConnection(connectionOleString))
                {
                    connectionsql.Open();
                    connectionOle.Open();
                    OleDbCommand commandOle = new OleDbCommand("select * from " + tableName + "", connectionOle);
                    // OleDbDataAdapter da = new OleDbDataAdapter("SHOW TABLES", connectionOle);
                    SqlCommand commandSql;
                    OleDbDataReader readerOle = commandOle.ExecuteReader();

                    string sqlExpressionOnlyName = "INSERT INTO " + tableName + " ( ";
                    for (int i = 1; i < readerOle.FieldCount; i++)
                    {
                        sqlExpressionOnlyName = sqlExpressionOnlyName + readerOle.GetName(i) + " , ";
                        string nextColumnName = readerOle.GetName(i);
                        Console.Write(nextColumnName + ";");
                    }
                    sqlExpressionOnlyName = sqlExpressionOnlyName.Remove(sqlExpressionOnlyName.Length - 2, 1);
                    sqlExpressionOnlyName = sqlExpressionOnlyName + " ) VALUES ( ";
                    Console.WriteLine(readerOle.FieldCount);

                    while (readerOle.Read())
                    {
                        string sqlExpressionFinal = sqlExpressionOnlyName;
                        //int i = 1;
                        for (int i = 1; i < readerOle.FieldCount; i++)
                        {
                            //ArrayList myArrayList = new ArrayList();

                            if (readerOle.GetValue(i) is string)
                            {
                                sqlExpressionFinal = sqlExpressionFinal + "'" + readerOle.GetValue(i) + "' , ";
                            }
                            else
                            {
                                sqlExpressionFinal = sqlExpressionFinal + readerOle.GetValue(i) + " , ";
                            }
                            // myArrayList.Add(readerOle.GetValue(i));
                        }

                            sqlExpressionFinal = sqlExpressionFinal.Remove(sqlExpressionFinal.Length - 2, 1);
                            sqlExpressionFinal = sqlExpressionFinal + " )";


                            object id = readerOle.GetValue(0);
                            object name = readerOle.GetValue(1);
                            object age = readerOle.GetValue(2);
                            // string sqlExpression = String.Format("INSERT INTO Users (Name, Age) VALUES ('{0}', {1})", name, age);
                            commandSql = new SqlCommand(String.Format(sqlExpressionFinal), connectionsql);
                            commandSql.ExecuteNonQuery();
                            // i++;
                            //Console.WriteLine("{0} \t{1} \t{2}", id, name, age);
                        
                    }
                    commandSql = new SqlCommand(("SHOW TABLES"), connectionsql);
                   
                    //readerOle = commandOle.ExecuteReader();
                    //Console.Write();
                }
            }
        }
    }
}
