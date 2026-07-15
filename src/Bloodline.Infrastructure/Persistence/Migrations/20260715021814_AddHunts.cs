using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bloodline.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHunts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hunts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    character_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    starts_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ends_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    energy_cost = table.Column<int>(type: "integer", nullable: false),
                    experience_reward = table.Column<long>(type: "bigint", nullable: false),
                    gold_reward = table.Column<long>(type: "bigint", nullable: false),
                    definition_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hunts", x => x.id);
                    table.ForeignKey(
                        name: "FK_hunts_characters_character_id",
                        column: x => x.character_id,
                        principalTable: "characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_hunts_character_id_status",
                table: "hunts",
                columns: new[] { "character_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ux_hunts_character_id_active",
                table: "hunts",
                column: "character_id",
                unique: true,
                filter: "\"status\" = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hunts");
        }
    }
}
