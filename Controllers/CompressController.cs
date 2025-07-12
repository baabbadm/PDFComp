using Microsoft.AspNetCore.Mvc;
using FileCompressor.Services;
using System.IO;

namespace FileCompressor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompressController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly PdfCompressor _compressor;

        public CompressController(IWebHostEnvironment env, PdfCompressor compressor)
        {
            _env = env;
            _compressor = compressor;
        }

        [HttpPost("pdf")]
        public async Task<IActionResult> CompressPdf(IFormFile file, [FromForm] string quality = "/ebook")
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
            var compressedDir = Path.Combine(_env.WebRootPath, "compressed");

            Directory.CreateDirectory(uploadsDir);
            Directory.CreateDirectory(compressedDir);

            var fileName = Path.GetFileNameWithoutExtension(file.FileName);
            var ext = Path.GetExtension(file.FileName);
            var inputPath = Path.Combine(uploadsDir, $"{fileName}{ext}");
            var outputPath = Path.Combine(compressedDir, $"{fileName}_compressed{ext}");

            using (var stream = new FileStream(inputPath, FileMode.Create))
                await file.CopyToAsync(stream);

            var success = await _compressor.CompressPdfAsync(inputPath, outputPath, quality);

            if (!success)
                return StatusCode(500, "Compression failed");

            long originalSize = new FileInfo(inputPath).Length;
            long compressedSize = new FileInfo(outputPath).Length;
            var compressionRatio = (1 - ((double)compressedSize / originalSize)) * 100;

            var fileUrl = $"{Request.Scheme}://{Request.Host}/compressed/{Path.GetFileName(outputPath)}";

            return Ok(new
            {
                fileUrl,
                originalSizeKb = originalSize / 1024,
                compressedSizeKb = compressedSize / 1024,
                compressionRatio = Math.Round(compressionRatio, 2)
            });
        }
    }
}
