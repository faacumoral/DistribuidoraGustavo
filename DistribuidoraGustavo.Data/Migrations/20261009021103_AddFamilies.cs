using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DistribuidoraGustavo.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFamilies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Families",
                columns: table => new
                {
                    FamilyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CodeSuffix = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Families", x => x.FamilyID);
                });

            migrationBuilder.CreateTable(
                name: "Products_Families",
                columns: table => new
                {
                    ProductFamilyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    FamilyID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products_Families", x => x.ProductFamilyID);
                    table.ForeignKey(
                        name: "FK_Products_Families_FamilyID",
                        column: x => x.FamilyID,
                        principalTable: "Families",
                        principalColumn: "FamilyID");
                    table.ForeignKey(
                        name: "FK_Products_Families_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID");
                });

            migrationBuilder.CreateIndex(
                name: "UQ_Families_CodeSuffix",
                table: "Families",
                column: "CodeSuffix",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Families_Name",
                table: "Families",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Families_FamilyID",
                table: "Products_Families",
                column: "FamilyID");

            migrationBuilder.CreateIndex(
                name: "UQ_Products_Families_ProductID",
                table: "Products_Families",
                column: "ProductID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products_Families");

            migrationBuilder.DropTable(
                name: "Families");
        }
    }
}
