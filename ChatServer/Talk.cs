using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Newtonsoft.Json.Linq;
using ChatClientBlazor.Model;

namespace ChatServer
{
    public static class Talk
    {
        [FunctionName("Talk")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "post",
                Route = "talk")]
            HttpRequest req,
            [SignalR(HubName = "simplechat")]
            IAsyncCollector<SignalRMessage> questionR,
            ILogger log)
        {
            try
            {
                string json = await new StreamReader(req.Body).ReadToEndAsync();
                var message = JsonConvert.DeserializeObject<Message>(json);

                await questionR.AddAsync(
                    new SignalRMessage
                    {
                        Target = "newMessage",
                        Arguments = new[] { message }
                    });

                log.LogInformation($"Sent message '{message.Text}'");

                return new OkObjectResult($"Hello {message.Name}, your message was '{message.Text}'");
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult("There was an error: " + ex.Message);
            }
        }
    }
}
