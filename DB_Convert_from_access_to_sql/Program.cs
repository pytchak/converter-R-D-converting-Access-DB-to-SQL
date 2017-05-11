using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace DB_Convert_from_access_to_sql
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] wayToDB = File.ReadAllLines(@"F:\С#\Progect City\DB_Convert_from_access_to_sql\DB_Convert_from_access_to_sql\WayToDataBase.txt");
            ConvertAcsToSql A = new ConvertAcsToSql(wayToDB[0], wayToDB[1]);
            Console.Read();
        }
    }
}
