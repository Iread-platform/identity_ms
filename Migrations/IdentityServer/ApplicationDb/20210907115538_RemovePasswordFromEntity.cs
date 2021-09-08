using Microsoft.EntityFrameworkCore.Migrations;

namespace iread_identity_ms.Migrations.IdentityServer.ApplicationDb
{
    public partial class RemovePasswordFromEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                table: "AspNetUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "AspNetUsers",
                type: "text",
                nullable: false);
        }
    }
}
