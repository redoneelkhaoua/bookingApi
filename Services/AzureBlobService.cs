namespace server.Services;

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

public class AzureBlobService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;
    private readonly ILogger<AzureBlobService> _logger;

    public AzureBlobService(IConfiguration config, ILogger<AzureBlobService> logger, server.Data.AppDbContext dbContext)
    {
        _logger = logger;
        
        var setting = dbContext.SystemSettings.FirstOrDefault(s => s.Key == "AzureBlobConnectionString");
        var connectionString = setting?.Value ?? config["Azure:BlobConnectionString"];
        
        _containerName = config["Azure:BlobContainerName"] ?? "appointment-app";
        _blobServiceClient = new BlobServiceClient(connectionString);
        InitializeAsync().GetAwaiter().GetResult();
    }

    private async Task InitializeAsync()
    {
        try
        {
            // Create container with public blob access if not existing
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
            _logger.LogInformation("Azure Blob container '{Container}' is ready.", _containerName);

            // Configure CORS so browsers can PUT directly to Blob Storage
            var props = (await _blobServiceClient.GetPropertiesAsync()).Value;
            props.Cors.Clear();
            props.Cors.Add(new BlobCorsRule
            {
                AllowedOrigins = "*",
                AllowedMethods = "GET,PUT,POST,DELETE,HEAD,OPTIONS",
                AllowedHeaders = "*",
                ExposedHeaders = "*",
                MaxAgeInSeconds = 3600
            });
            await _blobServiceClient.SetPropertiesAsync(props);
            _logger.LogInformation("Azure Blob CORS configured.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure Blob initialization failed. Uploads will not work.");
        }
    }

    /// <summary>Returns a SAS URL that allows the browser to PUT the file directly.</summary>
    public string GenerateSasToken(string blobName, int expiryMinutes = 15)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerName,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes)
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Write | BlobSasPermissions.Create | BlobSasPermissions.Read);

        return blobClient.GenerateSasUri(sasBuilder).ToString();
    }

    /// <summary>Returns the permanent public URL of a blob (no SAS).</summary>
    public string GetPublicUrl(string blobName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        return containerClient.GetBlobClient(blobName).Uri.ToString();
    }
}
