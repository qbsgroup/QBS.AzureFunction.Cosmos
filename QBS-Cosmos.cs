using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos;

namespace QBS.Cosmos
{
    public class QBSCosmosReadBC
    {
        private Container container;
        private CosmosClient cosmosClient;
        private Database database;

        [FunctionName("ReadBusinessCentralData")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var request = Newtonsoft.Json.JsonConvert.DeserializeObject<Request>(requestBody);

            if (request.tableId.Equals(""))
                throw new Exception("Invalid API Tablename.");

            RequestLogger requestLogger = new RequestLogger
            {
                Request = request,
                Logger = log
            };
            Result myResult = await GetDataFromBusinessCentral(requestLogger);

            return new OkObjectResult(myResult);
        }

        public async Task<Result> GetDataFromBusinessCentral(RequestLogger request)
        {
            Result res = new Result();
            res.ResultText = new List<string>();
            if (request.Request.DataSet == null)
            {
                request.Request.DataSet = await GetDataUsingAPI(request);
            }

            if (request.Request.DataSet == null)
            {
                res.ResultText.Add("No dataset provided");
                return res;
            }
            await SetupCosmos(request);

            CosmosUploader cosmosUploader = new CosmosUploader(container);

            try
            {
                res = await cosmosUploader.CreateDocumentsAsync(request);
                request.Logger.LogInformation("Items added to Cosmos.\n");
            }
            catch (Exception e)
            {
                request.Logger.LogInformation(e.ToString());
                res.ResultText.Add(e.ToString());
            }


            return res;

        }
        private async Task<CosmosDataSet> GetDataUsingAPI(RequestLogger request)
        {
            CosmosDataSet dataSet = new CosmosDataSet();
            APIDataFetcher aPIDataFetcher = new APIDataFetcher(request);

            List<Parameter> parameters = new List<Parameter>();

            if (request.Request.tableId != 0)
                parameters.Add(new Parameter("filter", "tableId eq " + request.Request.tableId));

            if (request.Request.sourceTableView != "")
                parameters.Add(new Parameter("filter", "sourceTableView eq '" + request.Request.sourceTableView + "'"));

            if (request.Request.versionNo != "")
                parameters.Add(new Parameter("filter", "versionNo eq '" + request.Request.versionNo + "'"));

            if (request.Request.description != "")
                parameters.Add(new Parameter("filter", "description eq '" + request.Request.description + "'"));

            if (request.Request.containerName != "")
                parameters.Add(new Parameter("filter", "containerName eq '" + request.Request.containerName + "'"));

            string query = QueryBuilder(parameters);

            request.Logger.LogInformation("Query " + query);

            try
            {
                dataSet = await aPIDataFetcher.GetItemsAsync(query);
            }
            catch (Exception e)
            {
                request.Logger.LogInformation(e.ToString());
            }

            return dataSet;

        }

        //This method builds a query out of the parameters that we define. This makes it easier to add/remove parameters to the query.
        private static string QueryBuilder(List<Parameter> parameters)
        {
            string query = "";
            bool first = true;
            foreach (Parameter parameter in parameters)
            {
                if (first)
                {
                    query += "?";
                    first = false;
                    query += "$" + parameter.getName() + "=" + parameter.getValue();
                }
                else
                {
                    query += " and ";
                    query += parameter.getValue();
                }

            }
            return query;
        }

        public async Task SetupCosmos(RequestLogger request)
        {
            // Create a new instance of the Cosmos Client
            this.cosmosClient = new CosmosClient(request.Request.endPointUri, request.Request.Login.accessKey);

            await this.CreateDatabaseAsync(request);

            await this.CreateContainerAsync(request);
        }

        private async Task CreateDatabaseAsync(RequestLogger request)
        {
            // Create a new database if none exists, otherwise gets the reference.
            try
            {
                this.database = await this.cosmosClient.CreateDatabaseIfNotExistsAsync(request.Request.Login.DatabaseName);
            }
            catch (Exception e)
            {
                request.Logger.LogInformation("The exception is: " + e);
            }
            request.Logger.LogInformation("Connected to Database: {0}\n", this.database.Id);
        }

        private async Task CreateContainerAsync(RequestLogger request)
        {
            // Create a new container if none exists, otherwise gets the reference
            this.container = await this.database.CreateContainerIfNotExistsAsync(request.Request.DataSet.ContainerName, "/versionNo");
            request.Logger.LogInformation("Connected to Container: {0}\n", this.container.Id);
        }
    }
    public class Parameter
    {
        private string name;
        private string value;

        public Parameter(string name, string value)
        {
            this.name = name;
            this.value = value;
        }

        public string getName()
        {
            return this.name;
        }

        public string getValue()
        {
            return this.value;
        }

    }

}

