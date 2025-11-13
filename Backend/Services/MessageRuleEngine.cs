using Newtonsoft.Json;
using OutlookInboxManagement.Data;
using OutlookInboxManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace OutlookInboxManagement.Services;

public class MessageRuleEngine : IMessageRuleEngine
{
    private readonly ApplicationDbContext _context;

    public MessageRuleEngine(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> EvaluateConditionsAsync(MessageRule rule, Message message)
    {
        var conditions = JsonConvert.DeserializeObject<List<RuleCondition>>(rule.ConditionsJson);
        if (conditions == null || !conditions.Any())
            return false;

        bool result = true;
        var groupedConditions = conditions.GroupBy(c => c.LogicalOperator);

        foreach (var group in groupedConditions)
        {
            if (group.Key == LogicalOperator.And)
            {
                result = result && group.All(c => EvaluateCondition(c, message));
            }
            else // Or
            {
                result = result || group.Any(c => EvaluateCondition(c, message));
            }
        }

        return await Task.FromResult(result);
    }

    private bool EvaluateCondition(RuleCondition condition, Message message)
    {
        var field = condition.Field.ToLower();
        var value = condition.Value;
        var operatorType = condition.Operator.ToLower();

        string? fieldValue = field switch
        {
            "subject" => message.Subject,
            "from" => message.Sender?.Email,
            "body" => message.Body,
            "to" => string.Join(",", message.ToRecipients.Select(r => r.Email)),
            _ => null
        };

        if (fieldValue == null)
            return false;

        return operatorType switch
        {
            "contains" => fieldValue.Contains(value, StringComparison.OrdinalIgnoreCase),
            "equals" => fieldValue.Equals(value, StringComparison.OrdinalIgnoreCase),
            "startswith" => fieldValue.StartsWith(value, StringComparison.OrdinalIgnoreCase),
            "endswith" => fieldValue.EndsWith(value, StringComparison.OrdinalIgnoreCase),
            "notcontains" => !fieldValue.Contains(value, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    public async Task ExecuteRuleAsync(MessageRule rule, Message message)
    {
        if (await EvaluateConditionsAsync(rule, message))
        {
            await ExecuteActionsAsync(rule, message);
        }
    }

    public async Task ExecuteActionsAsync(MessageRule rule, Message message)
    {
        var actions = JsonConvert.DeserializeObject<List<RuleAction>>(rule.ActionsJson);
        if (actions == null)
            return;

        foreach (var action in actions)
        {
            await ExecuteAction(action, message);
        }

        await _context.SaveChangesAsync();
    }

    private async Task ExecuteAction(RuleAction action, Message message)
    {
        switch (action.ActionType.ToLower())
        {
            case "move":
                if (action.Parameters.TryGetValue("folderId", out var folderIdObj) &&
                    int.TryParse(folderIdObj.ToString(), out int folderId))
                {
                    message.FolderId = folderId;
                }
                break;

            case "categorize":
                if (action.Parameters.TryGetValue("categoryId", out var categoryIdObj) &&
                    int.TryParse(categoryIdObj.ToString(), out int categoryId))
                {
                    var category = await _context.MessageCategories.FindAsync(categoryId);
                    if (category != null && !message.Categories.Contains(category))
                    {
                        message.Categories.Add(category);
                    }
                }
                break;

            case "flag":
                message.IsFlagged = true;
                message.FlagStatus = FlagStatus.Flagged;
                break;

            case "markasread":
                message.IsRead = true;
                message.ReadAt = DateTime.UtcNow;
                break;

            case "delete":
                var deletedFolder = await _context.MessageFolders
                    .FirstOrDefaultAsync(f =>
                        f.UserId == message.SenderId &&
                        f.Type == FolderType.Deleted);
                if (deletedFolder != null)
                {
                    message.FolderId = deletedFolder.Id;
                }
                break;

            case "setimportance":
                if (action.Parameters.TryGetValue("importance", out var importanceObj) &&
                    Enum.TryParse<MessageImportance>(importanceObj.ToString(), out var importance))
                {
                    message.Importance = importance;
                }
                break;
        }
    }
}
