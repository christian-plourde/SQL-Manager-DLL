using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Xml;

namespace SQLManager
{
    public class SQLManager
    {
        string sqlConnectionString;
        SqlCommand sql_command;

        public SQLManager(string connection_string)
        {
            this.sqlConnectionString = connection_string;
            sql_command = new SqlCommand();
        }

        private void ConnectToDB(string conn_string)
        {
            sql_command.Connection = new SqlConnection(conn_string);
        }

        public string SQLQuery(string sql_query)
        {
            SqlDataReader lecteurSQL;
            LinkedList<SQLRow> row_list = new LinkedList<SQLRow>();

            try
            {
                ConnectToDB(sqlConnectionString);
            }

            catch (Exception e)
            {
                return e.Message + " " + e.StackTrace;
            }

            try
            {
                //open the sql connection
                sql_command.Connection.Open();

                sql_command.CommandText = sql_query;
                lecteurSQL = sql_command.ExecuteReader();

                int cols = lecteurSQL.FieldCount;


                while (lecteurSQL.Read())
                {
                    SQLRow new_row = new SQLRow();

                    for (int i = 0; i < cols; i++)
                    {
                        //if we fail to append the value it is because it is null

                        if (!lecteurSQL.IsDBNull(i))
                        {
                            string res = lecteurSQL.GetValue(i).ToString();
                            res = res.Replace("<", "");
                            res = res.Replace(">", "");
                            new_row.append(res);
                        }
                            
                        else
                            new_row.append("");
                    }

                    row_list.AddLast(new_row);
                }

                sql_command.Connection.Close();

            }

            catch (Exception e)
            {
                sql_command.Connection.Close();
                return e.Message + " " + e.StackTrace;
            }

            int columns;

            try
            {
                columns = row_list.First.Value.ColumnCount;
            }

            catch (Exception e)
            {
                return e.Message + " " + e.StackTrace;
            }

            SQLRow[] row_array = row_list.ToArray<SQLRow>();
            string[,] toReturn = new string[row_array.Length, columns];

            for (int i = 0; i < row_array.Length; i++)
            {
                string[] sql_row_array = row_array[i].Cells.ToArray<string>();

                for (int j = 0; j < columns; j++)
                {
                    toReturn[i, j] = sql_row_array[j];
                }
            }

            return arrayToXML(toReturn).InnerXml;

        }

        private XmlDocument arrayToXML(string[,] Array)
        {
            XmlDocument xml = new XmlDocument();
            XmlDeclaration xdec = xml.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = xml.DocumentElement;
            xml.InsertBefore(xdec, root);
            root = xml.CreateElement(string.Empty, "root", string.Empty);
            xml.AppendChild(root);

            for (int i = 0; i < Array.GetLength(0); i++)
            {

                XmlElement new_row;
                new_row = xml.CreateElement("row");
                root.AppendChild(new_row);

                for (int j = 0; j < Array.GetLength(1); j++)
                {
                    XmlElement new_column;
                    new_column = xml.CreateElement("col");
                    new_column.InnerXml = Array[i, j];
                    new_row.AppendChild(new_column);
                }
            }

            return xml;
        }

        public string SQLTransaction(string sql_query)
        {
            XmlDocument xml = getPostResponseXml();

            try
            {
                ConnectToDB(sqlConnectionString);
            }
            catch (Exception E)
            {
                xml.SelectNodes("root/Accepted")[0].InnerXml = "false";
                xml.SelectNodes("root/Reason")[0].InnerXml = E.Message;
                return xml.InnerXml;
            }

            try
            {
                sql_command.Connection.Open();
                sql_command.CommandText = sql_query;
                sql_command.ExecuteNonQuery();

                sql_command.Connection.Close();
            }
            catch (Exception E)
            {
                sql_command.Connection.Close();
                xml.SelectNodes("root/Accepted")[0].InnerXml = "false";
                xml.SelectNodes("root/Reason")[0].InnerXml = E.Message;
                return xml.InnerXml;
            }

            xml.SelectNodes("root/Accepted")[0].InnerXml = "true";
            return xml.InnerXml;
        }

        private XmlDocument getPostResponseXml()
        {
            XmlDocument xml = new XmlDocument();
            XmlDeclaration xdec = xml.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = xml.DocumentElement;
            xml.InsertBefore(xdec, root);
            root = xml.CreateElement(string.Empty, "root", string.Empty);
            xml.AppendChild(root);

            XmlElement accepted = xml.CreateElement("Accepted");
            root.AppendChild(accepted);
            XmlElement reason = xml.CreateElement("Reason");
            root.AppendChild(reason);
            return xml;

        }
    }
}
