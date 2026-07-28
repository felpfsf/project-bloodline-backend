using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Bloodline.Api.Features.Characters.GetCurrentCharacter;
using Bloodline.Api.Features.Hunts.CompleteHunt;
using Bloodline.Api.Features.Hunts.StartHunt;
using Bloodline.IntegrationTests.Infrastructure;

namespace Bloodline.IntegrationTests.Hunts;

[Collection(PostgreSqlCollection.Name)]
public sealed class CompleteHuntFlowTests(PostgreSqlFixture fixture)
{
  private readonly BloodlineWebApplicationFactory _factory = fixture.Factory;

  [Fact]
  public async Task CompleteHunt_Should_Complete_Hunt_And_Reward_Character_When_End_Time_Has_Arrived()
  {
    var client = await CreateAuthenticatedClientAsync("CompleteHunt");
    var startedHunt = await StartHuntAsync(client);

    _factory.TestClock.UtcNow = startedHunt.EndsAtUtc;

    var response = await client.PostAsync($"api/hunts/{startedHunt.HuntId}/complete", content: null);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var completedHunt = await response.Content.ReadFromJsonAsync<CompleteHuntResponse>();

    Assert.NotNull(completedHunt);
    Assert.Equal(startedHunt.HuntId, completedHunt!.HuntId);
    Assert.Equal(startedHunt.EndsAtUtc, completedHunt.CompletedAtUtc);
    Assert.Equal(25, completedHunt.ExperienceReward);
    Assert.Equal(15, completedHunt.GoldReward);

    var character = await GetCurrentCharacterAsync(client);

    Assert.Equal(25, character.Experience);
    Assert.Equal(15, character.Gold);
  }

  [Fact]
  public async Task CompleteHunt_Should_Not_Duplicate_Reward_When_Completed_Multiple_Times()
  {
    var client = await CreateAuthenticatedClientAsync("DupHunt");
    var startedHunt = await StartHuntAsync(client);

    _factory.TestClock.UtcNow = startedHunt.EndsAtUtc;

    var firstResponse = await client.PostAsync($"api/hunts/{startedHunt.HuntId}/complete", content: null);
    var secondResponse = await client.PostAsync($"api/hunts/{startedHunt.HuntId}/complete", content: null);

    Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
    Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);

    var character = await GetCurrentCharacterAsync(client);

    Assert.Equal(25, character.Experience);
    Assert.Equal(15, character.Gold);
  }

  [Fact]
  public async Task CompleteHunt_Should_Not_Duplicate_Reward_When_Completed_Concurrently()
  {
    var client = await CreateAuthenticatedClientAsync("ConcurHunt");
    var startedHunt = await StartHuntAsync(client);

    _factory.TestClock.UtcNow = startedHunt.EndsAtUtc;

    var completeHuntUri = $"api/hunts/{startedHunt.HuntId}/complete";
    var completionResponses = await Task.WhenAll(
      client.PostAsync(completeHuntUri, content: null),
      client.PostAsync(completeHuntUri, content: null)
    );

    Assert.All(completionResponses, response => Assert.Equal(HttpStatusCode.OK, response.StatusCode));

    var character = await GetCurrentCharacterAsync(client);

    Assert.Equal(25, character.Experience);
    Assert.Equal(15, character.Gold);
  }

  [Fact]
  public async Task CompleteHunt_Should_Return_Conflict_When_Character_Has_An_Active_Hunt_But_End_Time_Has_Not_Arrived()
  {
    var client = await CreateAuthenticatedClientAsync("PendingHunt");
    var startedHunt = await StartHuntAsync(client);

    _factory.TestClock.UtcNow = startedHunt.EndsAtUtc.AddSeconds(-1);

    var response = await client.PostAsync($"api/hunts/{startedHunt.HuntId}/complete", content: null);

    Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
  }

  private async Task<HttpClient> CreateAuthenticatedClientAsync(string characterNamePrefix)
  {
    _factory.TestClock.UtcNow = new DateTimeOffset(2026, 7, 27, 0, 0, 0, TimeSpan.Zero);

    var client = _factory.CreateClient();
    var accessToken = await TestAuthUtils.RegisterLoginAndCreateCharacterAsync(
      client,
      characterName: TestAuthUtils.UniqueCharacterName(characterNamePrefix)
    );

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    return client;
  }

  private static async Task<StartHuntResponse> StartHuntAsync(HttpClient client)
  {
    var response = await client.PostAsync("api/hunts", content: null);

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);

    var hunt = await response.Content.ReadFromJsonAsync<StartHuntResponse>();

    Assert.NotNull(hunt);

    return hunt!;
  }

  private static async Task<GetCurrentCharacterResponse> GetCurrentCharacterAsync(HttpClient client)
  {
    var response = await client.GetAsync("api/characters/me");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var character = await response.Content.ReadFromJsonAsync<GetCurrentCharacterResponse>();

    Assert.NotNull(character);

    return character!;
  }
}