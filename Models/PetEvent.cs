namespace PetHealthAPI.Models
{
    // 1. Event Sourcing Model
    public class PetEvent
    {
        public Guid Id { get; set; }
        public int PetId { get; set; }
        public string EventType { get; set; } = string.Empty; 
        public string Data { get; set; } = string.Empty; 
        public DateTime Timestamp { get; set; }
        public string Version { get; set; } = "v1"; 
    }

    // 2. CQRS Read Model (DTO)
    public class PetReadModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
    }
}