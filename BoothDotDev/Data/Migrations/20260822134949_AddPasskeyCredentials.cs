using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPasskeyCredentials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "passkey_credential",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    aa_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    credential_id = table.Column<byte[]>(type: "bytea", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_used_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    nickname = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    public_key = table.Column<byte[]>(type: "bytea", nullable: false),
                    signature_counter = table.Column<long>(type: "bigint", nullable: false),
                    transports = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_passkey_credential", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_passkey_credential_credential_id",
                schema: "public",
                table: "passkey_credential",
                column: "credential_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_passkey_credential_user_id",
                schema: "public",
                table: "passkey_credential",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "passkey_credential",
                schema: "public");
        }
    }
}
