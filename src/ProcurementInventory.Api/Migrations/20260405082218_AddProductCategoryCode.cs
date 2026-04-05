using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcurementInventory.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCategoryCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CategoryCode",
                table: "Products",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryCode",
                table: "Products");
        }
    }
}
