using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Bloodline.Api.Features.Identity.Login;

namespace Bloodline.IntegrationTests.Infrastructure;

public static class TestAuthUtils
{
  public static string UniqueCharacterName(string prefix) => $"{prefix}{Guid.NewGuid():N}"[..(prefix.Length + 8)];

  public static async Task<HttpClient> CreateAuthenticatedClientAsync(BloodlineWebApplicationFactory factory)
  {
    var client = factory.CreateClient();
    var accessToken = await RegisterAndLoginAsync(client);

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    return client;
  }

  public static async Task<string> RegisterAndLoginAsync(HttpClient client)
  {
    var email = $"player{Guid.NewGuid():N}@bloodline.com";
    const string password = "Password123!";

    var registerResponse = await client.PostAsJsonAsync("api/auth/register", new
    {
      Email = email,
      Password = password
    });

    Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

    var loginResponse = await client.PostAsJsonAsync("api/auth/login", new
    {
      Email = email,
      Password = password
    });

    Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

    var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

    Assert.NotNull(loginBody);
    Assert.False(string.IsNullOrWhiteSpace(loginBody!.AccessToken));

    return loginBody.AccessToken;
  }
}