using Microsoft.EntityFrameworkCore.Migrations;

namespace iread_identity_ms.Migrations.IdentityServer.ApplicationDb
{
    public partial class custom_photo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CustomPhoto",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomPhoto",
                table: "AspNetUsers");
        }
    }
}
