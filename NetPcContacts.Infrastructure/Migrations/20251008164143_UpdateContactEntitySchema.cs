using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetPcContacts.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateContactEntitySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Password",
                table: "Contacts",
                newName: "PasswordHash");

            migrationBuilder.AddColumn<string>(
                name: "CustomSubcategory",
                table: "Contacts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_Email",
                table: "Contacts",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Contacts_Email",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "CustomSubcategory",
                table: "Contacts");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "Contacts",
                newName: "Password");
        }
    }
}
