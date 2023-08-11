using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Persistence.Migrations
{
    public partial class update_table_01 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Servers_Id",
                table: "Servers",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServerLogs_Id",
                table: "ServerLogs",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Groups_Id",
                table: "Groups",
                column: "Id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Servers_Id",
                table: "Servers");

            migrationBuilder.DropIndex(
                name: "IX_ServerLogs_Id",
                table: "ServerLogs");

            migrationBuilder.DropIndex(
                name: "IX_Groups_Id",
                table: "Groups");
        }
    }
}
