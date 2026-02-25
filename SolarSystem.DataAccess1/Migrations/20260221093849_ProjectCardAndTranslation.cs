using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SolarSystem.DataAccess1.Migrations
{
    /// <inheritdoc />
    public partial class ProjectCardAndTranslation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectHomePageCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImageRelativePath = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectHomePageCards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectCardTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectCardId = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LocationText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectCardTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectCardTranslations_ProjectHomePageCards_ProjectCardId",
                        column: x => x.ProjectCardId,
                        principalTable: "ProjectHomePageCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ProjectHomePageCards",
                columns: new[] { "Id", "ImageRelativePath" },
                values: new object[,]
                {
                    { 1, "/images/projects/project1.jpg" },
                    { 2, "/images/projects/project2.jpg" }
                });

            migrationBuilder.InsertData(
                table: "ProjectCardTranslations",
                columns: new[] { "Id", "LanguageCode", "LocationText", "ProjectCardId", "Title" },
                values: new object[,]
                {
                    { 1, "en", "Cairo, Egypt", 1, "Residential Solar Install" },
                    { 2, "ar", "القاهرة، مصر", 1, "تركيب خلايا شمسية سكنية" },
                    { 3, "en", "Alexandria, Egypt", 2, "Commercial Solar Farm" },
                    { 4, "ar", "الإسكندرية، مصر", 2, "مزرعة طاقة شمسية تجارية" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectCardTranslations_ProjectCardId",
                table: "ProjectCardTranslations",
                column: "ProjectCardId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectCardTranslations");

            migrationBuilder.DropTable(
                name: "ProjectHomePageCards");
        }
    }
}
