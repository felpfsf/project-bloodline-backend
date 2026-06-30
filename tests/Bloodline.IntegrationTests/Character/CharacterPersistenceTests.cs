using Bloodline.Infrastructure.Persistence;
using Bloodline.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DomainCharacter = Bloodline.Domain.Character.Character;
using Faction = Bloodline.Domain.Character.Faction;

namespace Bloodline.IntegrationTests.Character;

[Collection(PostgreSqlCollection.Name)]
public sealed class CharacterPersistenceTests(PostgreSqlFixture fixture)
{
  [Fact]
  public async Task SaveChanges_Should_Persist_Character()
  {
    // Arrange
    using var scope = fixture.Factory.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<BloodlineDbContext>();

    var character = DomainCharacter.Create(
        userId: Guid.NewGuid(),
        name: "Draven",
        faction: Faction.Vampire,
        createdAtUtc: DateTime.UtcNow);

    dbContext.Characters.Add(character);

    // Act
    await dbContext.SaveChangesAsync();

    // Assert
    var savedCharacter = await dbContext.Characters
        .AsNoTracking()
        .SingleAsync(x => x.Id == character.Id);

    savedCharacter.Name.Should().Be("Draven");
    savedCharacter.Faction.Should().Be(Faction.Vampire);
  }
}
