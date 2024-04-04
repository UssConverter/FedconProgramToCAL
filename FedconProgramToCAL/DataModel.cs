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
            Maritim,

            [Display(Name = "Beethoven")]
            Beethoven,

            [Display(Name = "Haydn")]
            Haydn,

            [Display(Name = "Arndt - Fotosessions")]
            Arndt_Fotosessions
        }

        public enum Day
        {
            Freitag,
            Samstag,
            Sonntag
        }
    }    
}
