using Microsoft.EntityFrameworkCore;
using PetHealthAPI.Data;
using PetHealthAPI.Models;
using Microsoft.Extensions.Logging;

namespace PetHealthAPI.Services
{
    public interface ICustomerIntelligenceService
    {
        Task<CustomerIntelligenceProfile> GetUnifiedViewAsync(int userId);
        Task<UserCohortProfile> GetCohortEvolutionAsync(int userId);
        Task<GrowthPortfolioReport> GetGrowthPrioritizationAsync();
        Task<PrescriptiveReport> GetDecisionRecommendationsAsync(int userId);

        Task<EnterpriseOperatingSummary> GetEnterpriseOperatingModelAsync();

    }

    public class CustomerIntelligenceService : ICustomerIntelligenceService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CustomerIntelligenceService> _logger;

        public CustomerIntelligenceService(AppDbContext context, ILogger<CustomerIntelligenceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CustomerIntelligenceProfile> GetUnifiedViewAsync(int userId)
        {
            var userPets = await _context.Pets.ToListAsync(); 
            var specificPet = userPets.FirstOrDefault(p => p.Id == userId) ?? userPets.FirstOrDefault();
            
            if (specificPet == null) return new CustomerIntelligenceProfile { UserId = userId };
            
            double actualPetHealth = specificPet.HealthScore;
            double riskScore = (actualPetHealth < 50) ? 80.0 : 20.0;

            var profile = new CustomerIntelligenceProfile
            {
                UserId = userId,
                HealthScore = actualPetHealth, 
                FeatureAdoptionScore = (userPets.Count > 0) ? 100 : 0,
                RetentionRiskScore = riskScore,
                LastCalculated = DateTime.UtcNow
            };

            if (actualPetHealth < 60)
            {
                profile.ConflictingSignals.Add($"Alert: Pet '{specificPet.Name}' health is at {actualPetHealth}. Needs attention.");
                profile.UserSegment = "At-Risk User";
                profile.RecommendedActions.Add("Send Veterinary Consultation Offer");
            }
            else
            {
                profile.UserSegment = "Healthy User";
                profile.RecommendedActions.Add("No immediate action required.");
            }

            return profile; 
        }

        public async Task<UserCohortProfile> GetCohortEvolutionAsync(int userId) 
        {
            _logger.LogInformation("Analyzing Longitudinal Trajectory for User {Id}", userId);
            var eventCount = await _context.PetEvents.CountAsync();
            var petCount = await _context.Pets.CountAsync();

            string cohort = (eventCount > 10) ? "Champion" : (petCount > 0) ? "Explorer" : "Seed";

            var profile = new UserCohortProfile {
                UserId = userId,
                CurrentCohort = cohort,
                AnalysisDate = DateTime.UtcNow,
                StabilityIndex = (cohort == "Champion") ? 95.0 : 45.0
            };

            if (cohort == "Explorer" || cohort == "Champion") 
            {
                profile.AdvancementIndicators.Add("Active feature adoption detected.");
                profile.NextLikelyTransition = "Stable Champion";
                profile.TrajectoryScore = 15.5;
                profile.ProgressionStatus = "Growth (Positive)";
            } 
            else 
            {
                profile.DeclineIndicators.Add("Low engagement frequency (Retention risk)");
                profile.NextLikelyTransition = "Sleeper (Negative)";
                profile.TrajectoryScore = -10.2;
                profile.ProgressionStatus = "Decline (Negative)";
            }

            return profile;
        }
        public async Task<GrowthPortfolioReport> GetGrowthPrioritizationAsync() {

    var totalUsers = await _context.Pets.CountAsync(); 

    // Formula: (Impact * Confidence * (10 - Risk)) / Effort
    var initiatives = new List<GrowthOpportunity> {
        new GrowthOpportunity { 
            OpportunityName = "Automated Vaccination Reminders", 
            TargetMetric = "Retention", 
            KPIDriver = "Retention (Churn Reduction)",
            TimeHorizon = "Short-term", 
            ImpactScore = 9, EffortScore = 3, ConfidenceScore = 0.9, RiskScore = 2 
        },
        new GrowthOpportunity { 
            OpportunityName = "Premium Pet Insurance Leads", 
            TargetMetric = "Revenue", 
            KPIDriver = "Adoption (Premium Conversion)", 
            TimeHorizon = "Long-term", 
            ImpactScore = 8, EffortScore = 7, ConfidenceScore = 0.7, RiskScore = 5 
        },
        new GrowthOpportunity { 
            OpportunityName = "Social Sharing for Pet Milestones", 
            TargetMetric = "Engagement", 
            KPIDriver = "Engagement (Daily Active Users)", 
            TimeHorizon = "Short-term",
            ImpactScore = 6, EffortScore = 4, ConfidenceScore = 0.8, RiskScore = 1 
        }
    };

    foreach (var item in initiatives) {
        item.FinalPriorityScore = (item.ImpactScore * item.ConfidenceScore * (10 - item.RiskScore)) / item.EffortScore;
        item.PriorityRank = item.FinalPriorityScore > 15 ? "P0 (Immediate)" : item.FinalPriorityScore > 8 ? "P1 (High)" : "P2 (Medium)";
    }

    return new GrowthPortfolioReport {
        HighPriorityInitiatives = initiatives.OrderByDescending(i => i.FinalPriorityScore).ToList(),
        TotalEstimatedImpact = initiatives.Sum(i => i.ImpactScore)
    };
}
public async Task<PrescriptiveReport> GetDecisionRecommendationsAsync(int userId) {
    var report = new PrescriptiveReport { UserId = userId };
    var profile = await GetUnifiedViewAsync(userId);
    var cohort = await GetCohortEvolutionAsync(userId);

    if (profile.RetentionRiskScore > 50) {
        var rec = new DecisionRecommendation {
            Category = "Retention",
            Insight = "User shows churn signs with high risk score of " + profile.RetentionRiskScore,
            RecommendedAction = "Offer personalized pet wellness bundle + 1 month extension",
            ConfidenceScore = 0.92
        };
        rec.ExpectedValueScore = (100 - profile.HealthScore) * rec.ConfidenceScore;
        rec.Priority = rec.ExpectedValueScore > 50 ? "Critical" : "High";
        rec.IsEscalationRequired = profile.RetentionRiskScore > 75;
        report.Recommendations.Add(rec);
    }

    if (cohort.CurrentCohort == "Explorer" || cohort.CurrentCohort == "Seed") {
        var rec = new DecisionRecommendation {
            Category = "Growth",
            Insight = "New user/Explorer transition stage detected.",
            RecommendedAction = "Send 'Getting Started' video tutorial for pet vaccinations",
            ConfidenceScore = 0.85
        };
        rec.ExpectedValueScore = 20.0 * rec.ConfidenceScore;
        rec.Priority = "Medium";
        report.Recommendations.Add(rec);
    }

    if (profile.FeatureAdoptionScore > 80 && profile.HealthScore > 70) {
        var rec = new DecisionRecommendation {
            Category = "Growth",
            Insight = "Power user detected with high adoption.",
            RecommendedAction = "Propose Annual Premium Plan for multiple pets",
            ConfidenceScore = 0.88
        };
        rec.ExpectedValueScore = 40.0 * rec.ConfidenceScore;
        rec.Priority = "High";
        report.Recommendations.Add(rec);
    }

    report.Recommendations = report.Recommendations
        .OrderByDescending(r => r.ExpectedValueScore)
        .ToList();

    if (report.Recommendations.Count == 0) {
        report.Recommendations.Add(new DecisionRecommendation {
            Category = "Customer Success",
            Insight = "User behavior is stable.",
            RecommendedAction = "Request app review for loyalty tracking",
            ConfidenceScore = 1.0,
            ExpectedValueScore = 5.0,
            Priority = "Low"
        });
    }

    return report;
}
public async Task<EnterpriseOperatingSummary> GetEnterpriseOperatingModelAsync() {
    var allPets = await _context.Pets.ToListAsync();

    var cohortSummary = new Dictionary<string, int>();
    foreach(var pet in allPets) {
        var c = await GetCohortEvolutionAsync(pet.Id);
        if (cohortSummary.ContainsKey(c.CurrentCohort)) cohortSummary[c.CurrentCohort]++;
        else cohortSummary[c.CurrentCohort] = 1;
    }
    var growthPlan = await GetGrowthPrioritizationAsync();

    var allRecommendations = new List<DecisionRecommendation>();
    foreach(var pet in allPets.Take(5)) { 
        var recs = await GetDecisionRecommendationsAsync(pet.Id);
        allRecommendations.AddRange(recs.Recommendations);
    }

    return new EnterpriseOperatingSummary {
        TotalUsersAnalyzed = allPets.Count,
        CohortDistribution = cohortSummary,
        TopStrategicOpportunities = growthPlan.HighPriorityInitiatives.Take(3).ToList(),
        CriticalPrescriptions = allRecommendations.Where(r => r.Priority == "Critical").ToList(),
        EstimatedLTVLift = growthPlan.TotalEstimatedImpact * 1.5, 
        SystemMaturityLevel = "Level 3: Prescriptive Autonomy"
    };
}
    }
}