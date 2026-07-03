using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetHealthAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesToPet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
        name: "Breed",
        table: "Pets",
        type: "nvarchar(200)",
        maxLength: 200,
        nullable: false,
        oldClrType: typeof(string),
        oldType: "nvarchar(max)");
        
    migrationBuilder.CreateIndex(
        name: "IX_Pets_Name",
        table: "Pets",
        column: "Name");

    migrationBuilder.CreateIndex(
        name: "IX_Pets_Breed",
        table: "Pets",
        column: "Breed");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
