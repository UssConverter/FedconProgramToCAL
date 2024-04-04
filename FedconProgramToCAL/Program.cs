using System.Configuration;
using static FedconProgramToCAL.PdfToImage;
using static FedconProgramToCAL.CropImageToTable;
using static FedconProgramToCAL.CropImageToColumns;
using static FedconProgramToCAL.CropImageToSlot;
using Microsoft.Extensions.Configuration;

namespace FedconProgramToCAL
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string tempfolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\FedconProgramToCAL";
            string programPdfSourceFile = tempfolder + @"\fedcon-programm.pdf";
            string dbFileName = ConfigurationManager.AppSettings["dbFileName"];
            string programUrl = ConfigurationManager.AppSettings["programUrl"];

            downloadProgram(programUrl, programPdfSourceFile);
            await convertPdfToImage(programPdfSourceFile, tempfolder);
            extractTables(tempfolder);
            extractColumns(tempfolder);
            extractSlots(tempfolder);

        }
    }
}

