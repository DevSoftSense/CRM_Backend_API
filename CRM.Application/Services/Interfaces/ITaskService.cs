using CRM.Domain.DTOs.Tasks.Requests;
using CRM.Domain.DTOs.Tasks.Responses;

namespace CRM.Application.Services.Interfaces;

public interface ITaskService
{
    Task<TaskListResponse> GetTasksAsync(int orgId, GetTasksRequest request, CancellationToken cancellationToken = default);
    Task<TaskDetailDto?> GetTaskByIdAsync(int orgId, int taskId, CancellationToken cancellationToken = default);
    Task<TaskDetailDto> CreateTaskAsync(int orgId, CreateTaskRequest request, CancellationToken cancellationToken = default);
    Task<TaskDetailDto> UpdateTaskAsync(int orgId, int taskId, UpdateTaskRequest request, CancellationToken cancellationToken = default);
    Task<TaskDetailDto> CompleteTaskAsync(int orgId, int taskId, CancellationToken cancellationToken = default);
    Task<DeactivateTaskResponse> DeactivateTaskAsync(int orgId, int taskId, CancellationToken cancellationToken = default);
    Task<TaskLookupsResponse> GetLookupsAsync(int orgId, CancellationToken cancellationToken = default);
}
