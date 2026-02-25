using Microsoft.EntityFrameworkCore.Migrations;
using SolarSystem.DataAccess1.Migrations;

#nullable disable

namespace SolarSystem.DataAccess1.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomersFeedBack : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerFeedBacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerFeedBacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerFeedBacks_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LanguageCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Job = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerTranslations_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerFeedbackTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LanguageCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FeedbackText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerFeedbackId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerFeedbackTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerFeedbackTranslations_CustomerFeedBacks_CustomerFeedbackId",
                        column: x => x.CustomerFeedbackId,
                        principalTable: "CustomerFeedBacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFeedBacks_CustomerId",
                table: "CustomerFeedBacks",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFeedbackTranslations_CustomerFeedbackId",
                table: "CustomerFeedbackTranslations",
                column: "CustomerFeedbackId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerTranslations_CustomerId",
                table: "CustomerTranslations",
                column: "CustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerFeedbackTranslations");

            migrationBuilder.DropTable(
                name: "CustomerTranslations");

            migrationBuilder.DropTable(
                name: "CustomerFeedBacks");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
