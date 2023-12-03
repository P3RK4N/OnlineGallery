using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Assert = Xunit.Assert;


[TestFixture]
public class UnitTest
{
    APUW.WebServer server;
    static string BaseUrl = "http://localhost:5000";

    // Pero
    static HttpContent Pero1Content = new FormUrlEncodedContent(new[]
    {
        new KeyValuePair<string, string>("username", "pero1"),
        new KeyValuePair<string, string>("password", "Pero123!")
    });

    // Pero2
    static HttpContent Pero2Content = new FormUrlEncodedContent(new[]
    {
        new KeyValuePair<string, string>("username", "pero2"),
        new KeyValuePair<string, string>("password", "Pero123!")
    });
    // Invalid content
    static HttpContent InvalidContent = new FormUrlEncodedContent(new[]
    {
        new KeyValuePair<string, string>("username", "pero21"),
        new KeyValuePair<string, string>("password", "Pe")
    });

    static void Line(string log) { Console.WriteLine($"Unit Test Log: {log}"); }

    static async Task RegisterUsers(HttpClient client)
    {
        await client.PostAsync($"{BaseUrl}/api/user/register", Pero1Content);
        await client.PostAsync($"{BaseUrl}/api/user/register", Pero2Content);
        await client.PostAsync($"{BaseUrl}/api/user/register", InvalidContent);
    }

    static APUW.UserDTO[]? GetUsersFromContent(string content)
    {
        try
        {
            return JsonConvert.DeserializeObject<APUW.UserDTO[]>(content);
        }
        catch(Exception)
        {
            return null;
        }
    }

    static APUW.UserDTO? GetUserFromContent(string content)
    {
        try
        {
            return JsonConvert.DeserializeObject<APUW.UserDTO>(content);
        }
        catch(Exception)
        {
            return null;
        }
    }

    [SetUp]
    public void Setup()
    {
        server = new APUW.WebServer();
        server.Start();
    }

    [TearDown]
    public void TearDown()
    {
        server.Stop();
    }

    [Test]
    public async Task GetHome_Test()
    {
        // Arrange
        using (var client = new HttpClient())
        {
            // Act
            var response = await client.GetAsync($"{BaseUrl}/");            

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        }
    }

    [Test]
    public async Task GetRegister_Test()
    {
        // Arrange
        using (var client = new HttpClient())
        {
            // Act
            var response = await client.GetAsync($"{BaseUrl}/api/user/register");
            string content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/plain", response.Content.Headers.ContentType?.MediaType);
        }
    }

    [Test]
    public async Task PostRegister_Test()
    {
        // Arrange
        using (var client = new HttpClient())
        {
            var res1 = await client.PostAsync($"{BaseUrl}/api/user/register", Pero1Content);
            var res2 = await client.PostAsync($"{BaseUrl}/api/user/register", Pero2Content);
            var invalidRes = await client.PostAsync($"{BaseUrl}/api/user/register", InvalidContent);
            
            // Act
            string content1 = await res1.Content.ReadAsStringAsync();
            string content2 = await res2.Content.ReadAsStringAsync();
            string invalidContent = await invalidRes.Content.ReadAsStringAsync();

            Line(content1);
            
            // Assert
            Assert.Equal(HttpStatusCode.OK, res1.StatusCode);
            Assert.Equal(HttpStatusCode.OK, res2.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, invalidRes.StatusCode);
        }
    }

    [Test]
    public async Task PostLogin_Test()
    {
        // Arrange
        using (var client = new HttpClient())
        {
            await RegisterUsers(client);
         
            var res1                = await client.PostAsync($"{BaseUrl}/api/user/login", Pero1Content);
            var res2                = await client.PostAsync($"{BaseUrl}/api/user/login", Pero2Content);
            var invalidRes          = await client.PostAsync($"{BaseUrl}/api/user/login", InvalidContent);

            // Act
            string content1         = await res1.Content.ReadAsStringAsync();
            string content2         = await res2.Content.ReadAsStringAsync();
            string invalidContent   = await invalidRes.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, res1.StatusCode);
            Assert.Equal(HttpStatusCode.OK, res2.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, invalidRes.StatusCode);

            Assert.Equal("pero1", GetUserFromContent(content1)?.UserName);
            Assert.Equal("pero2", GetUserFromContent(content2)?.UserName);
        }
    }
}