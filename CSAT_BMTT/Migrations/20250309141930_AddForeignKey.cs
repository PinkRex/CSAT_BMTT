using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CSAT_BMTT.Migrations
{
    /// <inheritdoc />
    public partial class AddForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AccessPermission_RequestorID",
                table: "AccessPermission",
                column: "RequestorID");

            migrationBuilder.CreateIndex(
                name: "IX_AccessPermission_TargetId",
                table: "AccessPermission",
                column: "TargetId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessPermission_User_RequestorID",
                table: "AccessPermission",
                column: "RequestorID",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AccessPermission_User_TargetId",
                table: "AccessPermission",
                column: "TargetId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessPermission_User_RequestorID",
                table: "AccessPermission");

            migrationBuilder.DropForeignKey(
                name: "FK_AccessPermission_User_TargetId",
                table: "AccessPermission");

            migrationBuilder.DropIndex(
                name: "IX_AccessPermission_RequestorID",
                table: "AccessPermission");

            migrationBuilder.DropIndex(
                name: "IX_AccessPermission_TargetId",
                table: "AccessPermission");
        }
    }
}
