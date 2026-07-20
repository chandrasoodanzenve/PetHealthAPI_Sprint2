namespace PetHealthAPI.Models {
    public class DecisionRecommendation {
        public string ActionId { get; set; } = Guid.NewGuid().ToString();
        public string Category { get; set; } = "Product"; 
        public string Insight { get; set; } = string.Empty; 
        public string RecommendedAction { get; set; } = string.Empty; 
        public double ExpectedValueScore { get; set; }
        public double ConfidenceScore { get; set; } 
        public string Priority { get; set; } = "Low"; 
        public bool IsEscalationRequired { get; set; } 
        public string FeedbackStatus { get; set; } = "Pending"; 
    }

    public class PrescriptiveReport {
        public int UserId { get; set; }
        public List<DecisionRecommendation> Recommendations { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}