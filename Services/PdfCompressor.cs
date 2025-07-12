using System.Diagnostics;

namespace FileCompressor.Services
{
    public class PdfCompressor
    {
        private const string GhostscriptCommand = "gs"; // اسم الأمر في النظام

        public async Task<bool> CompressPdfAsync(string inputPath, string outputPath, string quality = "/ebook")
        {
            // تأكد أن Ghostscript موجود في النظام
            if (string.IsNullOrWhiteSpace(GhostscriptCommand))
                throw new FileNotFoundException("Ghostscript not found in system PATH.");

            var args = $"-sDEVICE=pdfwrite -dCompatibilityLevel=1.4 -dPDFSETTINGS={quality} -dNOPAUSE -dQUIET -dBATCH -sOutputFile=\"{outputPath}\" \"{inputPath}\"";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = GhostscriptCommand,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            string stdOut = await process.StandardOutput.ReadToEndAsync();
            string stdErr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            // يمكنك تسجيل الإخراج إذا احتجت
            Console.WriteLine(stdOut);
            Console.Error.WriteLine(stdErr);

            return File.Exists(outputPath);
        }
    }
}
