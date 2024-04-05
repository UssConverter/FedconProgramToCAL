using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV;
using ImageMagick;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using static FedconProgramToCAL.Helpers;
using static FedconProgramToCAL.DataModel;


namespace FedconProgramToCAL
{
    internal class CropImageToSlot
    {
        public static void extractSlots(string tempfolder, string fileExtension, int dpi)
        {
            bool debug = false;

            for (int d = 0; d < 3; d++)
            {
                // source folder for the day
                string currDir = tempfolder + "\\days\\" + d;

                // get images in folder
                List<string> columns = new List<string>(Directory.GetFiles(currDir, "*." + fileExtension));

                // go through all column images of this day
                foreach (var column in columns)
                {
                    // events will be saved as single images in the room folder of the day
                    string roomDir = currDir + "\\" + Path.GetFileNameWithoutExtension(column);
                    Directory.CreateDirectory(roomDir);
                    
                    // storing the line point coordinates
                    List<Tuple<int, int>> tupleLineStart = new List<Tuple<int, int>>();

                    // Load the image and get its size
                    Mat image = CvInvoke.Imread(column);
                    Size pageInfo = image.Size;

                    // set color and run canny edge detection
                    Mat gray = new Mat();
                    CvInvoke.CvtColor(image, gray, ColorConversion.Bgr2Gray);
                    Mat edges = new Mat();
                    CvInvoke.Canny(gray, edges, 1, 1000, 5);

                    if (debug)
                    {
                        CvInvoke.Imshow("Canny", edges);
                        CvInvoke.WaitKey();
                    }

                    // run higes line detection, to get lines
                    double minLineLenght = 1 * dpi;
                    double maxGap = 0.005 * dpi;
                    LineSegment2D[] lines = CvInvoke.HoughLinesP(edges, 2, Math.PI / 180, 50, minLineLenght, maxGap);

                    if (debug) debugDrawLines(image, lines, DisplayDirection.All);                    

                    // Add the line points to lists for manipulation
                    foreach (LineSegment2D line in lines)
                    {
                        if (Math.Abs(line.P1.Y - line.P2.Y) < 5) // Assuming horizontal lines have small angle difference - offset between start and endpoint
                        {
                            tupleLineStart.Add(new Tuple<int, int>(line.P1.X, line.P1.Y));
                        }
                    }

                    // sorting
                    tupleLineStart = tupleLineStart.OrderBy(t => t.Item2).ToList();

                    // all horizontal lines should start from the same X coordinate - find the common value
                    var mostCommonValue = tupleLineStart.GroupBy(t => t.Item1).OrderByDescending(g => g.Count()).First().Key;

                    // remove lines that start a little behind - this are the info boxes
                    double mCLower = 0.025 * dpi;
                    double mCUpper = 0.083333 * dpi;
                    tupleLineStart.RemoveAll(t => t.Item1 > (mostCommonValue + (int)mCLower) && t.Item1 < (mostCommonValue + (int)mCUpper));
                    // remove lines that start at the top or bottom of the page
                    double pageMargin = 0.083333 * dpi;
                    tupleLineStart.RemoveAll(t => t.Item2 < (int)pageMargin || t.Item2 > (pageInfo.Height - (int)pageMargin));
                    // sorting
                    tupleLineStart = tupleLineStart.OrderBy(t => t.Item2).ToList();

                    // remove points that are to close together
                    double sameMargin = 0.083333 * dpi;
                    List<Tuple<int, int>> distinctTuples = RemoveAlmostDuplicates(tupleLineStart, (int)sameMargin);
                    distinctTuples = distinctTuples.OrderBy(t => t.Item2).ToList();

                    if (debug)
                    {
                        // draw the filtered lines to the column image
                        foreach (var tuple in distinctTuples)
                        {
                            CvInvoke.Line(image, TupleToPoint(tuple), TupleToPoint(tuple, 300, 0), new MCvScalar(0, 0, 255), 2);
                        }
                        CvInvoke.NamedWindow("Lines Optimised", WindowFlags.KeepRatio);
                        CvInvoke.Imshow("Lines Optimised", image);
                    }

                    // extract the events
                    for (int e = 0; e <= distinctTuples.Count; e++)
                    {
                        // file name will be number only for easy sorting later
                        string newName = roomDir + "\\" + e.ToString() + Path.GetExtension(column);

                        // calculate crop size
                        int startY = 0;
                        int height = 0;
                        int width = pageInfo.Width; // will always be the original image width

                        if (e == 0)
                        {
                            height = distinctTuples[e].Item2;
                        }
                        else if (e == distinctTuples.Count)
                        {
                            startY = distinctTuples[e - 1].Item2;
                            height = pageInfo.Height - distinctTuples[e - 1].Item2;
                        }
                        else
                        {
                            startY = distinctTuples[e - 1].Item2;
                            height = distinctTuples[e].Item2 - distinctTuples[e - 1].Item2;
                        }

                        // crop and save
                        using (MagickImage cropEvent = new MagickImage(column))
                        {
                            cropEvent.Crop(new MagickGeometry(0, startY, width, height));
                            //cropEvent.ColorType = ColorType.Grayscale;
                            cropEvent.Resize(new Percentage(500));
                            cropEvent.Write(newName);
                            Console.WriteLine(newName);
                        }
                    }
                }
            }
        }
    }
}
