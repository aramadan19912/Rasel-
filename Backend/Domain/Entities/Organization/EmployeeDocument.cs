using System;

namespace Domain.Entities.Organization
{
    public class EmployeeDocument
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public string DocumentName { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty; // Resume, Contract, ID, Certificate, etc.
        public string? Description { get; set; }

        public string FilePath { get; set; } = string.Empty;
        public string? FileUrl { get; set; }
        public long FileSize { get; set; }
        public string MimeType { get; set; } = string.Empty;

        public DateTime UploadDate { get; set; } = DateTime.UtcNow;
        public string UploadedBy { get; set; } = string.Empty;

        public DateTime? ExpiryDate { get; set; }
        public bool RequiresRenewal { get; set; }

        public bool IsConfidential { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedDate { get; set; }
        public string? VerifiedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
