using System.Text.Json.Serialization;

namespace CRM.Domain.DTOs.Timeline.Responses;

public sealed class TimelineStatsDto
{
    [JsonPropertyName("calls_count")]
    public int CallsCount { get; set; }

    [JsonPropertyName("emails_count")]
    public int EmailsCount { get; set; }

    [JsonPropertyName("meetings_count")]
    public int MeetingsCount { get; set; }

    [JsonPropertyName("notes_count")]
    public int NotesCount { get; set; }

    [JsonPropertyName("tasks_count")]
    public int TasksCount { get; set; }

    [JsonPropertyName("documents_count")]
    public int DocumentsCount { get; set; }
}

