using System;
using ExcelDataReader;
using System.IO;
using System.Data; 

namespace Read_Tables
{
    class Program
    {

        static DataSet ReadExcelFile(string file_name)
        {
            FileStream file = File.Open(file_name, FileMode.Open, FileAccess.Read);
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            IExcelDataReader excel_data_reader = ExcelReaderFactory.CreateReader(file);
            ExcelDataSetConfiguration configuration = new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration
                {
                    UseHeaderRow = true
                }
            };
            DataSet dataset = excel_data_reader.AsDataSet(configuration);
            return dataset; 
        }

        static string CreateTable(DataTable table)
        {
            string sql =
                @"
                    CREATE TABLE " + table.TableName + @"(";
            foreach (DataRow row in table.Rows)
            {
                sql += row["Field"].ToString().Replace(' ','_') + " " + row["Type"].ToString() + ", "; 
            }
            sql = sql.Remove(sql.LastIndexOf(',')).Trim(); 
            sql+=@"
                    ); 
                "; 
            return sql; 
        }

        static string DefaultValues()
        {
            string file_name = "default.xlt";
            string sql = ""; 
            DataTable values = ReadExcelFile(file_name).Tables[0];
            foreach (DataColumn column in values.Columns)
            {
                string table_name = column.ColumnName.Trim().Replace(' ','_');
                foreach (DataRow row in values.Rows)
                {
                    string value = row[column].ToString().Trim(); 
                    if(value!="")
                    {
                        sql += "INSERT INTO `" + table_name + "` VALUES('" + value + "'); \n"; 
                    }
                }
            }
            return sql; 
        }
        static void Main(string[] args)
        {
            //DataSet all_talbles = ReadExcelFile();
            //string sql = ""; 
            //foreach (DataTable table in all_talbles.Tables)
            //{
            //    sql += CreateTable(table); 
            //}
            //File.WriteAllText("tables.txt", sql); 

            string default_sql = DefaultValues();
            Console.WriteLine(default_sql);
            File.WriteAllText("default.txt", default_sql); 
        }
    }
}
