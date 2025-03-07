using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CSAT_BMTT.Migrations
{
    /// <inheritdoc />
    public partial class CitizenIdentificationNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CitizenIdentificationNumber",
                table: "User",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CitizenIdentificationNumber",
                table: "User");
        }
    }
}
