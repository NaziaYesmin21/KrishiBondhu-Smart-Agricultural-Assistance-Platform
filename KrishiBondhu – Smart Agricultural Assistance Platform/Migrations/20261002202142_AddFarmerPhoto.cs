using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KrishiBondhu___Smart_Agricultural_Assistance_Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddFarmerPhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoPath",
                table: "Farmers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoPath",
                table: "Farmers");
        }
    }
}
