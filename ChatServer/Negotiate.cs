using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace ChatServer
{
    public static class Negotiate
    {
        [FunctionName(nameof(Negotiate))]
        public static SignalRConnectionInfo Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get",
                Route = "negotiate")]
            HttpRequest req,
            [SignalRConnectionInfo(HubName = "simplechat")]
            SignalRConnectionInfo connectionInfo,
            ILogger log)
        {
            return connectionInfo;
        }
    }
}
