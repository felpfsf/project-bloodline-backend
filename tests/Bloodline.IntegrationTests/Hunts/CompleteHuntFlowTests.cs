using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Bloodline.Api.Features.Characters.GetCurrentCharacter;
using Bloodline.Api.Features.Hunts.CompleteHunt;
using Bloodline.Api.Features.Hunts.GetActiveHunt;
using Bloodline.Api.Features.Hunts.StartHunt;
using Bloodline.Domain.Hunts;
using Bloodline.IntegrationTests.Infrastructure;
using Bloodline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bloodline.IntegrationTests.Hunts;

[Collection(PostgreSqlCollection.Name)]
public sealed class CompleteHuntFlowTests(PostgreSqlFixture fixture)
{
  private readonly BloodlineWebApplicationFactory _factory = fixture.Factory;

  [Fact]
  public async Task CompleteHunt_Should_Return_Unauthorized_When_Token_Is_Missing()
  {
    var client = _factory.CreateClient();

    var response = await client.PostAsync($"api/hunts/{Guid.NewGuid()}/complete", content: null);

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

  [Fact]
  public async Task CompleteHunt_Should_Return_CharacterNotFound_When_Authenticated_User_Has_No_Character()
  {
    var client = _factory.CreateClient();
    var accessToken = await TestAuthUtils.RegisterAndLoginAsync(client);

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    var response = await client.PostAsync($"api/hunts/{Guid.NewGuid()}/complete", content: null);

    await AssertProblemAsync(response, HttpStatusCode.NotFound, "character_not_found");
  }

  [Fact]
  public async Task CompleteHunt_Should_Return_HuntNotFound_When_Hunt_Does_Not_Exist()
  {
    var client = await CreateAuthenticatedClientAsync("MissingHunt");

    var response = await client.PostAsync($"api/hunts/{Guid.NewGuid()}/complete", content: null);

    await AssertProblemAsync(response, HttpStatusCode.NotFound, "hunt_not_found");
  }

  [Fact]
  public async Task CompleteHunt_Should_Return_HuntNotFound_When_Hunt_Belongs_To_Another_Character()
  {
    var ownerClient = await CreateAuthenticatedClientAsync("HuntOwner");
    var startedHunt = await StartHuntAsync(ownerClient);
    var otherClient = await CreateAuthenticatedClientAsync("OtherChar");

    var response = await otherClient.PostAsync($"api/hunts/{startedHunt.HuntId}/complete", content: null);

    await AssertProblemAsync(response, HttpStatusCode.NotFound, "hunt_not_found");
  }

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

    var persistedHunt = await GetHuntAsync(startedHunt.HuntId);

    Assert.Equal(HuntStatus.Completed, persistedHunt.Status);
    Assert.Equal(startedHunt.EndsAtUtc, persistedHunt.CompletedAtUtc);
    Assert.Equal(1, persistedHunt.Version);

    var character = await GetCurrentCharacterAsync(client);

    Assert.Equal(25, character.Experience);
    Assert.Equal(15, character.Gold);
    Assert.Equal(1, character.Level);

    var activeHuntResponse = await client.GetAsync("api/hunts/active");

    Assert.Equal(HttpStatusCode.OK, activeHuntResponse.StatusCode);

    var activeHunt = await activeHuntResponse.Content.ReadFromJsonAsync<GetActiveHuntResponse>();

    Assert.NotNull(activeHunt);
    Assert.Null(activeHunt!.ActiveHunt);
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

    await AssertProblemAsync(response, HttpStatusCode.Conflict, "hunt_not_finished_yet");
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

  private async Task<Hunt> GetHuntAsync(Guid huntId)
  {
    using var scope = _factory.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<BloodlineDbContext>();

    return await dbContext.Hunts
      .AsNoTracking()
      .SingleAsync(hunt => hunt.Id == huntId);
  }

  private static async Task AssertProblemAsync(
    HttpResponseMessage response,
    HttpStatusCode expectedStatusCode,
    string expectedCode
  )
  {
    Assert.Equal(expectedStatusCode, response.StatusCode);

    var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>();

    Assert.NotNull(problem);
    Assert.Equal(expectedCode, problem!.Code);
  }

  private sealed record ProblemDetailsResponse(string? Code);
}