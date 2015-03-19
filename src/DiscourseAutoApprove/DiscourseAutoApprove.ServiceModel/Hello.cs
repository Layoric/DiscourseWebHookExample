using System.IO;
using ServiceStack;
using ServiceStack.Web;

namespace DiscourseAutoApprove.ServiceModel
{
    [Route("/hello/{Name}")]
    public class Hello : IReturn<HelloResponse>
    {
        public string Name { get; set; }
    }

    public class HelloResponse
    {
        public string Result { get; set; }
    }

    [Route("/discourse/user_created")]
    public class UserCreatedDiscourseWebHook : IRequiresRequestStream
    {
        public Stream RequestStream { get; set; }
    }

    public class DiscourseUser
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string AvatarTemplate { get; set; }
        public string Name { get; set; }
        public string CreatedAt { get; set; }
        public string Email { get; set; }
        public bool CanEdit { get; set; }
        public bool CanEditUsername { get; set; }
        public bool CanEditEmail { get; set; }
        public bool CanEditName { get; set; }
        public bool CanSendPrivateMessages { get; set; }
        public bool CanSendPrivateMessageToUser { get; set; }
        public string BioExcerpt { get; set; }
        public int TrustLevel { get; set; }
        public bool Moderator { get; set; }
        public bool Admin { get; set; }
        public int BadgeCount { get; set; }
        public int NotificationCount { get; set; }
        public bool HasTitleBadges { get; set; }
        public int PostCount { get; set; }
        public bool CanBeDeleted { get; set; }
        public bool CanDeleteAllPosts { get; set; }
        public bool EmailDigests { get; set; }
        public bool EmailPrivateMessages { get; set; }
        public bool EmailDirect { get; set; }
        public bool EmailAlways { get; set; }
        public int DigestAfterDays { get; set; }
        public bool MailingListMode { get; set; }
        public int AutoTrackTopicsAfterMsecs { get; set; }
        public int NewTopicDurationMinutes { get; set; }
        public bool ExternalLinksInNewTab { get; set; }
        public bool DynamicFavicon { get; set; }
        public bool EnableQuoting { get; set; }
        public bool DisableJumpReply { get; set; }
    }
}