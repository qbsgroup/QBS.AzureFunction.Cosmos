using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Azure.Cosmos;
using System.Net.Http;
using System.Net;

namespace QBS.Cosmos
{
    public class GetCosmosContainers
    {
        private CosmosClient cosmosClient;

        [FunctionName("GetCosmosContainers")]
        public async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            var requestBody = await req.Content.ReadAsStringAsync();
            log.LogInformation("GetCosmosContainer with key={itemKey}.", requestBody);

            RequestLogger requestLogger = new RequestLogger
            {
                Request = Newtonsoft.Json.JsonConvert.DeserializeObject<Request>(requestBody),
                Logger = log
            };
            ListOfContainers myContainers = await GetContainersFromCosmos(requestLogger);
            HttpResponseMessage result = new HttpResponseMessage
            {
                Content = new StringContent(JsonSerializer.Serialize(myContainers))
            };

            return myContainers != null
                ? result
                : req.CreateResponse(HttpStatusCode.BadRequest, "Wrong request...");
        }
        public async Task<ListOfContainers> GetContainersFromCosmos(RequestLogger request)
        {

            var ListOfContainers = await GetContainers(request);

            return ListOfContainers;
        }


        public async Task<ListOfContainers> GetContainers(RequestLogger request)
        {
            ListOfContainers myContainers = new ListOfContainers();
            myContainers.Name = new List<string>();
            // Create a new instance of the Cosmos Client

            this.cosmosClient = new CosmosClient("https://" + request.Request.Login.DatabaseName + ".documents.azure.com:443/", request.Request.Login.accessKey);

            Database database = this.cosmosClient.GetDatabase(request.Request.Login.DatabaseName);
            FeedIterator<ContainerProperties> iterator = database.GetContainerQueryIterator<ContainerProperties>();
            FeedResponse<ContainerProperties> containers = await iterator.ReadNextAsync().ConfigureAwait(false);

            foreach (var container in containers)
            {
                myContainers.Name.Add(container.Id);
            }
            return myContainers;
        }
    }

}

