namespace PetHealthAPI.Models {
    public class GrowthOpportunity {
        public string OpportunityName { get; set; } = string.Empty; 
        public string TargetMetric { get; set; } = string.Empty; 
        public string KPIDriver { get; set; } = string.Empty; 

        public string TimeHorizon { get; set; } = "Short-term"; 
        
        public double ImpactScore { get; set; }
        public double EffortScore { get; set; } 
        public double ConfidenceScore { get; set; } 
        public double RiskScore { get; set; } 
        
        public double FinalPriorityScore { get; set; } 
        public string PriorityRank { get; set; } = "Medium"; 
    }

    public class GrowthPortfolioReport {
        public List<GrowthOpportunity> HighPriorityInitiatives { get; set; } = new();
        public double TotalEstimatedImpact { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}