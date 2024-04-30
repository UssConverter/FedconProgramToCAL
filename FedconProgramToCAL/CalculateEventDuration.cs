using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FedconProgramToCAL.DataModel;
using static FedconProgramToCAL.Helpers;

namespace FedconProgramToCAL
{
    internal class CalculateEventDuration
    {
        public static void updateDuration()
        {
            using (var dbContext = new MyDbContext())
            {
                foreach (Day day in Enum.GetValues(typeof(Day)))
                {
                    foreach (Room room in Enum.GetValues(typeof(Room)))
                    {
                        string roomName = GetDisplayName((Room)room);

                        var entries = dbContext.Termine
                         .Where(e => e.Room == roomName.ToString())
                         .Where(e => e.Day == day.ToString())
                         .OrderBy(e => e.StartTimestamp)
                         .ToList();

                        TimeSpan lastDifference = TimeSpan.FromHours(0);

                        for (int i = 0; i < entries.Count - 1; i++)
                        {
                            TimeSpan difference = entries[i + 1].StartTimestamp - entries[i].StartTimestamp;
                            //Console.WriteLine($"Start: {entries[i].StartTimestamp}, End: {entries[i + 1].StartTimestamp}, Difference: {difference}");
                            entries[i].Duration = (int)difference.TotalMinutes;
                            lastDifference = difference;
                        }

                        //Last end Freitag
                        DateTime lastEnd = new DateTime(entries.Last().StartTimestamp.Year, entries.Last().StartTimestamp.Month, entries.Last().StartTimestamp.Day, 23, 0, 0);

                        switch (entries.Last().Day)
                        {
                            case "Samstag":
                                lastEnd = new DateTime(entries.Last().StartTimestamp.Year, entries.Last().StartTimestamp.Month, entries.Last().StartTimestamp.Day, 22, 0, 0);
                                break;
                            case "Sonntag":
                                lastEnd = new DateTime(entries.Last().StartTimestamp.Year, entries.Last().StartTimestamp.Month, entries.Last().StartTimestamp.Day, 18, 0, 0);
                                break;
                        }

                        TimeSpan lastDuration = lastEnd - entries.Last().StartTimestamp;
                        //Console.WriteLine($"Start: {entries.Last().StartTimestamp}, End: {lastEnd}, Difference: {lastDuration}");
                        entries.Last().Duration = (int)lastDuration.TotalMinutes;
                    }
                }
                dbContext.SaveChanges();
            }
        }
    }
}