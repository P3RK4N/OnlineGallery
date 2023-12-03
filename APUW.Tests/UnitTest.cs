using APUW;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Assert = Xunit.Assert;


[TestFixture]
public class UnitTest
{
#region Util & State

    APUW.WebServer server;
    static string BaseUrl = "http://localhost:5000";

    // Empty
    static HttpContent EmptyContent = new FormUrlEncodedContent(new List<KeyValuePair<string,string>>());

    // Pero
    static HttpContent Pero1Content = new FormUrlEncodedContent(new[]
    {
        new KeyValuePair<string, string>("username", "pero1"),
        new KeyValuePair<string, string>("password", "Pero123!")
    });

    // Pero updated
    static HttpContent Pero1UpdatedContent = new FormUrlEncodedContent(new[]
    {
        new KeyValuePair<string, string>("username", "pero1updated"),
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

    static HttpContent MockImageContent = new ByteArrayContent(new byte[]{ 0,0,0,01,01,0,1,1,01,01,01,0,0,010,10,1,01,01,112,0,0,0,0,101,20,120,120 });
    static HttpContent MockImageUpdatedContent = new ByteArrayContent(new byte[]{ 0,1,1,1,01,01,01,0,0,010,10,1,01,01,112,0,0,0,0,101,20,120,120 });

    static void Line(string log) { Console.WriteLine($"Unit Test Log: {log}"); }

    static async Task RegisterUsers(HttpClient client)
    {
        await client.PostAsync($"{BaseUrl}/api/user/register", Pero1Content);
        await client.PostAsync($"{BaseUrl}/api/user/register", Pero2Content);
        await client.PostAsync($"{BaseUrl}/api/user/register", InvalidContent);
    }

    static async Task LoginUser(HttpClient client, HttpContent content)
    {
        await client.PostAsync($"{BaseUrl}/api/user/login", content);
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

    static async Task<APUW.UserDTO[]?> GetUsers(HttpClient client)
    {
        var res = await client.GetAsync($"{BaseUrl}/api/users");
        var content = await res.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<APUW.UserDTO[]>(content);
    }

    static async Task<APUW.UserDTO?> GetUser(HttpClient client)
    {
        var res = await client.GetAsync($"{BaseUrl}/api/user");
        var content = await res.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<APUW.UserDTO>(content);
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

    static async Task CreateNewPhoto(HttpClient client)
    {
        await client.PostAsync($"{BaseUrl}/api/photos/{"MockImage"}/create", MockImageContent);
    }

#endregion

    [SetUp]
    public void Setup()
    {
        MockImageContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        MockImageUpdatedContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
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
    public async Task GetLogin_Test()
    {
        // Arrange
        using (var client = new HttpClient())
        {
            // Act
            var response = await client.GetAsync($"{BaseUrl}/api/user/login");
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
         
            // Act
            var res1                = await client.PostAsync($"{BaseUrl}/api/user/login", Pero1Content);
            var res2                = await client.PostAsync($"{BaseUrl}/api/user/login", Pero2Content);
            var invalidRes          = await client.PostAsync($"{BaseUrl}/api/user/login", InvalidContent);

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

    [Test]
    public async Task PostLogout_Test()
    {
        // Arrange
        using (var client = new HttpClient())
        {
            await RegisterUsers(client);

            // Act
            var invalidLogout1  = await client.PostAsync($"{BaseUrl}/api/user/logout", EmptyContent);
            
            await LoginUser(client, Pero1Content);

            var validLogout     = await client.PostAsync($"{BaseUrl}/api/user/logout", EmptyContent);
            var invalidLogout2  = await client.PostAsync($"{BaseUrl}/api/user/logout", EmptyContent);

            var userCount = (await GetUsers(client))?.Length;

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, invalidLogout1.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, invalidLogout2.StatusCode);
            Assert.Equal(HttpStatusCode.OK, validLogout.StatusCode);
            Assert.Equal(2, userCount);
        }
    }

    [Test]
    public async Task PostDeleteUser_Test()
    {
        // Arrange
        using (var client = new HttpClient())
        {
            await RegisterUsers(client);

            // Act
            var invalidUserDelete1  = await client.PostAsync($"{BaseUrl}/api/user/delete", EmptyContent);
            
            await LoginUser(client, Pero1Content);

            var validUserDelete1 = await client.PostAsync($"{BaseUrl}/api/user/delete", EmptyContent);
            var invalidUserDelete2  = await client.PostAsync($"{BaseUrl}/api/user/delete", EmptyContent);

            var userCount1 = (await GetUsers(client))?.Length;

            await LoginUser(client, Pero2Content);

            var validUserDelete2 = await client.PostAsync($"{BaseUrl}/api/user/delete", EmptyContent);
            var invalidUserDelete3  = await client.PostAsync($"{BaseUrl}/api/user/delete", EmptyContent);

            var userCount2 = (await GetUsers(client))?.Length;

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, invalidUserDelete1.StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, invalidUserDelete2.StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, invalidUserDelete3.StatusCode);
            Assert.Equal(HttpStatusCode.OK, validUserDelete1.StatusCode);
            Assert.Equal(HttpStatusCode.OK, validUserDelete2.StatusCode);
            Assert.Equal(1, userCount1);
            Assert.Equal(0, userCount2);
        }
    }

    [Test]
    public async Task PostUpdateUser_Test()
    {
        // Arrange
        using (var client = new HttpClient())
        {
            await RegisterUsers(client);

            // Act
            var invalidUserUpdate1  = await client.PostAsync($"{BaseUrl}/api/user/update", Pero1Content);
            
            await LoginUser(client, Pero1Content);

            var invalidUserUpdate2  = await client.PostAsync($"{BaseUrl}/api/user/update", EmptyContent);
            var validUserUpdate1 = await client.PostAsync($"{BaseUrl}/api/user/update", Pero1UpdatedContent);

            var user = await GetUser(client);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, invalidUserUpdate1.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, invalidUserUpdate2.StatusCode);
            Assert.Equal(HttpStatusCode.OK, validUserUpdate1.StatusCode);
            Assert.Equal("pero1updated", user?.UserName);
        }
    }

    [Test]
    public async Task GetUser_Test()
    {
        // Arrange
        using (var client = new HttpClient())
        {
            await RegisterUsers(client);

            // Act
            var invalidUserGet1  = await client.GetAsync($"{BaseUrl}/api/user");
            
            await LoginUser(client, Pero1Content);

            var validUserGet1 = await client.GetAsync($"{BaseUrl}/api/user");
            var userContent = await validUserGet1.Content.ReadAsStringAsync();
            var user = GetUserFromContent(userContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, invalidUserGet1.StatusCode);
            Assert.Equal("text/plain", invalidUserGet1.Content.Headers.ContentType?.MediaType);
            Assert.Equal(HttpStatusCode.OK, validUserGet1.StatusCode);
            Assert.Equal("pero1", user?.UserName);
        }
    }

    [Test]
    public async Task GetUsers_Test()
    {
        // Arrange
        using (var client = new HttpClient())
        {
            // Act
            var users1 = await client.GetAsync($"{BaseUrl}/api/users");
            var users1count = GetUsersFromContent(await users1.Content.ReadAsStringAsync())?.Length;
            
            await RegisterUsers(client);

            var users2 = await client.GetAsync($"{BaseUrl}/api/users");
            var users2count = GetUsersFromContent(await users2.Content.ReadAsStringAsync())?.Length;

            // Assert
            Assert.Equal(0, users1count);
            Assert.Equal(2, users2count);
        }
    }

    [Test]
    public async Task GetUserByUsername_Test()
    {
        // Arrange
        using (var client = new HttpClient())
        {
            // Act
            var invalidUserGet = await client.GetAsync($"{BaseUrl}/api/users/{"pero1"}");
            
            await RegisterUsers(client);

            var validUserGet = await client.GetAsync($"{BaseUrl}/api/users/{"pero1"}");
            var user = GetUserFromContent(await validUserGet.Content.ReadAsStringAsync());

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, invalidUserGet.StatusCode);
            Assert.Equal(HttpStatusCode.OK, validUserGet.StatusCode);
            Assert.Equal("pero1", user?.UserName);
        }
    }

    [Test]
    public async Task GetUserPhotos_Test()
    {
        // Arrange
        using (var client = new HttpClient())
        {
            // Act
            var invalidUserPhotosGet = await client.GetAsync($"{BaseUrl}/api/users/{"pero1"}/photos");
            
            await RegisterUsers(client);
            await LoginUser(client, Pero1Content);

            var validUserPhotosGet1 = await client.GetAsync($"{BaseUrl}/api/users/{"pero1"}/photos");
            var content1 = await validUserPhotosGet1.Content.ReadAsStringAsync();
            var photos1 = JsonConvert.DeserializeObject<PhotoDTO[]>(content1);

            await CreateNewPhoto(client);

            var validUserPhotosGet2 = await client.GetAsync($"{BaseUrl}/api/users/{"pero1"}/photos");
            var content2 = await validUserPhotosGet2.Content.ReadAsStringAsync();
            var photos2 = JsonConvert.DeserializeObject<PhotoDTO[]>(content2);

            await LoginUser(client, Pero2Content);
            await CreateNewPhoto(client);

            var validUserPhotosGet3 = await client.GetAsync($"{BaseUrl}/api/users/{"pero1"}/photos");
            var content3 = await validUserPhotosGet2.Content.ReadAsStringAsync();
            var photos3 = JsonConvert.DeserializeObject<PhotoDTO[]>(content2);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, invalidUserPhotosGet.StatusCode);
            Assert.Equal(HttpStatusCode.OK, validUserPhotosGet1.StatusCode);
            Assert.Equal(HttpStatusCode.OK, validUserPhotosGet2.StatusCode);
            Assert.Equal(0, photos1?.Length);
            Assert.Equal(1, photos2?.Length);
            Assert.Equal(1, photos3?.Length);

            Assert.Equal("MockImage", photos2[0]?.FileName);
            Assert.Equal("image/png", photos2[0]?.ContentType);
            Assert.Equal((int)MockImageContent.Headers.ContentLength.Value, photos2[0]?.Content.Length);
        }
    }

    [Test]
    public async Task GetPhotos_Test()
    {
        var photoCount = async (HttpResponseMessage res) => JsonConvert.DeserializeObject<PhotoDTO[]>(await res.Content.ReadAsStringAsync())?.Length;

        // Arrange
        using (var client = new HttpClient())
        {
            await RegisterUsers(client);

            // Act
            var photosGet1 = await client.GetAsync($"{BaseUrl}/api/photos");
            var photoCount1 = await photoCount(photosGet1);

            await CreateNewPhoto(client);
            
            var photosGet2 = await client.GetAsync($"{BaseUrl}/api/photos");
            var photoCount2 = await photoCount(photosGet2);

            await LoginUser(client, Pero1Content);
            await CreateNewPhoto(client);

            var photosGet3 = await client.GetAsync($"{BaseUrl}/api/photos");
            var photoCount3 = await photoCount(photosGet3);

            await LoginUser(client, Pero2Content);
            await CreateNewPhoto(client);

            var photosGet4 = await client.GetAsync($"{BaseUrl}/api/photos");
            var photoCount4 = await photoCount(photosGet4);

            // Assert
            Assert.Equal(0, photoCount1);
            Assert.Equal(0, photoCount2);
            Assert.Equal(1, photoCount3);
            Assert.Equal(2, photoCount4);
        }
    }

    [Test]
    public async Task GetPhotoByID_Test()
    {
        var photoCount = async (HttpResponseMessage res) => JsonConvert.DeserializeObject<PhotoDTO[]>(await res.Content.ReadAsStringAsync())?.Length;

        // Arrange
        using (var client = new HttpClient())
        {
            await RegisterUsers(client);
            await LoginUser(client, Pero1Content);
            await CreateNewPhoto(client);

            // Act
            var invalidPhotoGet = await client.GetAsync($"{BaseUrl}/api/photos/{15}");
            var validPhotoGet = await client.GetAsync($"{BaseUrl}/api/photos/{1}");
            
            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, invalidPhotoGet.StatusCode);
            Assert.Equal(HttpStatusCode.OK, validPhotoGet.StatusCode);
        }
    }

    [Test]
    public async Task GetPhotoUser_Test()
    {
        var photoCount = async (HttpResponseMessage res) => JsonConvert.DeserializeObject<PhotoDTO[]>(await res.Content.ReadAsStringAsync())?.Length;

        // Arrange
        using (var client = new HttpClient())
        {
            await RegisterUsers(client);
            await LoginUser(client, Pero1Content);
            await CreateNewPhoto(client);

            // Act
            var invalidPhotoUserGet = await client.GetAsync($"{BaseUrl}/api/photos/{15}/user");
            var validPhotoUserGet = await client.GetAsync($"{BaseUrl}/api/photos/{1}/user");
            var user = JsonConvert.DeserializeObject<UserDTO>(await validPhotoUserGet.Content.ReadAsStringAsync());

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, invalidPhotoUserGet.StatusCode);
            Assert.Equal(HttpStatusCode.OK, validPhotoUserGet.StatusCode);
            Assert.Equal("pero1", user?.UserName);
        }
    }

