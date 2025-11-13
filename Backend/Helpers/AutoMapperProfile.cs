using AutoMapper;
using OutlookInboxManagement.Models;
using OutlookInboxManagement.DTOs;

namespace OutlookInboxManagement.Helpers;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // Message mappings
        CreateMap<Message, MessageDto>()
            .ForMember(dest => dest.FolderName, opt => opt.MapFrom(src => src.Folder != null ? src.Folder.DisplayName : null))
            .ForMember(dest => dest.ReplyCount, opt => opt.MapFrom(src => src.Replies.Count));

        CreateMap<CreateMessageDto, Message>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.MessageId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.BodyPreview, opt => opt.MapFrom(src => GetBodyPreview(src.Body)));

        // Folder mappings
        CreateMap<MessageFolder, MessageFolderDto>();
        CreateMap<CreateFolderDto, MessageFolder>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => FolderType.Custom));

        // Category mappings
        CreateMap<MessageCategory, MessageCategoryDto>();
        CreateMap<CreateCategoryDto, MessageCategory>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // Recipient mappings
        CreateMap<MessageRecipient, RecipientDto>();
        CreateMap<RecipientDto, MessageRecipient>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // Attachment mappings
        CreateMap<MessageAttachment, AttachmentDto>();

        // User mappings
        CreateMap<ApplicationUser, UserDto>();

        // Mention mappings
        CreateMap<MessageMention, MessageMentionDto>()
            .ForMember(dest => dest.MentionedUserName, opt => opt.MapFrom(src => src.MentionedUser != null ? src.MentionedUser.FullName : ""));

        // Reaction mappings
        CreateMap<MessageReaction, MessageReactionDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : ""));

        // Conversation mappings
        CreateMap<ConversationThread, ConversationThreadDto>();

        // Rule mappings
        CreateMap<MessageRule, MessageRuleDto>()
            .ForMember(dest => dest.Conditions, opt => opt.MapFrom(src => DeserializeConditions(src.ConditionsJson)))
            .ForMember(dest => dest.Actions, opt => opt.MapFrom(src => DeserializeActions(src.ActionsJson)));

        CreateMap<CreateRuleDto, MessageRule>()
            .ForMember(dest => dest.ConditionsJson, opt => opt.MapFrom(src => SerializeConditions(src.Conditions)))
            .ForMember(dest => dest.ActionsJson, opt => opt.MapFrom(src => SerializeActions(src.Actions)));

        // Tracking mappings
        CreateMap<MessageTracking, MessageTrackingDto>();
    }

    private static string GetBodyPreview(string body)
    {
        if (string.IsNullOrEmpty(body))
            return string.Empty;

        // Strip HTML tags
        var preview = System.Text.RegularExpressions.Regex.Replace(body, "<.*?>", string.Empty);

        // Limit to 200 characters
        return preview.Length > 200 ? preview.Substring(0, 200) + "..." : preview;
    }

    private static List<RuleCondition> DeserializeConditions(string json)
    {
        try
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<RuleCondition>>(json) ?? new List<RuleCondition>();
        }
        catch
        {
            return new List<RuleCondition>();
        }
    }

    private static List<RuleAction> DeserializeActions(string json)
    {
        try
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<RuleAction>>(json) ?? new List<RuleAction>();
        }
        catch
        {
            return new List<RuleAction>();
        }
    }

    private static string SerializeConditions(List<RuleCondition> conditions)
    {
        return Newtonsoft.Json.JsonConvert.SerializeObject(conditions);
    }

    private static string SerializeActions(List<RuleAction> actions)
    {
        return Newtonsoft.Json.JsonConvert.SerializeObject(actions);
    }
}
