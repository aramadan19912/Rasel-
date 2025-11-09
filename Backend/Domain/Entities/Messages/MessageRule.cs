namespace Backend.Domain.Entities.Messages;

public class MessageRule
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public int Priority { get; set; }

    // Conditions and Actions stored as JSON
    public string ConditionsJson { get; set; } = "[]";
    public string ActionsJson { get; set; } = "[]";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModified { get; set; }
}

public class RuleCondition
{
    public string Field { get; set; } = string.Empty; // Subject, From, To, Body, etc.
    public string Operator { get; set; } = string.Empty; // Contains, Equals, StartsWith, etc.
    public string Value { get; set; } = string.Empty;
    public LogicalOperator LogicalOperator { get; set; } = LogicalOperator.And;
}

public class RuleAction
{
    public string ActionType { get; set; } = string.Empty; // Move, Copy, Delete, Flag, Category, etc.
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public enum LogicalOperator
{
    And,
    Or
}
