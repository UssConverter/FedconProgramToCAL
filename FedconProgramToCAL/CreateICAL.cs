using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FedconProgramToCAL.Helpers;


namespace FedconProgramToCAL
{
    public class CreateICAL
    {
        public static void writeICAL(string tempfolder)
        {
            string iCALFolder = tempfolder + @"\iCAL";
            Directory.CreateDirectory(iCALFolder);
            string icalFileMaritim = iCALFolder + @"\fedcon-programm-Maritim.ical";
            string icalFileBeethoven = iCALFolder + @"\fedcon-programm-Beethoven.ical";
            string icalFileHaydn = iCALFolder + @"\fedcon-programm-Haydn.ical";
            string icalFileArndt = iCALFolder + @"\fedcon-programm-Arndt.ical";
            string icalFileAll = iCALFolder + @"\fedcon-programm.ical";

            string icalStart = "BEGIN:VCALENDAR\r\nVERSION:2.0";
            string icalTemplate = "BEGIN:VEVENT\r\nSUMMARY:{SUMMARY}\r\nDTSTART:{DTSTART}\r\nDTEND:{DTEND}\r\nDTSTAMP:{DTSTAMP}\r\nUID:{UID}-FedconProgramToCAL@outlook.com\r\nLOCATION:{LOCATION}\r\nEND:VEVENT";
            string icalEnd = "END:VCALENDAR";

            File.Delete(icalFileMaritim);
            File.Delete(icalFileBeethoven);
            File.Delete(icalFileHaydn);
            File.Delete(icalFileArndt);
            File.Delete(icalFileAll);

            StreamWriter icalWriterMaritim = new StreamWriter(icalFileMaritim);
            StreamWriter icalWriterBeethoven = new StreamWriter(icalFileBeethoven);
            StreamWriter icalWriterHaydn = new StreamWriter(icalFileHaydn);
            StreamWriter icalWriterArndt = new StreamWriter(icalFileArndt);
            StreamWriter icalWriterAll = new StreamWriter(icalFileAll);

            // starting the files
            icalWriterMaritim.WriteLine(icalStart);
            icalWriterBeethoven.WriteLine(icalStart);
            icalWriterHaydn.WriteLine(icalStart);
            icalWriterArndt.WriteLine(icalStart);
            icalWriterAll.WriteLine(icalStart);

            //add content here
            string icalDTstamp = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ"); //20240510T090000Z

            // get content from DB
            using (var dbContext = new MyDbContext())
            {
                var Termine = dbContext.Termine
                      .Where(t => t.Title != "Default")
                      .ToList();

                foreach (var Termin in Termine)
                {
                    //Console.WriteLine($"Id: {Termin.Id}, Name: {Termin.Title}");
                    DateTime dateTime = Termin.StartTimestamp;

                    var icalDTstart = dateTime.ToUniversalTime().ToString("yyyyMMddTHHmmssZ"); //20240510T090000Z
                    var icalDTend = dateTime.AddMinutes(Termin.Duration).ToUniversalTime().ToString("yyyyMMddTHHmmssZ"); //20240510T090000Z

                    // Replace placeholders with actual values
                    string icalEvent = icalTemplate
                        .Replace("{SUMMARY}", Termin.Title)
                        .Replace("{LOCATION}", Termin.Room)
                        .Replace("{DTSTART}", icalDTstart)
                        .Replace("{DTEND}", icalDTend)
                        .Replace("{DTSTAMP}", icalDTstamp)
                        .Replace("{UID}", RemoveSpacesAndSpecialChars(Termin.Title));

                    switch (Termin.Room)
                    {
                        case "Maritim":
                            icalWriterMaritim.WriteLine(icalEvent);
                            break;
                        case "Beethoven":
                            icalWriterBeethoven.WriteLine(icalEvent);
                            break;
                        case "Haydn":
                            icalWriterHaydn.WriteLine(icalEvent);
                            break;
                        case "Arndt - Fotosessions":
                            icalWriterArndt.WriteLine(icalEvent);
                            break;
                    }
                    icalWriterAll.WriteLine(icalEvent);

                }
            }
            // ending the files
            icalWriterMaritim.WriteLine(icalEnd);
            icalWriterBeethoven.WriteLine(icalEnd);
            icalWriterHaydn.WriteLine(icalEnd);
            icalWriterArndt.WriteLine(icalEnd);
            icalWriterAll.WriteLine(icalEnd);

            icalWriterMaritim.Close();
            icalWriterBeethoven.Close();
            icalWriterHaydn.Close();
            icalWriterArndt.Close();
            icalWriterAll.Close();

        }
    }
}
