using CRM.Domain.DTOs.Tasks.Requests;
using CRM.Domain.DTOs.Tasks.Responses;

namespace CRM.Infrastructure.Repositories.Interfaces;

public interface ITaskRepository
{
    Task<IReadOnlyList<TaskListItemDto>> GetTasksAsync(
        int orgId, int appId, GetTasksRequest request, CancellationToken cancellationToken = default);

    Task<TaskDetailDto?> GetTaskByIdAsync(
        int orgId, int appId, int taskId, CancellationToken cancellationToken = default);

    Task<TaskDetailDto> CreateTaskAsync(
        int orgId, int appId, CreateTaskRequest request, CancellationToken cancellationToken = default);

    Task<TaskDetailDto> UpdateTaskAsync(
        int orgId, int appId, int taskId, UpdateTaskRequest request, CancellationToken cancellationToken = default);

    Task<TaskDetailDto> CompleteTaskAsync(
        int orgId, int appId, int taskId, CancellationToken cancellationToken = default);

    Task<DeactivateTaskResponse> DeactivateTaskAsync(
        int orgId, int appId, int taskId, CancellationToken cancellationToken = default);

    Task<TaskLookupsResponse> GetLookupsAsync(
        int orgId, int appId, CancellationToken cancellationToken = default);
}
