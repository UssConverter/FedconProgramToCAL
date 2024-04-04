using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;
using Windows.Data.Pdf;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using System.Net;
using Windows.Storage;
using System.Drawing;
using System.Drawing.Imaging;

namespace FedconProgramToCAL
{
    internal class PdfToImage
    {
        public static async Task convertPdfToImage(string programPdfSourceFile, string imageDestinationFolder)
        {
            Directory.CreateDirectory(imageDestinationFolder);
            
            double A4withAt100dpi = 2480.00/300;
            uint destinationWidth = (uint)(A4withAt100dpi * 600);

            StorageFile file = await StorageFile.GetFileFromPathAsync(programPdfSourceFile);
            PdfDocument doc = await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(file);

            PdfPageRenderOptions renderOptions = new PdfPageRenderOptions();
            renderOptions.DestinationWidth = destinationWidth;

            for (int i = 0; i < doc.PageCount; i++)
            {
                var page = doc.GetPage((uint)i);
                using (var stream = new InMemoryRandomAccessStream())
                {
                    await page.RenderToStreamAsync(stream, renderOptions);
                    using (var image = new Bitmap(stream.AsStream()))
                    {
                        image.Save(imageDestinationFolder + @"\" + i + ".bmp", ImageFormat.Bmp);
                    }
                }
            }
        }

        public static bool downloadProgram(string url, string destinationPdfFile)
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    client.DownloadFile(url, destinationPdfFile);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    return false;
                }
            }
        }

    }
}
