using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FedconProgramToCAL.DataModel;

namespace FedconProgramToCAL
{
    internal class Helpers
    {
        public static (int?, int?) FindClosestValues(List<int> list, int x)
        {
            if (list == null || list.Count == 0)
            {
                throw new ArgumentException("List is null or empty.");
            }

            int? closestLower = null;
            int? closestHigher = null;

            foreach (int num in list)
            {
                if (num < x && (!closestLower.HasValue || num > closestLower))
                {
                    closestLower = num;
                }
                else if (num > x && (!closestHigher.HasValue || num < closestHigher))
                {
                    closestHigher = num;
                }
            }

            return (closestLower, closestHigher);
        }

        public static List<Tuple<int, int>> RemoveAlmostDuplicates(List<Tuple<int, int>> tupleList, int threshold)
        {
            // Sort the list by the second item of each tuple
            var sortedList = tupleList.OrderBy(t => t.Item2).ToList();

            // Remove almost duplicate tuples
            var distinctTuples = new List<Tuple<int, int>>();
            distinctTuples.Add(sortedList[0]);

            for (int i = 1; i < sortedList.Count; i++)
            {
                if (sortedList[i].Item2 - distinctTuples.Last().Item2 >= threshold)
                {
                    distinctTuples.Add(sortedList[i]);
                }
            }

            return distinctTuples;
        }
        public static List<int> RemoveAlmostDuplicates(List<int> values, int threshold)
        {
            List<int> result = new List<int>(values);
            List<int> toRemove = new List<int>();

            for (int i = 0; i < result.Count; i++)
            {
                for (int j = i + 1; j < result.Count; j++)
                {
                    if (Math.Abs(result[i] - result[j]) <= threshold)
                    {
                        // Mark for removal the element that is further from the middle
                        int middle = (result[i] + result[j]) / 2;
                        if (Math.Abs(middle - result[i]) < Math.Abs(middle - result[j]))
                        {
                            toRemove.Add(j);
                        }
                        else
                        {
                            toRemove.Add(i);
                        }
                    }
                }
            }

            // Remove marked elements in reverse order to avoid invalidating indices
            toRemove.Sort();
            for (int i = toRemove.Count - 1; i >= 0; i--)
            {
                result.RemoveAt(toRemove[i]);
            }

            return result;
        }

        public static System.Drawing.Point TupleToPoint(Tuple<int, int> tuple)
        {
            return new System.Drawing.Point(tuple.Item1, tuple.Item2);
        }

        public static System.Drawing.Point TupleToPoint(Tuple<int, int> tuple, int offsetX, int offsetY)
        {
            return new System.Drawing.Point(tuple.Item1 + offsetX, tuple.Item2 + offsetY);
        }

        public static void debugDrawLines(Mat image, LineSegment2D[] lines, DisplayDirection direction)
        {
            // count found lines
            int c = 0;
            // Draw detected lines on the original image
            foreach (LineSegment2D line in lines)
            {
                if (direction == DisplayDirection.VerticalOnly || direction == DisplayDirection.All)
                {
                    if (Math.Abs(line.P1.X - line.P2.X) < 5) // Assuming vetical lines have small angle difference - offset between start and endpoint
                    {
                        debugDrawLinesOutput(image, line);
                        c++;
                    }
                }
                if (direction == DisplayDirection.HorizontalOnly || direction == DisplayDirection.All)
                {
                    if (Math.Abs(line.P1.X - line.P2.X) < 5) // Assuming vetical lines have small angle difference - offset between start and endpoint
                    {
                        debugDrawLinesOutput(image, line);
                        c++;
                    }
                }
            }
            Console.WriteLine("Anzahl Linien: " + c.ToString());
            CvInvoke.NamedWindow("Lines", WindowFlags.KeepRatio);
            CvInvoke.Imshow("Lines", image);
            CvInvoke.WaitKey();
        }

        public static void debugDrawLinesOutput(Mat image, LineSegment2D line)
        {
            CvInvoke.Line(image, line.P1, line.P2, new MCvScalar(0, 0, 255), 2);
            Console.WriteLine($"Line Point 1: ({line.P1.X}, {line.P1.Y})");
            Console.WriteLine($"Line Point 2: ({line.P2.X}, {line.P2.Y})");
            Console.WriteLine("*******************************************");
        }
    }

    public class FileNameComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            // Extract numeric parts from file names
            int xNumber = ExtractNumber(x);
            int yNumber = ExtractNumber(y);

            // Compare numeric parts as integers
            return xNumber.CompareTo(yNumber);
        }

        private int ExtractNumber(string fileName)
        {
            string numberString = string.Concat(fileName.Where(char.IsDigit));
            return int.Parse(numberString);
        }
    }

}