using TaskTracker.Api.Dtos.Attachments;
using TaskTracker.Api.Services.Interfaces;

namespace TaskTracker.Api.Endpoints;

public static class AttachmentEndpoints
{
    public static IEndpointRouteBuilder MapAttachmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tasks/{taskId:int}/attachments")
            .WithTags("Attachments")
            .RequireAuthorization();

        group.MapGet("/", async (
            int taskId,
            IAttachmentService service,
            CancellationToken cancellationToken) =>
        {
            var attachments = await service.GetTaskAttachmentsAsync(taskId, cancellationToken);
            return Results.Ok(attachments);
        });
        
        group.MapPost("/", async (
            int taskId,
            CreateAttachmentRequest request,
            IAttachmentService service,
            CancellationToken cancellationToken) =>
        {
            var attachment = await service.AddAttachmentAsync(taskId, request, cancellationToken);
            return Results.Created($"/api/tasks/{taskId}/attachments/{attachment.Id}", attachment);
        });
        
        group.MapDelete("/{attachmentId:int}", async (
            int taskId,
            int attachmentId,
            IAttachmentService service,
            CancellationToken cancellationToken) =>
        {
            var deleted = await service.DeleteAttachmentAsync(taskId, attachmentId, cancellationToken);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        return app;
    }
}