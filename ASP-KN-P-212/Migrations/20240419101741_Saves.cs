using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASP_KN_P_212.Migrations
{
    /// <inheritdoc />
    public partial class Saves : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "Rooms",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "Rooms");
        }
    }
}
