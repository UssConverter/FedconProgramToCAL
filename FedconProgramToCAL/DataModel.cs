using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FedconProgramToCAL
{
    public class DataModel
    {
        public enum DisplayDirection
        {
            All,
            VerticalOnly,
            HorizontalOnly
        }

        public enum Room
        {
            [Display(Name = "Maritim")]
            Maritim = 0,

            [Display(Name = "Beethoven")]
            Beethoven = 1,

            [Display(Name = "Haydn")]
            Haydn = 2,

            [Display(Name = "Arndt-Fotosessions")]
            Arndt_Fotosessions = 3
        }

        public enum Day
        {
            Freitag = 0 ,
            Samstag = 1,
            Sonntag = 2
        }
    }
    public class Termin
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Start { get; set; }
        public DateTime StartTimestamp { get; set; } // DateTime property
        public int Duration { get; set; }
        public string Room { get; set; }
        public string Day { get; set; }
    }

    public class RawEvent
    {
        public int Id { get; set; }
        public string RawText { get; set; }
        public string Raum { get; set; }
        public string Tag { get; set; }
        public string File { get; set; }
    }

    public class MyDbContext : DbContext
    {
        public DbSet<Termin> Termine { get; set; }
        public DbSet<RawEvent> RawEvents { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite(GetConnectionString());
            }
        }

        private string GetConnectionString()
        {
            return ConfigurationManager.AppSettings["dbFileName"];

        }
    }
}
