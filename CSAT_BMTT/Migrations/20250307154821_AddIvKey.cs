using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CSAT_BMTT.Migrations
{
    /// <inheritdoc />
    public partial class AddIvKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IvKey",
                table: "User",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IvKey",
                table: "User");
        }
    }
}
