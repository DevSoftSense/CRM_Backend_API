using CRM.Application.Helpers;
using CRM.Application.Services.Interfaces;
using CRM.Domain.DTOs.Tasks.Requests;
using CRM.Domain.DTOs.Tasks.Responses;
using CRM.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CRM.Application.Services.Classes;

public sealed class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IConfiguration _configuration;

    public TaskService(ITaskRepository taskRepository, IConfiguration configuration)
    {
        _taskRepository = taskRepository;
        _configuration = configuration;
    }

    public async Task<TaskListResponse> GetTasksAsync(
        int orgId, GetTasksRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        request ??= new GetTasksRequest();
        if (request.Limit <= 0) request.Limit = 50;
        if (request.Offset < 0) request.Offset = 0;

        var items = await _taskRepository.GetTasksAsync(orgId, GetAppId(), request, cancellationToken);
        return new TaskListResponse
        {
            Items = items,
            TotalCount = items.Count > 0 ? items[0].TotalCount : 0
        };
    }

    public async Task<TaskDetailDto?> GetTaskByIdAsync(
        int orgId, int taskId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (taskId <= 0) throw new ArgumentException("Task id is required.");
        return await _taskRepository.GetTaskByIdAsync(orgId, GetAppId(), taskId, cancellationToken);
    }

    public async Task<TaskDetailDto> CreateTaskAsync(
        int orgId, CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Title is required.");
        if (string.IsNullOrWhiteSpace(request.Status)) request.Status = "TO_DO";
        if (string.IsNullOrWhiteSpace(request.Priority)) request.Priority = "MEDIUM";
        if (string.IsNullOrWhiteSpace(request.TaskType)) request.TaskType = "FOLLOW_UP";

        return await _taskRepository.CreateTaskAsync(orgId, GetAppId(), request, cancellationToken);
    }

    public async Task<TaskDetailDto> UpdateTaskAsync(
        int orgId, int taskId, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        ArgumentNullException.ThrowIfNull(request);
        if (taskId <= 0) throw new ArgumentException("Task id is required.");
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Title is required.");

        return await _taskRepository.UpdateTaskAsync(orgId, GetAppId(), taskId, request, cancellationToken);
    }

    public async Task<TaskDetailDto> CompleteTaskAsync(
        int orgId, int taskId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (taskId <= 0) throw new ArgumentException("Task id is required.");
        return await _taskRepository.CompleteTaskAsync(orgId, GetAppId(), taskId, cancellationToken);
    }

    public async Task<DeactivateTaskResponse> DeactivateTaskAsync(
        int orgId, int taskId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (taskId <= 0) throw new ArgumentException("Task id is required.");
        return await _taskRepository.DeactivateTaskAsync(orgId, GetAppId(), taskId, cancellationToken);
    }

    public async Task<TaskLookupsResponse> GetLookupsAsync(
        int orgId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        return await _taskRepository.GetLookupsAsync(orgId, GetAppId(), cancellationToken);
    }

    private int GetAppId() => SoftOnCloudProductId.Require(_configuration);

    private static void ValidateOrg(int orgId)
    {
        if (orgId <= 0) throw new ArgumentException("Organisation ID is required.");
    }
}
