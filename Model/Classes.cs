using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace QBS.Cosmos
{
    public class Login
    {
        public string DatabaseName { get; set; }
        public string SyncMode { get; set; }
        public string accessKey { get; set; }
        public string businessCentralUser { get; set; }
        public string businessCentralKey { get; set; }
    }

    public class Request
    {
        public int tableId { get; set; }
        public string versionNo { get; set; } = "1.0";
        public string sourceTableView { get; set; } = "";
        public string description { get; set; } = "";
        public string containerName { get; set; } = "";
        public string endPointUri { get; set; }
        public string tenandId { get; set; }
        public string companyId { get; set; }
        public string apiPublisher { get; set; }
        public string apiGroup { get; set; }
        public string apiVersion { get; set; }
        public string apiEndpoint { get; set; }
        public string sandBoxName { get; set; }
        public Login Login { get; set; }
        public CosmosDataSet DataSet { get; set; }
    }

    public class RequestLogger
    {
        public Request Request { get; set; }
        public ILogger Logger { get; set; }

    }
    public class CosmosDataSet
    {
        public string Description { get; set; }
        public string ContainerName { get; set; }
        public Newtonsoft.Json.Linq.JObject Data { get; set; }
    }

    public class ListOfContainers
    {
        public List<string> Name { get; set; }
    }
    public class Result
    {
        public List<string> ResultText { get; set; }
    }
}

