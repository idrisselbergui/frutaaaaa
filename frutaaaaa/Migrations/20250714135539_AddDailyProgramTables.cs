using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace frutaaaaa.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyProgramTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Exporter");

            migrationBuilder.RenameTable(
                name: "Destination",
                newName: "destination");

            migrationBuilder.RenameColumn(
                name: "Coddes",
                table: "destination",
                newName: "coddes");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "destination",
                newName: "vildes");

            migrationBuilder.CreateTable(
                name: "partenaire",
                columns: table => new
                {
                    @ref = table.Column<int>(name: "ref", type: "int", nullable: false),
                    nom = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "variete",
                columns: table => new
                {
                    codvar = table.Column<int>(type: "int", nullable: false),
                    nomvar = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "partenaire");

            migrationBuilder.DropTable(
                name: "variete");

            migrationBuilder.RenameTable(
                name: "destination",
                newName: "Destination");

            migrationBuilder.RenameColumn(
                name: "coddes",
                table: "Destination",
                newName: "Coddes");

            migrationBuilder.RenameColumn(
                name: "vildes",
                table: "Destination",
                newName: "Name");

            migrationBuilder.CreateTable(
                name: "Exporter",
                columns: table => new
                {
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Refexp = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
