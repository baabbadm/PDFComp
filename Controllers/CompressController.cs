using FileCompressor.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FileCompressor.Controllers
{
    [Route("api/compress")]
    [ApiController]
    public class CompressController : ControllerBase
    {
        private readonly PdfCompressor _compressor;
        private readonly IWebHostEnvironment _env;

        public CompressController(PdfCompressor compressor, IWebHostEnvironment env)
        {
            _compressor = compressor;
            _env = env;
        }

        [HttpPost("pdf-direct")]
        public async Task<IActionResult> CompressDirect(IFormFile file, [FromForm] string quality)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File not uploaded.");

            var uploads = Path.Combine(_env.WebRootPath, "uploads");
            var results = Path.Combine(_env.WebRootPath, "results");
            Directory.CreateDirectory(uploads);
            Directory.CreateDirectory(results);

            var inputPath = Path.Combine(uploads, Guid.NewGuid() + Path.GetExtension(file.FileName));
            var outputPath = Path.Combine(results, Path.GetFileNameWithoutExtension(inputPath) + "_compressed.pdf");

            await using (var stream = new FileStream(inputPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var stopwatch = Stopwatch.StartNew();
            bool success = await _compressor.CompressPdfAsync(inputPath, outputPath, quality);
            stopwatch.Stop();

            if (!success) return StatusCode(500, "Compression failed.");

            long originalSize = new FileInfo(inputPath).Length;
            long compressedSize = new FileInfo(outputPath).Length;

            return Ok(new
            {
                originalSize = originalSize,
                compressedSize = compressedSize,
                ratio = (100 - ((compressedSize / (double)originalSize) * 100)).ToString("0.0"),
                timeTaken = stopwatch.Elapsed.TotalSeconds.ToString("0.00"),
                downloadUrl = $"/results/{Path.GetFileName(outputPath)}"
            });
        }
    }
}
