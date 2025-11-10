using System;

namespace Domain.Entities.Organization
{
    public class PerformanceReview
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public string ReviewerId { get; set; } = string.Empty;
        public Employee Reviewer { get; set; } = null!;

        public DateTime ReviewPeriodStart { get; set; }
        public DateTime ReviewPeriodEnd { get; set; }
        public DateTime ReviewDate { get; set; }

        public string ReviewType { get; set; } = "Annual"; // Annual, Quarterly, Probation, etc.
        public string Status { get; set; } = "Pending"; // Pending, Completed, Approved

        // Ratings (1-5 scale)
        public int? OverallRating { get; set; }
        public int? QualityOfWorkRating { get; set; }
        public int? ProductivityRating { get; set; }
        public int? TeamworkRating { get; set; }
        public int? CommunicationRating { get; set; }
        public int? InitiativeRating { get; set; }
        public int? LeadershipRating { get; set; }

        // Comments
        public string? Strengths { get; set; }
        public string? AreasForImprovement { get; set; }
        public string? Goals { get; set; }
        public string? EmployeeComments { get; set; }
        public string? ManagerComments { get; set; }

        // Recommendations
        public bool RecommendPromotion { get; set; }
        public bool RecommendSalaryIncrease { get; set; }
        public decimal? RecommendedSalaryIncreasePercent { get; set; }
        public bool RequiresImprovementPlan { get; set; }

        // Signatures
        public DateTime? EmployeeSignatureDate { get; set; }
        public DateTime? ManagerSignatureDate { get; set; }
        public DateTime? HrApprovalDate { get; set; }
        public string? HrApprovedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
