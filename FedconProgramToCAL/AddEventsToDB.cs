using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static FedconProgramToCAL.DataModel;
using static FedconProgramToCAL.Helpers;


namespace FedconProgramToCAL
{
    public class AddEventsToDB
    {
        public static void CreateEvents()
        {
            DateOnly firstDay = new DateOnly(2024, 5, 10);
            DateTime dateTime = new DateTime(2024, 5, 10, 12, 0, 0);

            //try to get the relevant info from the raw data
            // http://regexstorm.net/tester

            using (var dbContext = new MyDbContext())
            {
                var RawEvents = dbContext.RawEvents.ToList();

                foreach (var rawEvent in RawEvents)
                {
                    DateOnly currentDay = firstDay;
                    // remove line breaks, | and space from the start
                    string input = rawEvent.RawText.Replace("\r\n", " ").Replace("|", "").Replace("{", "").Replace("}", "").TrimStart();

                    // remove additional times from string
                    Regex regexTimes = new Regex(@"\b(\d{2}:\d{2})");
                    Match matchTimes = regexTimes.Match(input);

                    if (matchTimes.Success)
                    {
                        // Extract the first time occurrence
                        string firstTime = matchTimes.Groups[1].Value;

                        // Replace all subsequent time occurrences with an empty string
                        input = regexTimes.Replace(input, m => m.Value == firstTime ? m.Value : "");
                    }

                    // reduce multiple spaces to one
                    input = Regex.Replace(input, @"\s+", " ");

                    // fix [E]
                    input = Regex.Replace(input, @"\[E.{0,2}(?:\s|$)", "[E]");

                    // fix strange chars...
                    input = Regex.Replace(input, @"\[(?![ED])", "");

                    //extract time and text
                    Regex regexInfo = new Regex(@"(\d{2}:\d{2})\s*(.*)");
                    Match matchInfo = regexInfo.Match(input);

                    if (matchInfo.Success)
                    {
                        // Extract the time and the following string
                        string timeString = matchInfo.Groups[1].Value;
                        string followingString = matchInfo.Groups[2].Value.Trim();

                        // Output the extracted time and following string
                        int room = int.Parse(rawEvent.Raum);
                        //var room = (DataModel.Room)roomId;
                        string roomName = GetDisplayName((Room)room);

                        int dayId = int.Parse(rawEvent.Tag);
                        var day = (DataModel.Day)dayId;

                        currentDay = currentDay.AddDays(dayId);

                        if (TimeSpan.TryParse(timeString, out TimeSpan time))
                        {
                            // Create a DateTime object by combining date components with time
                            dateTime = new DateTime(currentDay.Year, currentDay.Month, currentDay.Day, time.Hours, time.Minutes, 0);
                            timeString = dateTime.ToString("dd.MM.yyyy HH:mm");
                        }
                        string title = (string.IsNullOrEmpty(followingString) ? "Default" : followingString);
                        if (title.Contains("MEET & GREETS") || title.Contains("Preise für Gruppenshoots"))
                        {
                            title =  "Default";
                        }

                        /*
                        Console.WriteLine();
                        Console.WriteLine("********************************");
                        Console.WriteLine("Title: " + title);
                        Console.WriteLine("Time: " + timeString);
                        Console.WriteLine("Raum: " + room);
                        Console.WriteLine("Tag: " + day);
                        */

                        using (var dbContextAdd = new MyDbContext())
                        {
                            // Ensure the database is created
                            dbContextAdd.Database.EnsureCreated();

                            // Add data
                            dbContextAdd.Termine.Add(
                                new Termin { Title = title, Room = roomName.ToString(), Start = timeString, Duration = 99, StartTimestamp = dateTime, Day = day.ToString() }
                                );
                            dbContextAdd.SaveChanges();
                        }
                    }
                }
            }
        }
    }
}
