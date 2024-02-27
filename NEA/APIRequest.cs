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
        private string url = "https://10.0.2.2:7123";
        public async void AddUser(string username)
        {
            int id = 358365890;
            //creates http client
            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            HttpClient client = new HttpClient(handler);
            client.BaseAddress = new Uri(url);

            //creates data to put in body of request
            var data = $"{{ \"uname\": \"{username}\" }}";
            var content = new StringContent(data, Encoding.UTF8, "application/json");
            //HttpResponseMessage response = await client.GetAsync($"User?id={id}");
            //does request
            //I CHANGED THIS TO BE "USER" IDK IF IT WORKS
            HttpResponseMessage response = await client.PostAsync("User", content);
            response.EnsureSuccessStatusCode();

            //prints response
            var jsonResponse = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"{jsonResponse}\n");
        }
    }
}