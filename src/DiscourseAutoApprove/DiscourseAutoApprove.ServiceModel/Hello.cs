using System.Collections.Generic;
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

    [Route("/sync")]
    public class SyncServiceStackCustomers : IReturnVoid
    {
    }

    [Route("/sync/users/{UserId}")]
    public class SyncSingleUser : IReturnVoid
    {
        public string UserId { get; set; }
    }

    public class SyncSingleUserByEmail : IReturnVoid
    {
        public string Email { get; set; }
    }

    [Route("/sync/users")]
    public class SyncListOfUsers : IReturn<SyncListOfUsersResponse>
    {
        public List<string> UserIds { get; set; } 
    }

    public class SyncListOfUsersResponse
    {
        public List<UserSyncResult> Results { get; set; }
    }

    public class UserSyncResult
    {
        public string UserId { get; set; }
        public bool IsActive { get; set; }
        public string AccountExpiry { get; set; }
    }

    [Route("/daily")]
    public class SyncAccountsDaily : IReturnVoid
    {
        
    }

    [Route("/hourly")]
    public class SyncAccountsHourly : IReturnVoid
    {

    }
}