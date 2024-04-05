using System.Configuration;
using static FedconProgramToCAL.PdfToImage;
using static FedconProgramToCAL.CropImageToTable;
using static FedconProgramToCAL.CropImageToColumns;
using static FedconProgramToCAL.CropImageToSlot;
using static FedconProgramToCAL.ExtractTextFromSlotImage;
using static FedconProgramToCAL.AddEventsToDB;
using static FedconProgramToCAL.CalculateEventDuration;
using static FedconProgramToCAL.CreateICAL;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using System.Drawing;
using Windows.Data.Pdf;
using Windows.Storage.Streams;
using Windows.Storage;
using System.Drawing.Imaging;

namespace FedconProgramToCAL
{
    internal class Program
    {
        
        static async Task Main(string[] args)
        {
            string tempfolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\FedconProgramToCAL";
            string programPdfSourceFile = tempfolder + @"\fedcon-programm.pdf";
            string tempTextFile = tempfolder + @"\fedcon-programm.txt";
            string dbFileName = ConfigurationManager.AppSettings["dbFileName"];
            string programUrl = ConfigurationManager.AppSettings["programUrl"];

            int dpi = 2400;
            ImageFormat format = ImageFormat.Bmp;
            string fileExtension = format.ToString().ToLower();

            Directory.CreateDirectory(tempfolder);

            downloadProgram(programUrl, programPdfSourceFile);
            await convertPdfToImage(programPdfSourceFile, tempfolder, dpi, format, fileExtension);
            extractTables(tempfolder, fileExtension, dpi);
            extractColumns(tempfolder, fileExtension, dpi);
            extractSlots(tempfolder, fileExtension, dpi);
            runOCR(tempfolder);
            CreateEvents();
            updateDuration();
            writeICAL(tempfolder);
        }
    }
}

