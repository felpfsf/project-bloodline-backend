using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bloodline.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCharacter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "characters",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    faction = table.Column<short>(type: "smallint", nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    experience = table.Column<long>(type: "bigint", nullable: false),
                    gold = table.Column<long>(type: "bigint", nullable: false),
                    energy = table.Column<int>(type: "integer", nullable: false),
                    max_energy = table.Column<int>(type: "integer", nullable: false),
                    strength = table.Column<int>(type: "integer", nullable: false),
                    defense = table.Column<int>(type: "integer", nullable: false),
                    agility = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_characters", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_characters_normalized_name",
                table: "characters",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_characters_user_id",
                table: "characters",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "characters");
        }
    }
}
