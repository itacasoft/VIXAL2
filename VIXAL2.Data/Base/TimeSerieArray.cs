using SharpML.Types;
using SharpML.Types.Normalization;
using System;

namespace VIXAL2.Data.Base
{
    public class TimeSerieArray
    {
        double[][] values;
        DateTime[] dates;
        string[] colNames;

        public TimeSerieArray(int rows, int cols)
        {
            values = new double[rows][];
            for (int i = 0; i < rows; i++)
            {
                values[i] = new double[cols];
            }
            dates = new DateTime[rows];
            colNames = new string[cols];
            for (int i = 0; i < cols; i++)
            {
                colNames[i] = "col" + (i+1).ToString();
            }
        }

        public TimeSerieArray(string[] stockNames, DateTime[] dates, double[][] allData)
        {
            int rows = allData.Length;
            int cols = stockNames.Length;

            values = new double[rows][];
            for (int i = 0; i < rows; i++)
            {
                values[i] = new double[cols];
            }
            this.dates = new DateTime[rows];
            for (int r = 0; r < rows; r++)
            {
                this.dates[r] = dates[r];
            }

            colNames = new string[stockNames.Length];
            for (int i=0; i<cols; i++)
            {
                colNames [i] = stockNames [i];
            }

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    SetValue(r, c, this.dates[r], allData[r][c]);
                }
            }
        }

        public double[][] Values
        {
            get
            {
                return values;
            }
        }

        public int Length
        {
            get { return values.Length; }
        }

        public int Rows
        {
            get { return values.Length; }
        }

        public int Columns
        {
            get { return values[0].Length; }
        }


        public void SetValue(int row, int col, DateTime date, double value)
        {
            values[row][col] = value;
            dates[row] = date;
        }

        public DateTime GetDate(int row)
        {
            return dates[row];
        }

        public DateTime MinDate
        {
            get
            {
                return dates[0];
            }
        }

        public DateTime MaxDate
        {
            get
            {
                return dates[dates.Length - 1];
            }
        }

        public double GetValueOnMaxDate(int col = 0)
        {
            return GetValue(dates[dates.Length - 1], col);
        }

        public double GetValueOnMinDate(int col = 0)
        {
            return GetValue(dates[0], col);
        }

        public void DecodeValues(INormalizer normalizer)
        {
            values = normalizer.Decode(values);
        }

        public double[] GetColumnValues(int column = 0)
        {
            return Utils.GetVectorFromArray(values, column);
        }

        public double GetValue(DateTime date, int column = 0)
        {
            for (int row = 0; row < dates.Length; row++)
            {
                if (dates[row] == date)
                {
                    return values[row][column];
                }
            }

            return Double.NaN;
        }

        public void SetColName(int col, string name)
        {
            colNames[col] = name;
        }

        public string GetColName(int col)
        {
            return colNames[col];
        }
    }
}
