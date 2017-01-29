using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Xunit;
using Microsoft.AspNetCore.TestHost;
using Mvc.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.WebUtilities;

namespace TwitterMWTests
{
    public class AuthenticationTests
    {
        private readonly TestServer server;

        private readonly HttpClient client;

        public AuthenticationTests()
        {
            // Arrange
            server = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>());
            client = server.CreateClient();
        }

        [Fact]
        public async Task Should_return_hello()
        {
            var loginItems = new Dictionary<string, string>() { { "provider", "Twitter" } };
            var response = await client.PostAsync("/signin", new FormUrlEncodedContent(loginItems));

            var queryString = QueryHelpers.ParseQuery(response.Headers.Location.Query);
            var twitterlogintoken = queryString["oauth_token"].First();

            Console.WriteLine(twitterlogintoken);

            //The below I have manually worked out from the HTML in the browser
            var twitterloginItems = new Dictionary<string, string>() { { "oauth_token", twitterlogintoken }, { "session[username_or_email]", "muusername" }, { "session[password]", "mypassword" } };
            var twitterClient = new HttpClient();
            //This seems to not login my user so fuck knows :)
            var loginResponse = await twitterClient.PostAsync("https://api.twitter.com/oauth/authenticate", new FormUrlEncodedContent(twitterloginItems));
            foreach (var header in loginResponse.Headers)
            {
                Console.WriteLine(header.Key + ":" + string.Join(",", header.Value));
            }

            //Hit our route in our app - we need to add a cookie so we can get to that route
            var homeresponse = await client.GetAsync("/home");
            var body = await homeresponse.Content.ReadAsStringAsync();
            Assert.Equal("hello", body);
        }
    }
}