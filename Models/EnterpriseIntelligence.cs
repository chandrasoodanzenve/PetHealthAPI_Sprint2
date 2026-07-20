namespace PetHealthAPI.Models {
    public class EnterpriseOperatingSummary {
        public int TotalUsersAnalyzed { get; set; }
        public Dictionary<string, int> CohortDistribution { get; set; } = new();
        public List<GrowthOpportunity> TopStrategicOpportunities { get; set; } = new();
        public List<DecisionRecommendation> CriticalPrescriptions { get; set; } = new();
        public double EstimatedLTVLift { get; set; }
        public string SystemMaturityLevel { get; set; } = "Level 3: Prescriptive";
        public DateTime ReportTimestamp { get; set; } = DateTime.UtcNow;
    }
}