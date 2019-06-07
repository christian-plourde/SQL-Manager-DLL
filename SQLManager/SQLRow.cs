using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLManager
{
    public class SQLRow
    {
        LinkedList<string> cell_data;
        int column_count;

        public LinkedList<string> Cells
        {
            get
            {
                return cell_data;
            }
        }

        public int ColumnCount
        {
            get { return column_count; }
        }


        public SQLRow()
        {
            cell_data = new LinkedList<string>();
            column_count = 0;
        }

        public void append(string s)
        {
            cell_data.AddLast(s);
            column_count++;
        }

    }
}
