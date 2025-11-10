using System;

namespace Domain.Entities.Organization
{
    public class EmployeeSkill
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public string SkillName { get; set; } = string.Empty;
        public string? Category { get; set; } // Technical, Soft Skills, Language, etc.
        public int ProficiencyLevel { get; set; } // 1-5 or 1-10
        public int? YearsOfExperience { get; set; }

        public DateTime AcquiredDate { get; set; }
        public DateTime? LastUsedDate { get; set; }
        public DateTime? CertificationDate { get; set; }
        public DateTime? CertificationExpiryDate { get; set; }
        public string? CertificationNumber { get; set; }

        public bool IsPrimary { get; set; }
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
