using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace frutaaaaa.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTraitAndTraitementTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "codgrp",
                table: "trait");

            migrationBuilder.AddColumn<int>(
                name: "codgrp",
                table: "traitement",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "dos",
                table: "trait",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "unité",
                table: "trait",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "codgrp",
                table: "traitement");

            migrationBuilder.DropColumn(
                name: "dos",
                table: "trait");

            migrationBuilder.DropColumn(
                name: "unité",
                table: "trait");

            migrationBuilder.AddColumn<int>(
                name: "codgrp",
                table: "trait",
                type: "int",
                nullable: true);
        }
    }
}
