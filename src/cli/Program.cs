using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace cli
{
    class Program
    {
        const string tokenEndpoint = @"https://login.microsoftonline.com/c4407ff9-b5f0-4ce7-9696-53c8758fcc25/oauth2/token";
        const string resourceId = "6ce552f6-4b27-4c6c-bfb8-1e96e5083686";
        const string clientId = "a8ce3afd-dac3-4e22-98fc-2f53fb1ff88f";

        static string esc(string s) => Uri.EscapeDataString(s); 
        static string query(string username, string password) =>
            string.Join("&", new[]
        {
            $"resource={esc(resourceId)}",
            $"client_id={esc(clientId)}",
            $"username={esc(username)}",
            $"grant_type=password",
            $"password={esc(password)}",
            $"scope=openid"
        });

        static Task<HttpResponseMessage> Auth(
            HttpClient c,
            string usr,
            string pwd) =>
            c.PostAsync(
                tokenEndpoint,
                new StringContent(
                    query(usr, pwd),
                    Encoding.UTF8,
                    "application/x-www-form-urlencoded"));

        static string PrettyPrint(string json) => JToken.Parse(json).ToString();

        static string Decode(string s) => 
            PrettyPrint(
                Encoding.UTF8.GetString(Convert.FromBase64String(s)));

        static string DecodeToken(string token) =>
            string.Join(Environment.NewLine, token.Split('.').Take(2).Select(Decode));
        
        static async Task Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("usage: cli username password");

                return;
            }

            var usr = args[0];
            var pwd = args[1];

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                try
                {
                    using (var response = await Auth(client, usr, pwd))
                    {
                        var req = await response.RequestMessage.Content.ReadAsStringAsync();
                        Console.WriteLine($"request {req}");

                        if (response.IsSuccessStatusCode)
                        {
                            Console.WriteLine("Received response:");

                            var json = await response.Content.ReadAsStringAsync();
                            Console.WriteLine(PrettyPrint(json));

                            var token = (string)JObject.Parse(json)["access_token"];
                            var parsed = DecodeToken(token);

                            Console.WriteLine("Parsed token");
                            Console.WriteLine(parsed);
                        }
                        else
                        {
                            Console.WriteLine("Non 200 result:");
                            var status = $"{response.StatusCode} {response.ReasonPhrase}";
                            var content = await response.Content.ReadAsStringAsync(); 
                            Console.WriteLine(PrettyPrint(content));
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"Exception: {e.Message}");
                }
            }
        }
    }
}
