using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Bloodline.Api.Features.Me;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Bloodline.Api.Tests.Features.Me;

public sealed class GetCurrentUserEndpointTests
{
  [Fact]
  public void Handle_ShouldReturnCurrentUser_WhenSubjectClaimIsPresent()
  {
    var userId = Guid.NewGuid();
    var user = CreateUser(new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()));

    var result = GetCurrentUserEndpoint.Handle(user);

    var ok = result.Should().BeOfType<Ok<GetCurrentUserResponse>>().Subject;
    ok.Value.Should().Be(new GetCurrentUserResponse(userId));
  }

  [Fact]
  public void Handle_ShouldReturnCurrentUser_WhenSubjectClaimWasMappedToNameIdentifier()
  {
    var userId = Guid.NewGuid();
    var user = CreateUser(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));

    var result = GetCurrentUserEndpoint.Handle(user);

    var ok = result.Should().BeOfType<Ok<GetCurrentUserResponse>>().Subject;
    ok.Value.Should().Be(new GetCurrentUserResponse(userId));
  }

  [Fact]
  public void Handle_ShouldReturnProblem_WhenSubjectClaimIsMissing()
  {
    var user = CreateUser();

    var result = GetCurrentUserEndpoint.Handle(user);

    var problem = result.Should().BeOfType<ProblemHttpResult>().Subject;
    problem.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
  }

  private static ClaimsPrincipal CreateUser(params Claim[] claims)
  {
    var identity = new ClaimsIdentity(claims, authenticationType: "Test");

    return new ClaimsPrincipal(identity);
  }
}
