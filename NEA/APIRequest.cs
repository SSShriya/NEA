using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NEA
{
    public class APIRequest
    {
        private string url = "https://10.0.2.2:7123/";
        public async Task<string> AddUser(string username)
        {
            //creates http client
            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            HttpClient client = new HttpClient(handler);
            client.BaseAddress = new Uri(url);
            
            //creates data to put in body of request
            string data = $"\"{username}\"";
            var content = new StringContent(data, Encoding.UTF8, "application/json");
            var jsonResponse = "";
            try
            {
                //does request
                HttpResponseMessage response = await client.PostAsync("User", content);
                response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                {
                    //prints response
                    jsonResponse = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                client.Dispose();
            }
            return jsonResponse;
        }
    }
}