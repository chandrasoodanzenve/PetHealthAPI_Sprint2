namespace PetHealthAPI.Models
{
    public class CustomerIntelligenceProfile
    {
        public int UserId { get; set; }
        public string UserSegment { get; set; } = "General"; 
        public double HealthScore { get; set; }
        public double RetentionRiskScore { get; set; }
        public double FeatureAdoptionScore { get; set; }
        public string SignalConfidence { get; set; } = "High";
        public List<string> RecommendedActions { get; set; } = new();
        public List<string> ConflictingSignals { get; set; } = new(); 
        public DateTime LastCalculated { get; set; }
    }
}