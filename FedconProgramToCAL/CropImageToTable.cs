using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using ImageMagick;
using static FedconProgramToCAL.Helpers;
using static FedconProgramToCAL.DataModel;

namespace FedconProgramToCAL
{
    internal class CropImageToTable
    {
        public static void extractTables(string tempfolder)
        {
            bool debug = false;

            List<string> pages = new List<string>(Directory.GetFiles(tempfolder, "*.bmp"));

            // find the table outline and crop the image - replacing the original page extract
            foreach (var page in pages)
            {
                // storing the line point coordinates
                List<int> valX = new List<int>();
                List<int> valY = new List<int>();

                // Load the image and get its size
                Mat image = CvInvoke.Imread(page);
                Size pageInfo = image.Size;
                int centerX = pageInfo.Width / 2;
                int centerY = pageInfo.Height / 2;

                // set color and run canny edge detection
                Mat gray = new Mat();
                CvInvoke.CvtColor(image, gray, ColorConversion.Bgr2Gray);
                Mat edges = new Mat();
                
                CvInvoke.Canny(gray, edges, 50, 150, 5);

                if (debug)
                {
                    CvInvoke.Imshow("Canny", edges);
                    CvInvoke.WaitKey();
                }

                // run higes line detection, to get horizontal lines
                LineSegment2D[] lines = CvInvoke.HoughLinesP(edges, 1, Math.PI / 180, 25, 3400, 15);
                if (debug) debugDrawLines(image, lines, DisplayDirection.All);
                
                // Add the line point to lists for manipulation
                foreach (LineSegment2D line in lines)
                {
                    if (Math.Abs(line.P1.Y - line.P2.Y) < 5) // Assuming horizontal lines have small angle difference - offset between start and endpoint
                    {
                        valY.Add(line.P1.Y);
                    }
                }

                // get the closest X and Y points to the center of the image
                (int? closestYLower, int? closestYHigher) = FindClosestValues(valY, centerY);

                // cropping the image
                int? startX = 0;
                int? startY = closestYLower - 2;
                int? newWidth = pageInfo.Width;
                int? newHeight = closestYHigher - closestYLower + 4;

                using (MagickImage cropPage = new MagickImage(page))
                {
                    cropPage.Crop(new MagickGeometry((int)startX, (int)startY, (int)newWidth, (int)newHeight));
                    cropPage.Write(page);
                }

                // Load the cropped image again and get its size
                image = CvInvoke.Imread(page);
                pageInfo = image.Size;
               
                // set color and run canny edge detection
                gray = new Mat();
                CvInvoke.CvtColor(image, gray, ColorConversion.Bgr2Gray);
                edges = new Mat();
                CvInvoke.Canny(gray, edges, 50, 150, 5);

                if (debug)
                {
                    CvInvoke.Imshow("Canny", edges);
                    CvInvoke.WaitKey();
                }

                // run higes line detection, to get horizontal lines
                lines = CvInvoke.HoughLinesP(edges, 1, Math.PI / 180, 25, 1100, 15);

                if (debug) debugDrawLines(image, lines, DisplayDirection.All);
                
                // Add the line point to lists for manipulation
                foreach (LineSegment2D line in lines)
                {
                    if (Math.Abs(line.P1.X - line.P2.X) < 5) // Assuming vertical lines have small angle difference - offset between start and endpoint
                    {
                        valX.Add(line.P1.X);
                    }
                }

                // cropping the image again
                startX = valX.Min() - 2;
                startY = 0;
                newWidth = valX.Max() - startX + 4;
                newHeight = pageInfo.Height;

                using (MagickImage cropPage = new MagickImage(page))
                {
                    cropPage.Crop(new MagickGeometry((int)startX, (int)startY, (int)newWidth, (int)newHeight));
                    cropPage.Write(page);
                }
            }
        }
    }
}
