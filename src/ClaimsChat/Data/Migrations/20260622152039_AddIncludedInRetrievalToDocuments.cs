using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClaimsChat.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIncludedInRetrievalToDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IncludedInRetrieval",
                table: "Documents",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IncludedInRetrieval",
                table: "Documents");
        }
    }
}