    [Test]
    public async Task PostCreatePhoto_Test()
    {
        var photoCount = async (HttpResponseMessage res) => JsonConvert.DeserializeObject<PhotoDTO[]>(await res.Content.ReadAsStringAsync())?.Length;

        // Arrange
        using (var client = new HttpClient())
        {
            await RegisterUsers(client);

            // Act
            var invalidPhotoCreate1 = await client.PostAsync($"{BaseUrl}/api/photos/{"MockImage"}/create", MockImageContent);
            
            await LoginUser(client, Pero1Content);
            
            var invalidPhotoCreate2 = await client.PostAsync($"{BaseUrl}/api/photos/{"MockImage"}/create", EmptyContent);
            var validPhotoCreate = await client.PostAsync($"{BaseUrl}/api/photos/{"MockImage"}/create", MockImageContent);

            var photoGet = await client.GetAsync($"{BaseUrl}/api/photos/{1}");
            var photo = await photoGet.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, invalidPhotoCreate1.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, invalidPhotoCreate2.StatusCode);
            Assert.Equal(HttpStatusCode.OK, validPhotoCreate.StatusCode);
            Assert.Equal((int)MockImageContent.Headers.ContentLength.Value, photo.Length);
        }
    }

    [Test]
    public async Task PostUpdatePhoto_Test()
    {
        var photoCount = async (HttpResponseMessage res) => JsonConvert.DeserializeObject<PhotoDTO[]>(await res.Content.ReadAsStringAsync())?.Length;

        // Arrange
        using (var client = new HttpClient())
        {
            await RegisterUsers(client);

            // Act
            var invalidPhotoUpdate1 = await client.PostAsync($"{BaseUrl}/api/photos/{1}/update", MockImageContent);
            
            await LoginUser(client, Pero1Content);
            await CreateNewPhoto(client);

            await LoginUser(client, Pero2Content);

            var invalidPhotoUpdate2 = await client.PostAsync($"{BaseUrl}/api/photos/{1}/update", MockImageContent);

            await LoginUser(client, Pero1Content);

            var invalidPhotoUpdate3 = await client.PostAsync($"{BaseUrl}/api/photos/{2}/update", EmptyContent);
            var validPhotoUpdate = await client.PostAsync($"{BaseUrl}/api/photos/{1}/update", MockImageUpdatedContent);
            
            var imageGet = await client.GetAsync($"{BaseUrl}/api/photos/{1}");
            var image = await imageGet.Content.ReadAsStringAsync();

            Line(validPhotoUpdate.ToString());
            Line(await validPhotoUpdate.Content.ReadAsStringAsync());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, invalidPhotoUpdate1.StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, invalidPhotoUpdate2.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, invalidPhotoUpdate3.StatusCode);
            Assert.Equal(HttpStatusCode.OK, validPhotoUpdate.StatusCode);
            Assert.Equal(image.Length, MockImageUpdatedContent.Headers.ContentLength.Value);
            Assert.Equal("image/jpeg", imageGet.Content.Headers.ContentType?.MediaType);
        }
    }

    [Test]
    public async Task PostDeletePhoto_Test()
    {
        var photoCount = async (HttpResponseMessage res) => JsonConvert.DeserializeObject<PhotoDTO[]>(await res.Content.ReadAsStringAsync())?.Length;

        // Arrange
        using (var client = new HttpClient())
        {
            await RegisterUsers(client);

            // Act
            var invalidPhotoDelete1 = await client.PostAsync($"{BaseUrl}/api/photos/{1}/delete", EmptyContent);
            
            await LoginUser(client, Pero1Content);
            await CreateNewPhoto(client);

            await LoginUser(client, Pero2Content);

            var invalidPhotoDelete2 = await client.PostAsync($"{BaseUrl}/api/photos/{1}/delete", EmptyContent);

            await LoginUser(client, Pero1Content);

            var invalidPhotoDelete3 = await client.PostAsync($"{BaseUrl}/api/photos/{2}/delete", EmptyContent);

            var imagesGet1 = await client.GetAsync($"{BaseUrl}/api/photos");
            var images1 = JsonConvert.DeserializeObject<PhotoDTO[]>(await imagesGet1.Content.ReadAsStringAsync());

            var validPhotoDelete = await client.PostAsync($"{BaseUrl}/api/photos/{1}/delete", EmptyContent);
            
            var imagesGet2 = await client.GetAsync($"{BaseUrl}/api/photos");
            var images2 = JsonConvert.DeserializeObject<PhotoDTO[]>(await imagesGet2.Content.ReadAsStringAsync());
            
            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, invalidPhotoDelete1.StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, invalidPhotoDelete2.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, invalidPhotoDelete3.StatusCode);
            Assert.Equal(HttpStatusCode.OK, validPhotoDelete.StatusCode);
            Assert.Equal(1, images1?.Length);
            Assert.Equal(0, images2?.Length);
        }
    }
}