using System;
using System.Data;
using System.IO;
using ExcelDataReader;
using System.Linq;
using System.Text;
using System.Collections.Generic; 

namespace EnterData
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

        static int StartReadingIndex(DataTable table, string start_reading)
        {
            int start_index = 0;

            while (table.Rows[start_index][0].ToString() != start_reading && start_index < table.Rows.Count)
            {
                start_index++;
            }
            return start_index+1;
        }

        static List<string> ColumnNames(DataTable table, ref string sql)
        {
            List<string> column_names = new List<string>();
            foreach (DataColumn column in table.Columns)
            {
                string column_name = column.ColumnName.Trim().Replace(' ', '_');
                if (column_name.ToLower().Contains("column") || column_name == "Electricity" || column_name == "Water")
                {
                    continue;
                }
                if (column_name.ToLower() == "amount")
                {
                    column_name += "_VND";
                }
                sql += column_name + ",";
                column_names.Add(column.ColumnName);
            }
            sql = sql.Remove(sql.LastIndexOf(',')).Trim() + ") VALUES \n";
            return column_names; 
        }

        static string SqlQuery(DataTable table, int start_index, string sql, List<string> column_names, string[]break_string)
        {
            for (int i = start_index; i < table.Rows.Count; i++)
            {
                if (table.Rows[i][0].ToString().Trim() == "")
                {
                    continue;
                }
                if (break_string.Contains(table.Rows[i][0].ToString().Trim()))
                {
                    break;
                }
                string values = "(";
                foreach (string column_name in column_names)
                {
                    values += "'" + table.Rows[i][column_name].ToString() + "',";
                }
                values = values.Remove(values.LastIndexOf(',')).Trim() + ")";
                sql += values + ",\n";
            }

            return sql.Remove(sql.LastIndexOf(',')).Trim() + ";";
        }

        static string RevenueExpense(DataTable table)
        {
            string sql = "INSERT INTO `" + table.TableName.Replace("AP_", string.Empty).Trim()+"`(";

            List<string> column_names = ColumnNames(table, ref sql); 
            int start_index = StartReadingIndex(table, "Data entry"); 
            return SqlQuery(table, start_index, sql, column_names, new string[]{"End", "Total" }); 
        }

        static string TenantLease(DataTable table)
        {
            string table_name = table.TableName; 
            try
            {
                table_name = table_name.Substring(0, table_name.IndexOf('-')); 
            }
            catch { }
            string sql = "INSERT INTO `" + table_name.Trim() + "`(";
            List<string> column_names = ColumnNames(table, ref sql);
            int start_index = StartReadingIndex(table, "MAY AN PHU");
            return SqlQuery(table, start_index, sql, column_names, new string[] { "MAY THI NGHE" });
        }
        static void Main(string[] args)
        {
            string file_name = "AP_dulieu_May_Template.xls";
            DataSet data = ReadExcelFile(file_name);

            //string sql = Revenue_Expense(data.Tables["AP_Revenue"]);

            //Console.WriteLine("****************************");
            //Console.WriteLine(sql);

            //sql = Revenue_Expense(data.Tables["AP_Expense"]);
            //Console.WriteLine("&&&&&&&&&&&&&&&&&&&&&&&777");
            //Console.WriteLine(sql); 

            //string sql = TenantLease(data.Tables["Tenant"]);
            //Console.WriteLine(sql); 

            string sql = TenantLease(data.Tables["Tenant"]) +"\n\n" + TenantLease(data.Tables["LeaseAgrm-011014"]) + "\n\n" +  RevenueExpense(data.Tables["AP_Revenue"]) + "\n\n" + RevenueExpense(data.Tables["AP_Expense"]) + "\n\n";
            File.WriteAllText("data.txt", sql); 
        }
    }
}
