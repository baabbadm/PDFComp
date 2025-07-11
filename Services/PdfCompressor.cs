using System.Diagnostics;

namespace FileCompressor.Services
{
    public class PdfCompressor
    {
        private readonly string _gsExecutablePath;

        public PdfCompressor(IWebHostEnvironment env)
        {
            _gsExecutablePath = Path.Combine(env.ContentRootPath, "gs", "bin", "gswin64c.exe");
        }

        public async Task<bool> CompressPdfAsync(string inputPath, string outputPath, string quality = "/ebook")
        {
            if (!File.Exists(_gsExecutablePath))
                throw new FileNotFoundException($"Ghostscript not found at {_gsExecutablePath}");

            var args = $"-sDEVICE=pdfwrite -dCompatibilityLevel=1.4 -dPDFSETTINGS={quality} -dNOPAUSE -dQUIET -dBATCH -sOutputFile=\"{outputPath}\" \"{inputPath}\"";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _gsExecutablePath,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();

            return File.Exists(outputPath);
        }
    }
}
