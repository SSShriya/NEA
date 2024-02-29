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
            byte[] messageBytes = Encoding.UTF8.GetBytes(data);
            var content = new ByteArrayContent(messageBytes);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
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
                jsonResponse = "";
            }
            finally
            {
                client.Dispose();
            }
            return jsonResponse;
        }

        public async Task<string> EditUsername(int id, string username)
        {
            //creates http client
            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            HttpClient client = new HttpClient(handler);
            client.BaseAddress = new Uri(url);

            //creates data to put in body of request
            string data = $"{{\"uid\": {id},\"uName\": \"{username}\"}}";
            byte[] messageBytes = Encoding.UTF8.GetBytes(data);
            var content = new ByteArrayContent(messageBytes);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var jsonResponse = "";
            try
            {
                //does request
                HttpResponseMessage response = await client.PutAsync($"User", content);
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
                jsonResponse = "";
            }
            finally
            {
                client.Dispose();
            }
            return jsonResponse;
        }

        public async Task<string> GetUser(int UID)
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            HttpClient client = new HttpClient(handler);
            client.BaseAddress = new Uri(url);
            var jsonResponse = "";
            try
            {
                //does request
                HttpResponseMessage response = await client.GetAsync($"User/{UID}");
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
                jsonResponse = "";
            }
            finally
            {
                client.Dispose();
            }
            return jsonResponse;
        }

        public async Task<string> AddPairing(int user1, int user2)
        {
            //creates http client
            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            HttpClient client = new HttpClient(handler);
            client.BaseAddress = new Uri(url);

            //creates data to put in body of request
            string data = $"{{\"u1\": {user1}, \"u2\": {user2}}}";
            byte[] messageBytes = Encoding.UTF8.GetBytes(data);
            var content = new ByteArrayContent(messageBytes);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var jsonResponse = "";
            try
            {
                //does request
                HttpResponseMessage response = await client.PostAsync("Pairing", content);
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
                jsonResponse = "";
            }
            finally
            {
                client.Dispose();
            }
            return jsonResponse;
        }
    }
}