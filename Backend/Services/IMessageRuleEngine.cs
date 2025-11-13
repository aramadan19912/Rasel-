using OutlookInboxManagement.Models;

namespace OutlookInboxManagement.Services;

public interface IMessageRuleEngine
{
    Task<bool> EvaluateConditionsAsync(MessageRule rule, Message message);
    Task ExecuteRuleAsync(MessageRule rule, Message message);
    Task ExecuteActionsAsync(MessageRule rule, Message message);
}
