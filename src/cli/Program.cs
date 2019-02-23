using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using cli.StringExtensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace cli
{
    class Program
    {
        const string tokenEndpoint = "https://login.microsoftonline.com/c4407ff9-b5f0-4ce7-9696-53c8758fcc25/oauth2/v2.0/token";
        const string resourceId = "6ce552f6-4b27-4c6c-bfb8-1e96e5083686";
        const string clientId = "a8ce3afd-dac3-4e22-98fc-2f53fb1ff88f";

        static string esc(string s) => Uri.EscapeDataString(s); 
        static string queryV2(string username, string password) =>
            string.Join("&", new[]
        {
            //$"resource={esc(resourceId)}",
            $"client_id={esc(clientId)}",
            $"username={esc(username)}",
            $"grant_type=password",
            $"password={esc(password)}",
            $"scope={esc($"openid https://punchclock-api/user_impersonation")}"
        });

        static String queryV1(string username, string password) =>
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
                    queryV2(usr, pwd),
                    Encoding.UTF8,
                    "application/x-www-form-urlencoded"));

        static async Task<int> Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Error.WriteLine("usage: cli username password");
                return 1;
            }

            var usr = args[0];
            var pwd = args[1];
            var output = "";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                try
                {
                    using (var response = await Auth(client, usr, pwd))
                    {
                        var req = await response.RequestMessage.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            output = (string)JObject.Parse(json)["access_token"];
                        }
                        else
                        {
                            var status = $"{response.StatusCode} {response.ReasonPhrase}";
                            var content = await response.Content.ReadAsStringAsync(); 
                            Console.Error.WriteLine($"{status}{Environment.NewLine}{content.PrettyPrint()}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"Exception: {e.Message}");
                    return 1;
                }
            }

            Console.Write(output);
            return 0;
        }
    }
}
