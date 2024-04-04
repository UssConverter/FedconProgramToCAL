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
    internal class CropImageToColumns
    {
        public static void extractColumns(string tempfolder)
        {
            bool debug = false;
            string outputFolder = tempfolder + @"\days";

            List<string> pages = new List<string>(Directory.GetFiles(tempfolder + @"\", "*.bmp"));

            // Crop Image to colums
            foreach (var page in pages)
            {
                // storing the line X point coordinates
                List<int> valX = new List<int>();

                // Load the image and get its size
                Mat image = CvInvoke.Imread(page);
                Size pageInfo = image.Size;

                // set color and run canny edge detection
                Mat gray = new Mat();
                CvInvoke.CvtColor(image, gray, ColorConversion.Bgr2Gray);
                Mat edges = new Mat();
                CvInvoke.Canny(gray, edges, 50, 150);

                if (debug)
                {
                    CvInvoke.Imshow("Canny", edges);
                    CvInvoke.WaitKey();
                }

                // run higes line detection, to get lines
                LineSegment2D[] lines = CvInvoke.HoughLinesP(edges, 1, Math.PI / 180, 25, 1200, 3);

                if (debug) debugDrawLines(image, lines, DisplayDirection.VerticalOnly);
                
                // Add the line point of vertical lines to lists for manipulation
                foreach (LineSegment2D line in lines)
                {
                    if (Math.Abs(line.P1.X - line.P2.X) < 5) // Assuming vetical lines have small angle difference - offset between start and endpoint
                    {
                        valX.Add(line.P1.X);
                    }
                }

                // remove duplicates
                List<int> distinctValX = RemoveAlmostDuplicates(valX, 5);

                // remove the line at the start and at the end
                distinctValX = distinctValX.Where(n => n >= 25 && n <= pageInfo.Width-25).ToList();

                // Sort Points by X Coordinate Value
                distinctValX.Sort();

                // extract all colums from the source image and save to folder with source image name (event day)
                int end = 1;
                int startX = 0;
                int width = distinctValX[end];

                for (int i = 0; i < 4; i++)
                {
                    string newName = outputFolder + "\\" + Path.GetFileNameWithoutExtension(page) + "\\" + i + Path.GetExtension(page);
                    Directory.CreateDirectory(outputFolder + "\\" + Path.GetFileNameWithoutExtension(page));
                   
                    // calculate starting point and image size for each column
                    if (i > 0 && i < 3)
                    {
                        startX = distinctValX[end];
                        end = end + 2;
                        width = distinctValX[end] - startX;

                    }                    
                    if (i == 3)
                    {
                        startX = distinctValX[end];
                        width = pageInfo.Width - startX;
                    }

                    // crop the image and save
                    using (MagickImage cropColumn = new MagickImage(page))
                    {
                        // Crop the image
                        cropColumn.Crop(new MagickGeometry(startX, 0, width, pageInfo.Height));
                        // Save the cropped image
                        cropColumn.Write(newName);
                    }
                }
            }
        }
    }
}
