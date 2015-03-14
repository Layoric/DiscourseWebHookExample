using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscourseAPIClient.Types
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public int UploadedAvatarId { get; set; }
        public string AvatarTemplate { get; set; }
    }

    public class Poster
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public int UploadedAvatarId { get; set; }
        public string AvatarTemplate { get; set; }
        public int? PostCount { get; set; }
    }

    public class Topic
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string FancyTitle { get; set; }
        public string Slug { get; set; }
        public int PostsCount { get; set; }
        public int ReplyCount { get; set; }
        public int HighestPostNumber { get; set; }
        public string ImageUrl { get; set; }
        public string CreatedAt { get; set; }
        public string LastPostedAt { get; set; }
        public bool Bumped { get; set; }
        public string BumpedAt { get; set; }
        public bool Unseen { get; set; }
        public bool Pinned { get; set; }

        //public object Unpinned { get; set; }
        public bool Visible { get; set; }
        public bool Closed { get; set; }
        public bool Archived { get; set; }
        public bool? Bookmarked { get; set; }
        public bool? Liked { get; set; }
        public Poster LastPoster { get; set; }
        public string Excerpt { get; set; }
        public int? LastReadPostNumber { get; set; }
        public int? Unread { get; set; }
        public int? NewPosts { get; set; }
        public int? NotificationLevel { get; set; }
    }

    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public string TextColor { get; set; }
        public string Slug { get; set; }
        public int TopicCount { get; set; }
        public int PostCount { get; set; }
        public string Description { get; set; }
        public string DescriptionText { get; set; }
        public string TopicUrl { get; set; }
        public bool ReadRestricted { get; set; }

        //public object Permission { get; set; }
        //public object NotificationLevel { get; set; }
        //public object LogoUrl { get; set; }
        //public object BackgroundUrl { get; set; }
        public bool CanEdit { get; set; }
        public int TopicsDay { get; set; }
        public int TopicsWeek { get; set; }
        public int TopicsMonth { get; set; }
        public int TopicsYear { get; set; }
        public int PostsDay { get; set; }
        public int PostsWeek { get; set; }
        public int PostsMonth { get; set; }
        public int PostsYear { get; set; }
        public string DescriptionExcerpt { get; set; }
        public List<int> FeaturedUserIds { get; set; }
        public List<Topic> Topics { get; set; }
        public bool? IsUncategorized { get; set; }

        public List<GroupPermission> GroupPermissions { get; set; }
    }

    public class TopicDetails
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string FancyTitle { get; set; }
        public string Slug { get; set; }
        public int PostsCount { get; set; }
        public int ReplyCount { get; set; }
        public int HighestPostNumber { get; set; }

        public string ImageUrl { get; set; }
        public string CreatedAt { get; set; }
        public string LastPostedAt { get; set; }
        public bool Bumped { get; set; }
        public string BumpedAt { get; set; }
        public bool Unseen { get; set; }
        public bool Pinned { get; set; }


        //public object Unpinned { get; set; }
        public string Excerpt { get; set; }
        public bool Visible { get; set; }
        public bool Closed { get; set; }
        public bool Archived { get; set; }

        //public object Bookmarked { get; set; }
        //public object Liked { get; set; }
        public int Views { get; set; }
        public int LikeCount { get; set; }
        public bool HasSummary { get; set; }
        public string Archetype { get; set; }
        public string LastPosterUsername { get; set; }
        public int CategoryId { get; set; }
        public bool PinnedGlobally { get; set; }
        public List<Poster> Posters { get; set; }
    }

    public class ActionsSummary
    {
        public int Id { get; set; }
        public int Count { get; set; }
        public bool Hidden { get; set; }
        public bool CanAct { get; set; }
        public bool CanDeferFlags { get; set; }
    }

    public class Post
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string AvatarTemplate { get; set; }
        public int UploadedAvatarId { get; set; }
        public string CreatedAt { get; set; }
        public string Cooked { get; set; }
        public int PostNumber { get; set; }
        public int PostType { get; set; }
        public string UpdatedAt { get; set; }
        public int LikeCount { get; set; }
        public int ReplyCount { get; set; }
        public int? ReplyToPostNumber { get; set; }
        public int QuoteCount { get; set; }

        //public object AvgTime { get; set; }
        public int IncomingLinkCount { get; set; }
        public int Reads { get; set; }
        public double Score { get; set; }
        public bool Yours { get; set; }
        public int TopicId { get; set; }
        public string TopicSlug { get; set; }
        public object TopicAutoCloseAt { get; set; }
        public string DisplayUsername { get; set; }

        public string PrimaryGroupName { get; set; }
        public int Version { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanRecover { get; set; }
        public bool Read { get; set; }
        public string UserTitle { get; set; }
        public List<ActionsSummary> ActionsSummary { get; set; }
        public bool Moderator { get; set; }
        public bool Admin { get; set; }
        public bool Staff { get; set; }
        public int UserId { get; set; }
        public bool Hidden { get; set; }
        public int? HiddenReasonId { get; set; }
        public int TrustLevel { get; set; }

        //public object DeletedAt { get; set; }
        public bool UserDeleted { get; set; }
        public string EditReason { get; set; }
        public bool CanViewEditHistory { get; set; }
        public bool Wiki { get; set; }
    }

    public class PostStream
    {
        public List<Post> Posts { get; set; }
        public List<int> Stream { get; set; }
    }

    public class TopicMetaData
    {
        public object AutoCloseAt { get; set; }
        public object AutoCloseHours { get; set; }
        public bool AutoCloseBasedOnLastPost { get; set; }
        public User CreatedBy { get; set; }
        public Poster LastPoster { get; set; }
        public List<Poster> Participants { get; set; }
        public List<Topic> SuggestedTopics { get; set; }
        public int NotificationLevel { get; set; }
        public bool CanMovePosts { get; set; }
        public bool CanEdit { get; set; }
        public bool CanRecover { get; set; }
        public bool CanRemoveAllowedUsers { get; set; }
        public bool CanCreatePost { get; set; }
        public bool CanReplyAsNewTopic { get; set; }
        public bool CanFlagTopic { get; set; }
    }

    public class CategoryList
    {
        public bool CanCreateCategory { get; set; }
        public bool CanCreateTopic { get; set; }

        //public object Draft { get; set; }
        public string DraftKey { get; set; }
        public int DraftSequence { get; set; }
        public List<Category> Categories { get; set; }
    }

    public class TopicList
    {
        public bool CanCreateTopic { get; set; }

        //public object draft { get; set; }
        public string DraftKey { get; set; }
        public int DraftSequence { get; set; }
        public int PerPage { get; set; }
        public List<TopicDetails> Topics { get; set; }
    }

    public class GroupPermission
    {
        public int PermissionType { get; set; }
        public string GroupName { get; set; }
    }

    public class GetCategoriesResponse
    {
        public List<User> FeaturedUsers { get; set; }
        public CategoryList CategoryList { get; set; }
    }

    public class GetCategoryResponse
    {
        public List<User> Users { get; set; }
        public TopicList TopicList { get; set; }
    }

    public class GetLatestTopicsResponse
    {
        public List<User> Users { get; set; }
        public TopicList TopicList { get; set; }
    }

    public class GetTopicsResponse
    {
        public List<User> Users { get; set; }
        public TopicList TopicList { get; set; }
    }

    public class GetTopicResponse
    {
        public PostStream PostStream { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }
        public string FancyTitle { get; set; }
        public int PostsCount { get; set; }
        public string CreatedAt { get; set; }
        public int Views { get; set; }
        public int ReplyCount { get; set; }
        public int ParticipantCount { get; set; }
        public int LikeCount { get; set; }
        public string LastPostedAt { get; set; }
        public bool Visible { get; set; }
        public bool Closed { get; set; }
        public bool Archived { get; set; }
        public bool HasSummary { get; set; }
        public string Archetype { get; set; }
        public string Slug { get; set; }
        public int CategoryId { get; set; }
        public int WordCount { get; set; }

        //public object DeletedAt { get; set; }
        //public object Draft { get; set; }

        public string DraftKey { get; set; }
        public int DraftSequence { get; set; }

        //public bool? Unpinned { get; set; }
        public bool PinnedGlobally { get; set; }
        public bool Pinned { get; set; }

        //public object PinnedAt { get; set; }
        public TopicMetaData Details { get; set; }
        public int HighestPostNumber { get; set; }

        //public object DeletedBy { get; set; }
        public bool HasDeleted { get; set; }
        public List<ActionsSummary> ActionsSummary { get; set; }
        public int ChunkSize { get; set; }
        public bool? Bookmarked { get; set; }
    }

    public class CreatePostResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string AvatarTemplate { get; set; }
        public int UploadedAvatarId { get; set; }
        public string CreatedAt { get; set; }
        public string Cooked { get; set; }
        public int PostNumber { get; set; }
        public int PostType { get; set; }
        public string UpdatedAt { get; set; }
        public int LikeCount { get; set; }
        public int ReplyCount { get; set; }

        //public object ReplyToPostNumber { get; set; }
        public int QuoteCount { get; set; }

        //public object AvgTime { get; set; }
        public int IncomingLinkCount { get; set; }
        public int Reads { get; set; }
        public int Score { get; set; }
        public bool Yours { get; set; }
        public int TopicId { get; set; }
        public string TopicSlug { get; set; }

        //public object TopicAutoCloseAt { get; set; }
        public string DisplayUsername { get; set; }

        //public object PrimaryGroupName { get; set; }
        public int Version { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanRecover { get; set; }

        //public object UserTitle { get; set; }
        public List<ActionsSummary> ActionsSummary { get; set; }
        public bool Moderator { get; set; }
        public bool Admin { get; set; }
        public bool Staff { get; set; }
        public int UserId { get; set; }
        public int DraftSequence { get; set; }
        public bool Hidden { get; set; }

        //public object HiddenReasonId { get; set; }
        public int TrustLevel { get; set; }

        //public object DeletedAt { get; set; }
        public bool UserDeleted { get; set; }

        //public object EditReason { get; set; }
        public bool CanViewEditHistory { get; set; }
        public bool Wiki { get; set; }
    }

    public class CreateCategoryResponse
    {
        public Category Category { get; set; }
    }

    public class AdminApproveUserResponse
    {
        
    }

    public class ReplyToPostResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string AvatarTemplate { get; set; }
        public int UploadedAvatarId { get; set; }
        public string CreatedAt { get; set; }
        public string Cooked { get; set; }
        public int PostNumber { get; set; }
        public int PostType { get; set; }
        public string UpdatedAt { get; set; }
        public int LikeCount { get; set; }
        public int ReplyCount { get; set; }

        public int? ReplyToPostNumber { get; set; }
        public int QuoteCount { get; set; }

        //public object avg_time { get; set; }
        public int IncomingLinkCount { get; set; }
        public int Reads { get; set; }
        public int Score { get; set; }
        public bool Yours { get; set; }
        public int TopicId { get; set; }
        public string TopicSlug { get; set; }

        //public object topic_auto_close_at { get; set; }
        public string DisplayUsername { get; set; }
        public string PrimaryGroupName { get; set; }
        public int Version { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanRecover { get; set; }

        public string UserTitle { get; set; }
        public List<ActionsSummary> ActionsSummary { get; set; }
        public bool Moderator { get; set; }
        public bool Admin { get; set; }
        public bool Staff { get; set; }
        public int UserId { get; set; }
        public int DraftSequence { get; set; }
        public bool Hidden { get; set; }

        public int? HiddenReasonId { get; set; }
        public int TrustLevel { get; set; }

        //public object deleted_at { get; set; }
        public bool UserDeleted { get; set; }
        public string EditReason { get; set; }
        public bool CanViewEditHistory { get; set; }
        public bool Wiki { get; set; }
    }
}
