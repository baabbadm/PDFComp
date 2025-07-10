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

        [HttpPost("pdf-direct")]
        public async Task<IActionResult> CompressDirect(IFormFile file, [FromForm] string quality = "/ebook")
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var inputPath = Path.GetTempFileName();
            var outputPath = Path.GetTempFileName();

            using (var stream = new FileStream(inputPath, FileMode.Create))
                await file.CopyToAsync(stream);

            var success = await _compressor.CompressPdfAsync(inputPath, outputPath, quality);

            if (!success || !System.IO.File.Exists(outputPath))
                return StatusCode(500, "Compression failed");

            var outputBytes = await System.IO.File.ReadAllBytesAsync(outputPath);
            System.IO.File.Delete(inputPath);
            System.IO.File.Delete(outputPath);

            return File(outputBytes, "application/pdf", $"{Path.GetFileNameWithoutExtension(file.FileName)}_compressed.pdf");
        }
    }
}
