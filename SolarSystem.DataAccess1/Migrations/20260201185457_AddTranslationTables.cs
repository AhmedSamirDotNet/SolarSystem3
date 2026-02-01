using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SolarSystem.DataAccess1.Migrations
{
    /// <inheritdoc />
    public partial class AddTranslationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Sections");

            migrationBuilder.DropColumn(
                name: "MainDesc",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SubDesc",
                table: "Products");

            migrationBuilder.CreateTable(
                name: "ProductTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LanguageCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MainDesc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubDesc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductTranslations_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SectionTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LanguageCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SectionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectionTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SectionTranslations_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ProductTranslations",
                columns: new[] { "Id", "LanguageCode", "MainDesc", "Name", "ProductId", "SubDesc" },
                values: new object[,]
                {
                    { 1, "en", "High efficiency panel", "Super Solar 3000", 1, null },
                    { 2, "en", "Pure sine wave inverter", "MaxVolt Inverter", 2, null },
                    { 3, "ar", "لوح عالي الكفاءة", "سوبر سولار 3000", 1, null },
                    { 4, "ar", "محول موجة جيبية نقية", "ماكس فولت إنفرتر", 2, null }
                });

            migrationBuilder.InsertData(
                table: "SectionTranslations",
                columns: new[] { "Id", "LanguageCode", "Name", "SectionId" },
                values: new object[,]
                {
                    { 1, "en", "Solar Panels", 1 },
                    { 2, "en", "Inverters", 2 },
                    { 3, "ar", "الألواح الشمسية", 1 },
                    { 4, "ar", "المحولات", 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductTranslations_ProductId",
                table: "ProductTranslations",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SectionTranslations_SectionId",
                table: "SectionTranslations",
                column: "SectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductTranslations");

            migrationBuilder.DropTable(
                name: "SectionTranslations");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Sections",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MainDesc",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SubDesc",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "MainDesc", "Name", "SubDesc" },
                values: new object[] { "High efficiency panel", "Super Solar 3000", null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "MainDesc", "Name", "SubDesc" },
                values: new object[] { "Pure sine wave inverter", "MaxVolt Inverter", null });

            migrationBuilder.UpdateData(
                table: "Sections",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Solar Panels");

            migrationBuilder.UpdateData(
                table: "Sections",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Inverters");
        }
    }
}
