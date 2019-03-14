using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;

namespace AspNetCore.Proxy.Tests
{
    public class UnitTests
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;

        public UnitTests()
        {
            _server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            _client = _server.CreateClient();
        }

        [Fact]
        public async Task ProxyAttributeToTask()
        {
            var response = await _client.GetAsync("api/posts/totask/1");
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("sunt aut facere repellat provident occaecati excepturi optio reprehenderit", JObject.Parse(responseString).Value<string>("title"));
        }

        [Fact]
        public async Task ProxyAttributeToString()
        {
            var response = await _client.GetAsync("api/posts/tostring/1");
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("sunt aut facere repellat provident occaecati excepturi optio reprehenderit", JObject.Parse(responseString).Value<string>("title"));
        }

        [Fact]
        public async Task ProxyAttributePostRequest()
        {
            var content = new StringContent("{\"title\": \"foo\", \"body\": \"bar\", \"userId\": 1}", Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("api/posts", content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("101", JObject.Parse(responseString).Value<string>("id"));
        }

        [Fact]
        public async Task ProxyAttributeCatchAll()
        {
            var response = await _client.GetAsync("api/catchall/posts/1");
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("sunt aut facere repellat provident occaecati excepturi optio reprehenderit", JObject.Parse(responseString).Value<string>("title"));
        }

        [Fact]
        public async Task ProxyMiddlewareWithContextAndArgsToTask()
        {
            var response = await _client.GetAsync("api/comments/contextandargstotask/1");
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("id labore ex et quam laborum", JObject.Parse(responseString).Value<string>("name"));
        }

        [Fact]
        public async Task ProxyMiddlewareWithArgsToTask()
        {
            var response = await _client.GetAsync("api/comments/argstotask/1");
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("id labore ex et quam laborum", JObject.Parse(responseString).Value<string>("name"));
        }

        [Fact]
        public async Task ProxyMiddlewareWithEmptyToTask()
        {
            var response = await _client.GetAsync("api/comments/emptytotask");
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("id labore ex et quam laborum", JObject.Parse(responseString).Value<string>("name"));
        }

        [Fact]
        public async Task ProxyMiddlewareWithContextAndArgsToString()
        {
            var response = await _client.GetAsync("api/comments/contextandargstostring/1");
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("id labore ex et quam laborum", JObject.Parse(responseString).Value<string>("name"));
        }

        [Fact]
        public async Task ProxyMiddlewareWithArgsToString()
        {
            var response = await _client.GetAsync("api/comments/argstostring/1");
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("id labore ex et quam laborum", JObject.Parse(responseString).Value<string>("name"));
        }

        [Fact]
        public async Task ProxyMiddlewareWithEmptyToString()
        {
            var response = await _client.GetAsync("api/comments/emptytostring");
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("id labore ex et quam laborum", JObject.Parse(responseString).Value<string>("name"));
        }
    }

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseProxies();

            app.UseProxy("api/comments/contextandargstotask/{postId}", (context, args) => {
                context.GetHashCode();
                return Task.FromResult($"https://jsonplaceholder.typicode.com/comments/{args["postId"]}");
            });

            app.UseProxy("api/comments/argstotask/{postId}", (args) => {
                return Task.FromResult($"https://jsonplaceholder.typicode.com/comments/{args["postId"]}");
            });

            app.UseProxy("api/comments/emptytotask", () => {
                return Task.FromResult($"https://jsonplaceholder.typicode.com/comments/1");
            });

            app.UseProxy("api/comments/contextandargstostring/{postId}", (context, args) => {
                context.GetHashCode();
                return $"https://jsonplaceholder.typicode.com/comments/{args["postId"]}";
            });

            app.UseProxy("api/comments/argstostring/{postId}", (args) => {
                return $"https://jsonplaceholder.typicode.com/comments/{args["postId"]}";
            });

            app.UseProxy("api/comments/emptytostring", () => {
                return $"https://jsonplaceholder.typicode.com/comments/1";
            });
        }
    }

    public static class UseProxies
    {
        [ProxyRoute("api/posts/totask/{postId}")]
        public static Task<string> ProxyToTask(int postId)
        {
            return Task.FromResult($"https://jsonplaceholder.typicode.com/posts/{postId}");
        }

        [ProxyRoute("api/posts/tostring/{postId}")]
        public static string ProxyToString(int postId)
        {
            return $"https://jsonplaceholder.typicode.com/posts/{postId}";
        }

        [ProxyRoute("api/posts")]
        public static string ProxyPostRequest()
        {
            return $"https://jsonplaceholder.typicode.com/posts";
        }

        [ProxyRoute("api/catchall/{*rest}")]
        public static string ProxyCatchAll(string rest)
        {
            return $"https://jsonplaceholder.typicode.com/{rest}";
        }
    }
}
