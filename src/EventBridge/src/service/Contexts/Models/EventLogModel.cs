using System.ComponentModel.DataAnnotations;

namespace Pocco.Svc.EventBridge.Contexts.Models;

public class EventLogModel {
  [Key]
  public string EventId { get; set; } = string.Empty;
  [Required]
  public EventType EventType { get; set; }
  [Required]
  public string EventData { get; set; } = "{}";
  public DateTime FiredAt { get; set; }
}
