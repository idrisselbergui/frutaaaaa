using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace frutaaaaa.Migrations
{
    /// <inheritdoc />
    public partial class AddDashboardTables : Migration
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
                name: "palbrut",
                columns: table => new
                {
                    numpal = table.Column<int>(type: "int", nullable: false),
                    numrec = table.Column<int>(type: "int", nullable: true),
                    dterec = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    refadh = table.Column<int>(type: "int", nullable: true),
                    refver = table.Column<int>(type: "int", nullable: true),
                    codvar = table.Column<int>(type: "int", nullable: true),
                    nbrcai = table.Column<int>(type: "int", nullable: true),
                    pdsfru = table.Column<double>(type: "double", nullable: true),
                    etat = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    codsvar = table.Column<int>(type: "int", nullable: true),
                    codtyp = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    caiver = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "palette",
                columns: table => new
                {
                    numpal = table.Column<int>(type: "int", nullable: false),
                    dtepal = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    colpal = table.Column<int>(type: "int", nullable: true),
                    pdscom = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    pdsfru = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    codtyp = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    numbdq = table.Column<int>(type: "int", nullable: true),
                    codmar = table.Column<int>(type: "int", nullable: true),
                    codvar = table.Column<int>(type: "int", nullable: true),
                    codgrp = table.Column<int>(type: "int", nullable: true),
                    codexp = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "palette_d",
                columns: table => new
                {
                    numpal = table.Column<int>(type: "int", nullable: true),
                    nbrcol = table.Column<int>(type: "int", nullable: true),
                    nbrfru = table.Column<int>(type: "int", nullable: true),
                    refver = table.Column<int>(type: "int", nullable: true),
                    pdscom = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    pdsfru = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    numbdq = table.Column<int>(type: "int", nullable: true),
                    codvar = table.Column<int>(type: "int", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "verger",
                columns: table => new
                {
                    refver = table.Column<int>(type: "int", nullable: false),
                    refadh = table.Column<int>(type: "int", nullable: true),
                    nomver = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
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
                name: "palbrut");

            migrationBuilder.DropTable(
                name: "palette");

            migrationBuilder.DropTable(
                name: "palette_d");

            migrationBuilder.DropTable(
                name: "partenaire");

            migrationBuilder.DropTable(
                name: "tpalette");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "variete");

            migrationBuilder.DropTable(
                name: "verger");

            migrationBuilder.DropTable(
                name: "DailyPrograms");
        }
    }
}
