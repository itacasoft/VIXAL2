using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace SharpML.Types
{
    public class Utils
    {
        public const int PrefixLength = 27;
        public const int ProgressBarLength = 52;
        private static Action<string, ConsoleColor> outputProcedure;

        public static void RegisterProcedureForOutput(Action<string, ConsoleColor> outputProcedure)
        {
            Utils.outputProcedure = outputProcedure;
        }

        public static string CreateProgressBar(int length, double percent)
        {
            // Minus place for number of percent
            length -= 7;

            int left = (int)(length * percent / 100);
            int right = length - left;

            string progressBar = "";
            progressBar += "[";
            progressBar += new String('=', left);
            progressBar += new String(' ', right);
            progressBar += "]";
            progressBar += " " + String.Format("{0,3}", Math.Round(percent)) + "%";

            return progressBar;
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        public static void DrawMessage(string prefix, string message, ConsoleColor color)
        {
            if (outputProcedure != null)
            {
                outputProcedure((prefix.PadRight(Utils.PrefixLength) + message).PadRight(Console.WindowWidth - 1), color);
            }
            else if (GetConsoleWindow() != IntPtr.Zero)
            {
                Console.CursorLeft = 0;
                Console.ForegroundColor = color;
                Console.Write((prefix.PadRight(Utils.PrefixLength) + message).PadRight(Console.WindowWidth - 1));
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public static void DrawMessage(string message)
        {
            DrawMessage("", message, ConsoleColor.Gray);
        }

        public static int PercentIntervalByLength(int length)
        {
            return Math.Max(1, (int)Math.Round(length / 100.0));
        }

        public static double ToInterval(double value, double newMin, double newMax, double oldMin, double oldMax)
        {
            if (oldMax - oldMin == 0)
                return (newMax - newMin) / 2;

            return (value - oldMin) * (newMax - newMin) / (oldMax - oldMin) + newMin;
        }

        public static double[][] LoadCsv(string filename)
        {
            return File.ReadAllLines(filename).Select(x => x.Split(new char[] { ';' }).Select(y => double.Parse(y)).ToArray()).ToArray();
        }

        public static string[][] LoadCsvAsStrings(string filename, int colCount = 0)
        {
            string[][] result0 = File.ReadAllLines(filename).Select(x => x.Split(new char[] { ';' })).ToArray();

            if (colCount == 0)
                return result0;

            string[][] result = new string[result0.Length][];
            for (int row = 0; row < result0.Length; row++)
            {
                result[row] = new string[Math.Min(result0[row].Length, colCount)];
                for (int col = 0; col < Math.Min(result0[row].Length, colCount); col++)
                {
                    result[row][col] = result0[row][col];
                }
            }

            return result;
        }

        public static void WriteCsv(double[][] data, string path)
        {
            List<string> content = new List<string>();
            for (int i = 0; i < data.Length; i++)
            {
                string s = "";
                for (int j=0; j<data[i].Length; j++)
                {
                    s += Convert.ToString(data[i][j]);
                    if (j < (data[i].Length-1)) 
                        s += ';';
                }
                content.Add(s);
            }

            File.WriteAllLines(path, content);
        }

        public static void WriteCsv(string[] names, double[] values, string path)
        {
            List<string> content = new List<string>();
            for (int i = 0; i < values.Length; i++)
            {
                string s = "";
                s += Convert.ToString(names[i]);
                s += ';';
                s += values[i];
                content.Add(s);
            }

            File.WriteAllLines(path, content);
        }

        public static void WriteCsv(double[] data, string path)
        {
            double[][] input = new double[data.Length][];
            for (int i = 0; i < data.Length; i++)
            {
                input[i] = new double[1];
                input[i][0] = data[i];
            }

            WriteCsv(input, path);
        }

        public static double[] LoadVector(string filename)
        {
            double[][] a = System.IO.File.ReadAllLines(filename).Select(x => x.Split(new char[] { ';' }).Select(y => double.Parse(y)).ToArray()).ToArray();
            double[] result = new double[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                result[i] = a[i][0];
            }
            return result;
        }


        public static double[] GetVectorFromArray(double[][] input, int column)
        {
            double[] result = new double[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                result[i] = input[i][column];
            }
            return result;
        }

        public static void SetArrayFromVector(double[] input, int column, double[][] output)
        {
            double[] result = new double[input.Length];
            for (int row = 0; row < input.Length; row++)
            {
                output[row][column] = input[row];
            }
        }

        public static double StandardDeviation(IEnumerable<double> values)
        {
            double result = 0;

            if (values.Any())
            {
                double average = values.Average();
                double sum = values.Sum(d => System.Math.Pow(d - average, 2));
                result = System.Math.Sqrt((sum) / values.Count());
            }
            return result;
        }

        public static double Mean(double[] values)
        {
            double sum = 0.0;

            for (int i = 0; i < values.Length; i++)
                sum += values[i];

            return sum / values.Length;
        }

        public static long ToUnixTimestamp(DateTime value)
        {
            return (long)(value.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        public static float[][] ToFloatArray(double[][] doubleArray)
        {
            float[][] floatArray = new float[doubleArray.Length][];
            for (int row = 0; row < doubleArray.Length; row++)
            {
                floatArray[row] = new float[doubleArray[row].Length];
                for (int col = 0; col < doubleArray[row].Length; col++)
                {
                    floatArray[row][col] = (float)doubleArray[row][col];
                }
            }
            return floatArray;
        }

        public static float[] ToFloatArray(double[] doubleArray)
        {
            float[] floatArray = new float[doubleArray.Length];
            for (int row = 0; row < doubleArray.Length; row++)
            {
                 floatArray[row] = (float)doubleArray[row];
            }
            return floatArray;
        }
    }
}
