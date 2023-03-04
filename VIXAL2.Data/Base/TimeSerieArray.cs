using Accord.Math;
using SharpML.Types;
using SharpML.Types.Normalization;
using System;
using System.Linq;

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

        public DateTime[] Dates
        {
            get
            {
                return dates;
            }
        }

        internal string[] ColNames
        {
            get
            {
                return colNames;
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

        public Double MaxValue
        {
            get
            {
                return values.GetColumn<double>(0).Max();
            }
        }

        public Double MinValue
        {
            get
            {
                return values.GetColumn<double>(0).Min();
            }
        }

        public double[] this[int index]
        {
            get
            {
                return this.Values[index];
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

        /// <summary>
        /// Returns an array of a single column
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public double[] GetColumnValues(int column = 0)
        {
            return SharpML.Types.Utils.GetVectorFromArray(values, column);
        }

        /// <summary>
        /// Returns the value at a given date
        /// </summary>
        /// <param name="date"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public double GetValue(DateTime date, int column)
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

        /// <summary>
        /// Returns an array containing values from "date" for "lenght" days, exluding current
        /// </summary>
        /// <param name="date"></param>
        /// <param name="lenght"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public double[] GetPreviousValuesFromColumn(DateTime date, int lenght, int column)
        {
            double[] result = new double[lenght];

            //int i = 0;
            DateTime mydate = this.GetPreviousDate(date, lenght).Value;

            for (int i=0; i<lenght; i++)
            {
                result[i] = this.GetValue(mydate, column);
                mydate = this.GetNextDate(mydate).Value;
            }
            return result;
        }

        /// <summary>
        /// Returns an array containing values from "date" for "lenght" days, including current
        /// </summary>
        /// <param name="date"></param>
        /// <param name="lenght"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public double[] GetPreviousValuesFromColumnIncludingCurrent(DateTime date, int lenght, int column)
        {
            double[] result = new double[lenght];

            //int i = 0;
            DateTime mydate = this.GetPreviousDate(date, lenght-1).Value;

            for (int i = 0; i < lenght; i++)
            {
                mydate = this.GetNextDate(mydate).Value;
                result[i] = this.GetValue(mydate, column);
            }
            return result;
        }


        /// <summary>
        /// Returns an array containing values from "date" for "lenght" days
        /// </summary>
        /// <param name="date"></param>
        /// <param name="lenght"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public double[] GetNextValuesFromColumn(DateTime date, int lenght, int column)
        {
            double[] result = new double[lenght];

            DateTime mydate = this.GetNextDate(date).Value;

            for (int i = 0; i < lenght; i++)
            {
                result[i] = this.GetValue(mydate, column);
                mydate = this.GetNextDate(mydate).Value;
            }
            return result;
        }


        public void SetColName(int col, string name)
        {
            colNames[col] = name;
        }

        public string GetColName(int col)
        {
            return colNames[col];
        }

        /// <summary>
        /// Returns get next date present in the date list
        /// </summary>
        /// <param name="date"></param>
        /// <param name="shift"></param>
        /// <returns></returns>
        public DateTime? GetNextDate(DateTime date, int shift = 1)
        {
            for (int row = 0; row < dates.Length; row++)
            {
                if (dates[row] == date)
                {
                    if ((row + shift) < dates.Length)
                        return dates[row + shift];
                    else
                        return null;
                }
            }

            return null;
        }

        public int? DateToSampleIndex(DateTime date)
        {
            for (int row = 0; row < dates.Length; row++)
            {
                if (dates[row] == date)
                {
                    return row;
                }
            }

            return null;
        }

        public DateTime? SampleIndexToDate(int sampleIndex)
        {
            return dates[sampleIndex];
        }


        /// <summary>
        /// Returns the previous date present in the date list
        /// </summary>
        /// <param name="date"></param>
        /// <param name="shift"></param>
        /// <returns></returns>
        public DateTime? GetPreviousDate(DateTime date, int shift = 1)
        {
            for (int row = 0; row < dates.Length; row++)
            {
                if (dates[row] == date)
                {
                    if ((row - shift) >= 0)
                        return dates[row - shift];
                    else
                        return null;
                }
            }

            return null;
        }

        public override string ToString()
        {
            return GetColName(0);
        }
    }
}
