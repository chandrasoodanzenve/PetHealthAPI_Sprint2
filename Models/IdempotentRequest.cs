namespace PetHealthAPI.Models
{
    public class IdempotentRequest
    {
        public Guid Id { get; set; }
        public string Key { get; set; } = string.Empty; 
        public string Result { get; set; } = string.Empty; 
        public DateTime CreatedAt { get; set; }
    }
}