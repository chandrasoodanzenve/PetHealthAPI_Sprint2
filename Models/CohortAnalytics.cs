namespace PetHealthAPI.Models {
    public class UserCohortProfile {
        public int UserId { get; set; }
        public string CurrentCohort { get; set; } = "Seed"; 
        public double TrajectoryScore { get; set; }
        public string ProgressionStatus { get; set; } = "Stagnant"; 
        public List<string> AdvancementIndicators { get; set; } = new(); 
        public List<string> DeclineIndicators { get; set; } = new(); 
        public string NextLikelyTransition { get; set; } = "Unknown"; 
        public double StabilityIndex { get; set; } 
        public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;
    }
}