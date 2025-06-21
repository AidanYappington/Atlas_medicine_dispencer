using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicineDispencer.Migrations
{
    /// <inheritdoc />
    public partial class AddCompartmentNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompartmentNumber",
                table: "Compartments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompartmentNumber",
                table: "Compartments");
        }
    }
}
