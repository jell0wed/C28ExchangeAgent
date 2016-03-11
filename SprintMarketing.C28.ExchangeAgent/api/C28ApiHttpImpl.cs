using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;
using SprintMarketing.C28.ExchangeAgent.API.Models;

namespace SprintMarketing.C28.ExchangeAgent.API
{
    class C28APIHttpImpl : IC28WebApi
    {
        private const string ERROR_JSON_SCHEMA = "{\"$schema\":\"http://json-schema.org/draft-04/schema#\",\"id\":\"http://jsonschema.net\",\"type\":\"object\",\"properties\":{\"error\":{\"id\":\"http://jsonschema.net/error\",\"type\":\"object\",\"properties\":{\"message\":{\"id\":\"http://jsonschema.net/error/message\",\"type\":\"string\"}},\"required\":[\"message\"]}},\"required\":[\"error\"]}";

        private static readonly JSchema json_schema_error = JSchema.Parse(ERROR_JSON_SCHEMA);


        private readonly HttpClient client;
        private string apiKey = "";
        private string baseUri = "";

        public C28APIHttpImpl(String baseUri, String apiKey) {
            C28Logger.Debug(C28Logger.C28LoggerType.API, $"Initializing C28 HTTP WebAPI with api key '{apiKey}'");
            this.apiKey = apiKey;
            client = new HttpClient();
            this.baseUri = baseUri;

            this.client.DefaultRequestHeaders.Accept.Clear();
            this.client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        private async Task<string> getRequest(String uri) {
            try
            {
                using (HttpResponseMessage resp = await this.client.GetAsync(uri))
                using (HttpContent content = resp.Content)
                {
                    if (!resp.IsSuccessStatusCode)
                    {
                        C28Logger.Warn(C28Logger.C28LoggerType.API, String.Format("Potentially invalid status code from a GET HTTP Response (received {0})", resp.StatusCode));
                    }

                    String res = await resp.Content.ReadAsStringAsync();
                    JObject resJson = JObject.Parse(res);
                    if (resJson.IsValid(json_schema_error))
                    {
                        C28APIError err = JsonConvert.DeserializeObject<C28APIError>(res);
                        C28Logger.Error(C28Logger.C28LoggerType.API, String.Format("Unexpected API Error while processing GET Request : {0}", err.message));
                        C28Logger.Debug(C28Logger.C28LoggerType.API, "Received json: " + res);
                        throw new C28APIException(String.Format("Unexpected API Error : {0}", err.message));
                    }

                    return await resp.Content.ReadAsStringAsync();
                }
            }
            catch (Exception e) {
                C28Logger.Error(C28Logger.C28LoggerType.API, String.Format("Something went wrong when processing HTTP Get Request to URI {0}", uri));
                throw new Exception("Unable to process HTTP Get Request", e);
            }
        }

        private String getUri(String uri) {
            //TODO : use proper URI builder
            return this.baseUri + uri + "?api_key=" + this.apiKey;
        }

        public C28ExchangeData getExchangeData()
        {
            String domainJson = Task.Run(() => getRequest(this.getUri("/exchange"))).Result;
            var data = JsonConvert.DeserializeObject<C28ExchangeData>(domainJson);
            if (data == null) {
                C28Logger.Error(C28Logger.C28LoggerType.API, "Invalid exchange data sent from the API (unable to deserialize).");
                C28Logger.Debug(C28Logger.C28LoggerType.API, "Received: " + domainJson);
                throw new C28APIException("Received invalid serialized exchange data");
            }

            return data;
        }
    }
}
