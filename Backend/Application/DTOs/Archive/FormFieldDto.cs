namespace Application.DTOs.Archive;

/// <summary>
/// Form field DTO
/// </summary>
public class FormFieldDto
{
    public int Id { get; set; }
    public int FormId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string LabelAr { get; set; } = string.Empty;
    public string? LabelEn { get; set; }
    public string FieldType { get; set; } = string.Empty;
    public string? Placeholder { get; set; }
    public string? HelpText { get; set; }
    public string? DefaultValue { get; set; }
    public string? Options { get; set; }
    public string? ValidationRules { get; set; }
    public bool IsRequired { get; set; }
    public bool IsReadOnly { get; set; }
    public bool IsVisible { get; set; }
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public string? Pattern { get; set; }
    public string? CssClass { get; set; }
    public int ColumnWidth { get; set; }
    public int SortOrder { get; set; }
    public string? ConditionalRules { get; set; }
}

/// <summary>
/// Create/Update form field request
/// </summary>
public class CreateFormFieldRequest
{
    public string FieldName { get; set; } = string.Empty;
    public string LabelAr { get; set; } = string.Empty;
    public string? LabelEn { get; set; }
    public string FieldType { get; set; } = "Text";
    public string? Placeholder { get; set; }
    public string? HelpText { get; set; }
    public string? DefaultValue { get; set; }
    public string? Options { get; set; }
    public string? ValidationRules { get; set; }
    public bool IsRequired { get; set; } = false;
    public bool IsReadOnly { get; set; } = false;
    public bool IsVisible { get; set; } = true;
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public string? Pattern { get; set; }
    public string? CssClass { get; set; }
    public int ColumnWidth { get; set; } = 12;
    public int SortOrder { get; set; }
    public string? ConditionalRules { get; set; }
}
