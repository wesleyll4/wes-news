using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WesNews.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsFeatured : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FeaturedAt",
                table: "NewsArticles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "NewsArticles",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeaturedAt",
                table: "NewsArticles");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "NewsArticles");
        }
    }
}
