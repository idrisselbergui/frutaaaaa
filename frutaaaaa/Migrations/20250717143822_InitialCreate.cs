using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace frutaaaaa.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DailyPrograms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NumProg = table.Column<int>(type: "int", nullable: false),
                    Coddes = table.Column<int>(type: "int", nullable: false),
                    Refexp = table.Column<int>(type: "int", nullable: false),
                    PO = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Havday = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Dteprog = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Lot = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyPrograms", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "destination",
                columns: table => new
                {
                    coddes = table.Column<int>(type: "int", nullable: false),
                    vildes = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "grpvar",
                columns: table => new
                {
                    codgrv = table.Column<int>(type: "int", nullable: false),
                    nomgrv = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "partenaire",
                columns: table => new
                {
                    @ref = table.Column<int>(name: "ref", type: "int", nullable: false),
                    nom = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tpalette",
                columns: table => new
                {
                    codtyp = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nomemb = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Username = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Password = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Permission = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DailyProgramDetails",
                columns: table => new
                {
                    NumProg = table.Column<int>(type: "int", nullable: false),
                    codgrv = table.Column<int>(type: "int", nullable: false),
                    codtyp = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nbrpal = table.Column<int>(type: "int", nullable: false),
                    Nbrcoli = table.Column<int>(type: "int", nullable: false),
                    Valide = table.Column<int>(type: "int", nullable: false),
                    DailyProgramId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyProgramDetails", x => new { x.NumProg, x.codgrv });
                    table.ForeignKey(
                        name: "FK_DailyProgramDetails_DailyPrograms_DailyProgramId",
                        column: x => x.DailyProgramId,
                        principalTable: "DailyPrograms",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_DailyProgramDetails_DailyProgramId",
                table: "DailyProgramDetails",
                column: "DailyProgramId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyProgramDetails");

            migrationBuilder.DropTable(
                name: "destination");

            migrationBuilder.DropTable(
                name: "grpvar");

            migrationBuilder.DropTable(
                name: "partenaire");

            migrationBuilder.DropTable(
                name: "tpalette");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "DailyPrograms");
        }
    }
}
