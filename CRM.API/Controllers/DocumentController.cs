using CRM.Application.Services.Interfaces;
using CRM.Domain.DTOs.Documents.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace CRM.API.Controllers;

[Authorize]
[ApiController]
[Route("api/documents")]
public sealed class DocumentController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly ILogger<DocumentController> _logger;

    public DocumentController(IDocumentService documentService, ILogger<DocumentController> logger)
    {
        _documentService = documentService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetDocumentsAsync(
        [FromQuery] string? search,
        [FromQuery] string? folder,
        [FromQuery] string? fileType,
        [FromQuery] int? ownerId,
        [FromQuery] string? entityType,
        [FromQuery] long? entityId,
        [FromQuery] string? tab,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var res = await _documentService.GetDocumentsAsync(
                orgId,
                new GetDocumentsRequest
                {
                    Search = search,
                    Folder = folder,
                    FileType = fileType,
                    OwnerId = ownerId,
                    EntityType = entityType,
                    EntityId = entityId,
                    Tab = tab,
                    Limit = limit,
                    Offset = offset
                },
                cancellationToken);
            return Ok(res);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Document list failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET api/documents failed");
            return StatusCode(500, new { message = "Failed to load documents." });
        }
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            return Ok(await _documentService.GetStatsAsync(orgId, cancellationToken));
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Document stats failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET api/documents/stats failed");
            return StatusCode(500, new { message = "Failed to load document statistics." });
        }
    }

    [HttpGet("lookups")]
    public async Task<IActionResult> GetLookupsAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            return Ok(await _documentService.GetLookupsAsync(orgId, cancellationToken));
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Document lookups failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET api/documents/lookups failed");
            return StatusCode(500, new { message = "Failed to load document lookups." });
        }
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var doc = await _documentService.GetByIdAsync(orgId, id, cancellationToken);
            if (doc == null) return NotFound(new { message = $"Document {id} not found." });
            return Ok(doc);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Get document by id failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET api/documents/{Id} failed", id);
            return StatusCode(500, new { message = "Failed to load document details." });
        }
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadAsync(
        [FromForm] UploadDocumentRequest request, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            if (!request.OwnerId.HasValue || request.OwnerId.Value <= 0)
            {
                request.OwnerId = userId;
            }

            var result = await _documentService.UploadDocumentAsync(orgId, request, cancellationToken);
            return Created($"/api/documents/{result.DocumentId}", result);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Document upload failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "POST api/documents/upload failed");
            return StatusCode(500, new { message = "Failed to upload document." });
        }
    }

    [HttpPost("folder")]
    public async Task<IActionResult> CreateFolderAsync(
        [FromBody] CreateFolderRequest request, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var ok = await _documentService.CreateFolderAsync(orgId, request.FolderName, cancellationToken);
            return Ok(new { ok, folderName = request.FolderName });
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Create folder failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "POST api/documents/folder failed");
            return StatusCode(500, new { message = "Failed to create folder." });
        }
    }

    [HttpGet("{id:long}/download")]
    public async Task<IActionResult> DownloadAsync(long id, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var fileData = await _documentService.GetFileDownloadAsync(orgId, id, cancellationToken);
            if (fileData == null) return NotFound(new { message = "File not found." });

            var stream = new FileStream(fileData.Value.PhysicalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return File(stream, fileData.Value.ContentType, fileData.Value.FileName);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET api/documents/{Id}/download failed", id);
            return StatusCode(500, new { message = "Failed to download document." });
        }
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeactivateAsync(long id, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            return Ok(await _documentService.DeactivateDocumentAsync(orgId, id, cancellationToken));
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Document deactivate failed.");
            return BadRequest(new { message = ex.MessageText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DELETE api/documents/{Id} failed", id);
            return StatusCode(500, new { message = "Failed to delete document." });
        }
    }

    private bool TryGetAuthContext(out int userId, out int orgId)
    {
        userId = 0;
        orgId = 0;
        var userIdRaw = User.FindFirst("user_id")?.Value;
        var orgIdRaw = User.FindFirst("org_id")?.Value;
        return int.TryParse(userIdRaw, out userId) && userId > 0
               && int.TryParse(orgIdRaw, out orgId) && orgId > 0;
    }
}

