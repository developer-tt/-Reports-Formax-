using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TimeTrackerBIXF.Data.AuxModels;
using TimeTrackerBIXF.Utils;

namespace TimeTrackerBIXF.DataServices
{
    public static class WSMethods
    {
        public static async Task<Response> Get(string url)
        {
            string jResponseContent = null;

            Response ModelResponse = new Response();

            var uri = new Uri(url);

            HttpResponseMessage response = null;
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization-Token", Constants.AuthToken);

            try
            {
                response = await client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    jResponseContent = await response.Content.ReadAsStringAsync();
                    ModelResponse.Result = Result.OK;
                    ModelResponse.Data = jResponseContent;
                }
                else
                {
                    ModelResponse.Result = Result.ERROR_GETTING_DATA;
                }
            }
            catch (Exception ex)
            {
                ModelResponse.Result = Result.SERVICE_EXCEPTION;
            }
            return ModelResponse;
        }

        public static async Task<Response> Post(string url, Object objContent)
        {
            string jResponseContent = null;
            Response ModelResponse = new Response();

            var uri = new Uri(url);
            var json = JsonConvert.SerializeObject(objContent);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = null;
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization-Token", Constants.AuthToken);

            try
            {
                response = await client.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    jResponseContent = await response.Content.ReadAsStringAsync();
                    ModelResponse.Result = Result.OK;
                    ModelResponse.Data = jResponseContent;
                }
                else
                {
                    ModelResponse.Result = Result.ERROR_GETTING_DATA;
                }
            }
            catch (Exception ex)
            {
                ModelResponse.Result = Result.SERVICE_EXCEPTION;
            }
            return ModelResponse;
        }

        public static async Task<Response> Put(string url, Object objContent)
        {
            string jResponseContent = null;
            Response ModelResponse = new Response();

            var uri = new Uri(url);
            var json = JsonConvert.SerializeObject(objContent);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = null;
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization-Token", Constants.AuthToken);
         
            try
            {
                response = await client.PutAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    jResponseContent = await response.Content.ReadAsStringAsync();
                    ModelResponse.Result = Result.OK;
                    ModelResponse.Data = jResponseContent;
                }
                else
                {
                    ModelResponse.Result = Result.ERROR_GETTING_DATA;
                }
            }
            catch (Exception ex)
            {
                ModelResponse.Result = Result.SERVICE_EXCEPTION;
            }
            return ModelResponse;
        }
    }
}
