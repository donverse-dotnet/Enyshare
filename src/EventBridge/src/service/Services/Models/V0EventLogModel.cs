using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Pocco.Svc.EventBridge.Protobufs.Types;

namespace Pocco.Svc.EventBridge.Services.Models;

public class V0EventLogModel {
  [Key]
  public string EventId { get; set; } = string.Empty;
  [Required]
  public int EventType { get; set; }
  [Required]
  public DateTime FiredAt { get; set; }
  public string EventData { get; set; } = string.Empty;


  public V0AccountCreatedEvent ToAccountCreatedEvent() {
    var eventData = JsonSerializer.Deserialize<V0AccountCreatedEvent>(EventData);

    return eventData ?? throw new InvalidOperationException("Failed to deserialize V0AccountCreatedEvent from payload.");
  }
}
