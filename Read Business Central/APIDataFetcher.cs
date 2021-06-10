using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QBS.Cosmos
{
    class APIDataFetcher
    {
        static HttpClient client;
       
        public APIDataFetcher(RequestLogger request) 
        {
            RefreshHttpClient(request);
        }

        public void RefreshHttpClient(RequestLogger request)
        {
            byte[] UserInfo = System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", request.Request.Login.businessCentralUser,
                request.Request.Login.businessCentralKey));

            client = new HttpClient();
            client.BaseAddress = new Uri(string.Format("https://api.businesscentral.dynamics.com/v2.0/{0}/{6}/api/{1}/{2}/{3}/companies({4})/{5}",
                request.Request.tenandId, request.Request.apiPublisher, request.Request.apiGroup, request.Request.apiVersion, request.Request.companyId, request.Request.apiEndpoint, request.Request.sandBoxName));
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(UserInfo));
        }

        //This method sends a http request and converts the return-body into an array of json objects.
        public async Task<CosmosDataSet> GetItemsAsync(string query)
        {

            string JSON = "";
            HttpResponseMessage response = await client.GetAsync(query);
            if (response.IsSuccessStatusCode)
            {
                JSON = await response.Content.ReadAsStringAsync();
                return await ConvertToDataSet(JSON, "");
            }
            return null;
        }

        async Task<CosmosDataSet> ConvertToDataSet(string JSON, string tableName)
        {
            CosmosDataSet dataSet = new CosmosDataSet();
            dynamic json = JsonConvert.DeserializeObject(JSON);
            JObject myObject = new JObject();

            myObject = json["value"][0];
            string myString = myObject.SelectToken("jsonResult").ToString();
            dataSet = JsonConvert.DeserializeObject<CosmosDataSet>(myString);

            return dataSet;
        }
    }
}