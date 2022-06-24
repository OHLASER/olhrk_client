using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace olhrkcl
{
    internal class SimpleStrTable
    {

        /// <summary>
        /// keep row
        /// </summary>
        IList<string[][]> Rows;

        /// <summary>
        /// Display width
        /// </summary>
        internal uint DisplayWidth;

        /// <summary>
        /// Number of spaces among columns.
        /// </summary>
        internal uint Paddings;


        internal SimpleStrTable()
        {
            Rows = new List<string[][]>();
            Paddings = 4;
            DisplayWidth = 80;
        }

        /// <summary>
        /// add column1, column2
        /// </summary>
        /// <param name="column1"></param>
        /// <param name="column2"></param>
        internal void Add(string[] column1, string[] column2)
        {
            Rows.Add(new string[][] { column1, column2 });
        }


        public override string ToString()
        {

            string[] lines;

            lines = Format();

            return string.Join("\n", lines);

        }

        /// <summary>
        /// format table
        /// </summary>
        /// <returns></returns>
        internal string[] Format()
        {

            IList<string> lines;
            lines = new List<string>();

            string[] formats;
            formats = GetColumnsFormat();

            string padding;
            padding = string.Format("{0," + Paddings + "}", "");

            foreach (var row in Rows)
            {
                int maxCount;
                maxCount = Math.Max(row[0].Length, row[1].Length);
                for (int lineIndex = 0; lineIndex < maxCount; lineIndex++)
                {
                    string column1;
                    if (lineIndex < row[0].Length )
                    {
                        column1 = row[0][lineIndex];
                    }
                    else
                    {
                        column1 = "";
                    }
                    string column2;
                    if (lineIndex < row[1].Length)
                    {
                        column2 = row[1][lineIndex];
                    }
                    else
                    {
                        column2 = "";
                    }



                    lines.Add(string.Format(padding + formats[0] + padding + formats[1], column1, column2));
                }

            }


            return lines.ToArray();
        }


        internal string[] GetColumnsFormat()
        {
            string[] result;

            int column1Width;
            column1Width = (int)CalcColumn1Width();
            result = new string[2];

            result[0] = "{0," + (-(int)column1Width) + "}";
            result[1] = "{1}";


            return result;
        }


        /// <summary>
        /// calculate column1 width
        /// </summary>
        /// <returns></returns>
        internal uint CalcColumn1Width()
        {

            uint result;
            result = 0;
            foreach (var row in Rows)
            {
                result = Math.Max(result, CalcStrWidth(row[0]));
            }

            return result;
        }



        internal uint CalcStrWidth(string[] lines)
        {

            uint result;
            result = 0;

            foreach (var line in lines)
            {
                bool surrogate;
                surrogate = false;
                uint charCount;
                charCount = 0;
                foreach (var aValue in line)
                {
                    if (surrogate)
                    {
                        surrogate = false;
                    }
                    else
                    {
                        surrogate = Char.IsSurrogate(aValue);
                        charCount++;
                    }
                }
                result = Math.Max(result, charCount);
            }

            return result;
        }


    }
}
