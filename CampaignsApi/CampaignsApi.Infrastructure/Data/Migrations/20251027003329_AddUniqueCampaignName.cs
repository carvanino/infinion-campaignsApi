using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampaignsApi.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueCampaignName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_Name_Unique",
                table: "Campaigns",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Campaigns_Name_Unique",
                table: "Campaigns");
        }
    }
}
