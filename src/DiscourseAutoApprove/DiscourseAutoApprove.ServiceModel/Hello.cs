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
    public class SyncServiceStackCustomers
    {
    }
}