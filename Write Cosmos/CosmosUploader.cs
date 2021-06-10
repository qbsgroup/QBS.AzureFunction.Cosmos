using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QBS.Cosmos
{
    class CosmosUploader
    {
        private Container Container;

        public CosmosUploader(Container container)
        {
            this.Container = container;
        }

        public async Task<Result> CreateDocumentsAsync(RequestLogger request)
        {
            Result res = new Result();
            res.ResultText = new List<string>();
            //Storing json to CosmosDB
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(request.Request.Login.DatabaseName, request.Request.DataSet.ContainerName);
            try
            {
                using (DocumentClient DocumentDBclient2 = new DocumentClient(new Uri(request.Request.endPointUri), request.Request.Login.accessKey))
                {
                    Document doc = await DocumentDBclient2.CreateDocumentAsync(collectionUri, request.Request.DataSet.Data);
                }
                res.ResultText.Add("Succesfully added data to the cosmos container.");

            }
            catch (Exception e)
            {
                request.Logger.LogInformation("Error to Cosmos Container : " +  e.ToString());
                res.ResultText.Add(e.ToString());
            }
            return res;
        }
    }
}