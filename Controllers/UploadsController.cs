namespace server.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using server.Services;
using System;

public class SasRequestDto
{
    public string FileName { get; set; } = null!;
    public string Folder { get; set; } = "uploads";
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UploadsController : ControllerBase
{
    private readonly AzureBlobService _blobService;

    public UploadsController(AzureBlobService blobService)
    {
        _blobService = blobService;
    }

    [HttpPost("sas")]
    public IActionResult GenerateSas([FromBody] SasRequestDto request)
    {
        var businessId = HttpContext.Items["BusinessId"]?.ToString() ?? "0";
        var extension = System.IO.Path.GetExtension(request.FileName);
        var blobName = $"{businessId}/{request.Folder}/{Guid.NewGuid()}{extension}";

        try
        {
            var sasUrl = _blobService.GenerateSasToken(blobName);
            var publicUrl = _blobService.GetPublicUrl(blobName);
            return Ok(new { url = sasUrl, publicUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to generate SAS token.", error = ex.Message });
        }
    }
}

