using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WesNews.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDigestEnabledToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DigestEnabled",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DigestEnabled",
                table: "Users");
        }
    }
}
