namespace Application.Interfaces.Archive;

/// <summary>
/// PDF conversion service interface
/// </summary>
public interface IPdfConversionService
{
    /// <summary>
    /// Convert HTML content to PDF
    /// </summary>
    Task<byte[]> ConvertHtmlToPdfAsync(string htmlContent);

    /// <summary>
    /// Convert correspondence to PDF with metadata
    /// </summary>
    Task<byte[]> ConvertCorrespondenceToPdfAsync(
        int correspondenceId,
        bool includeAttachments = true,
        bool convertToPdfA = false,
        string? watermarkText = null);

    /// <summary>
    /// Merge multiple PDFs into one
    /// </summary>
    Task<byte[]> MergePdfsAsync(List<byte[]> pdfDocuments);

    /// <summary>
    /// Add watermark to PDF
    /// </summary>
    Task<byte[]> AddWatermarkAsync(byte[] pdfData, string watermarkText, float opacity = 0.3f);

    /// <summary>
    /// Apply digital signature to PDF
    /// </summary>
    Task<byte[]> ApplyDigitalSignatureAsync(byte[] pdfData, string certificatePath, string password);

    /// <summary>
    /// Encrypt PDF with password
    /// </summary>
    Task<byte[]> EncryptPdfAsync(byte[] pdfData, string password);

    /// <summary>
    /// Convert to PDF/A format for archiving
    /// </summary>
    Task<byte[]> ConvertToPdfAAsync(byte[] pdfData);

    /// <summary>
    /// Extract text content from PDF
    /// </summary>
    Task<string> ExtractTextFromPdfAsync(byte[] pdfData);

    /// <summary>
    /// Get PDF page count
    /// </summary>
    Task<int> GetPageCountAsync(byte[] pdfData);

    /// <summary>
    /// Calculate PDF checksum (SHA256)
    /// </summary>
    Task<string> CalculateChecksumAsync(byte[] pdfData);

    /// <summary>
    /// Verify PDF integrity using checksum
    /// </summary>
    Task<bool> VerifyPdfIntegrityAsync(byte[] pdfData, string expectedChecksum);

    /// <summary>
    /// Optimize PDF file size
    /// </summary>
    Task<byte[]> OptimizePdfAsync(byte[] pdfData);

    /// <summary>
    /// Generate PDF metadata JSON
    /// </summary>
    Task<string> GeneratePdfMetadataAsync(int correspondenceId);
}
