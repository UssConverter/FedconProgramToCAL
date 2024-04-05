using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tesseract;
using static FedconProgramToCAL.DataModel;

namespace FedconProgramToCAL
{
    internal class ExtractTextFromSlotImage
    {
        public static void runOCR(string tempfolder)
        {
            bool debug = true;
            string tessdataDir = ConfigurationManager.AppSettings["tessdataDir"];

            // go trough all day image folders and extract the events from each column image
            List<string> dayFolders = new List<string>(Directory.GetDirectories(tempfolder + @"\days"));

            foreach (string dayFolder in dayFolders)
            {
                string[] dayPath = dayFolder.Split('\\');
                string day = dayPath[dayPath.Length - 1];

                // get room folders of the day
                List<string> roomFolders = new List<string>(Directory.GetDirectories(dayFolder));
                foreach (string roomFolder in roomFolders)
                {
                    string[] roomPath = roomFolder.Split('\\');
                    string room = roomPath[roomPath.Length - 1];

                    List<string> events = new List<string>(Directory.GetFiles(roomFolder));
                    events.Sort(new FileNameComparer());

                    foreach (string item in events)
                    {
                        using (var engine = new TesseractEngine(tessdataDir, "eng+deu", EngineMode.Default))
                        {
                            using (var image = new System.Drawing.Bitmap(item))                            
                            {
                                using (var pic = PixConverter.ToPix(image))
                                //using (var pic = Pix.LoadFromFile(item))
                                {
                                    PageSegMode pageSegMode = PageSegMode.SingleBlock; // You can change this to the desired mode SparseText

                                    using (var page = engine.Process(pic, pageSegMode))
                                    {
                                        string extractedText = page.GetText();
                                        Console.WriteLine(extractedText);

                                        //var optimizedImage = page.GetThresholdedImage();
                                        //string optimizedImagePath = item + ".png";
                                        //optimizedImage.Save(optimizedImagePath, ImageFormat.Png);


                                        using (var dbContext = new MyDbContext())
                                        {
                                            // Ensure the database is created
                                            dbContext.Database.EnsureCreated();

                                            // Add data
                                            dbContext.RawEvents.Add(new RawEvent { Raum = room, RawText = extractedText, Tag = day, File = item });
                                            dbContext.SaveChanges();
                                        }
                                    }
                                }
                            }
                        }

                        if (debug) Console.WriteLine(item);
                    }
                }
            }

        }
    }
}
