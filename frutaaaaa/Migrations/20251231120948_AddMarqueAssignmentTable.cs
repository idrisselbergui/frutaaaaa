using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace frutaaaaa.Migrations
{
    /// <inheritdoc />
    public partial class AddMarqueAssignmentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "codvar",
                table: "traitement",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "unité",
                table: "trait",
                type: "longtext",
                nullable: true,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "nomcom",
                table: "trait",
                type: "longtext",
                nullable: true,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "matieractive",
                table: "trait",
                type: "longtext",
                nullable: true,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "nomemb",
                table: "tpalette",
                type: "varchar(40)",
                maxLength: 40,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "codtyp",
                table: "tpalette",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "codbarq",
                table: "tpalette",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "codebar",
                table: "tpalette",
                type: "varchar(25)",
                maxLength: 25,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "codgrp",
                table: "tpalette",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "codmar",
                table: "tpalette",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "codvar",
                table: "tpalette",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "indice",
                table: "tpalette",
                type: "varchar(1)",
                maxLength: 1,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "lier",
                table: "tpalette",
                type: "varchar(1)",
                maxLength: 1,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "modtrp",
                table: "tpalette",
                type: "varchar(2)",
                maxLength: 2,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "natpal",
                table: "tpalette",
                type: "varchar(2)",
                maxLength: 2,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "nbrbar",
                table: "tpalette",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "nbrfil",
                table: "tpalette",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "pdcomc",
                table: "tpalette",
                type: "decimal(16,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "pdnetc",
                table: "tpalette",
                type: "decimal(16,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "pdproc",
                table: "tpalette",
                type: "decimal(16,2)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "pdsbar",
                table: "tpalette",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "pdsfil",
                table: "tpalette",
                type: "decimal(16,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "prxcon",
                table: "tpalette",
                type: "decimal(16,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "prxemb",
                table: "tpalette",
                type: "decimal(16,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "prxpal",
                table: "tpalette",
                type: "decimal(16,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "tarcol",
                table: "tpalette",
                type: "decimal(16,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "tarpal",
                table: "tpalette",
                type: "decimal(16,2)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "numpal",
                table: "ecart_e",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<int>(
                name: "numvent",
                table: "ecart_e",
                type: "INT",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "pdsvent",
                table: "ecart_e",
                type: "DOUBLE",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ecart_e",
                table: "ecart_e",
                column: "numpal");

            migrationBuilder.CreateTable(
                name: "defaut",
                columns: table => new
                {
                    coddef = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    intdef = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    famdef = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_defaut", x => x.coddef);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ecart_direct",
                columns: table => new
                {
                    Numpal = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Refver = table.Column<int>(type: "int", nullable: true),
                    Codvar = table.Column<int>(type: "int", nullable: true),
                    Dtepal = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Numbl = table.Column<int>(type: "int", nullable: true),
                    Pdsfru = table.Column<double>(type: "DOUBLE", nullable: true),
                    Codtype = table.Column<int>(type: "int", nullable: true),
                    Numvent = table.Column<int>(type: "INT", nullable: true),
                    Pdsvent = table.Column<double>(type: "DOUBLE", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ecart_direct", x => x.Numpal);
                    table.ForeignKey(
                        name: "FK_ecart_direct_typeecart_Codtype",
                        column: x => x.Codtype,
                        principalTable: "typeecart",
                        principalColumn: "codtype",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "marque",
                columns: table => new
                {
                    codmar = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    desmar = table.Column<string>(type: "longtext", nullable: false, collation: "latin1_swedish_ci"),
                    lier = table.Column<string>(type: "longtext", nullable: true, collation: "latin1_swedish_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_marque", x => x.codmar);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "marque_assignment",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    codmar = table.Column<short>(type: "smallint", nullable: false),
                    refver = table.Column<int>(type: "int", nullable: false),
                    codvar = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_marque_assignment", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "reception",
                columns: table => new
                {
                    numrec = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    refsta = table.Column<short>(type: "smallint", nullable: true),
                    codvar = table.Column<short>(type: "smallint", nullable: true),
                    varrec = table.Column<short>(type: "smallint", nullable: true),
                    refver = table.Column<short>(type: "smallint", nullable: true),
                    numveh = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    numcai = table.Column<int>(type: "int", nullable: true),
                    numtrs = table.Column<int>(type: "int", nullable: true),
                    numbl = table.Column<int>(type: "int", nullable: true),
                    dterec = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    herrec = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dtecue = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    hercue = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    typcai = table.Column<short>(type: "smallint", nullable: true),
                    codtyp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nbrcai = table.Column<int>(type: "int", nullable: true),
                    nbrpal = table.Column<int>(type: "int", nullable: true),
                    pdspes = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    brurec = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    tarrec = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    netrec = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    moycai = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    coment = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    certif = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    codmaj = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    numbou = table.Column<uint>(type: "int unsigned", nullable: true),
                    codaff = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    pdsech = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    nbrfru = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    tmprec = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    numr = table.Column<uint>(type: "int unsigned", nullable: true),
                    numpes = table.Column<int>(type: "int", nullable: true),
                    numord = table.Column<int>(type: "int", nullable: true),
                    numcaisor = table.Column<int>(type: "int", nullable: true),
                    nbrcailiv = table.Column<int>(type: "int", nullable: true),
                    nbrpalliv = table.Column<int>(type: "int", nullable: true),
                    codtrj = table.Column<int>(type: "int", nullable: true),
                    listpar = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    comagri = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    affect = table.Column<short>(type: "smallint", nullable: true),
                    typetrns = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mttrans = table.Column<double>(type: "double", nullable: true),
                    dttare = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    pese = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    pdsNet = table.Column<double>(type: "double", nullable: true),
                    numtrait = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reception", x => x.numrec);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_page_permissions",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false),
                    page_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    allowed = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_page_permissions", x => new { x.user_id, x.page_name });
                    table.ForeignKey(
                        name: "FK_user_page_permissions_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "vente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Numbonvente = table.Column<int>(type: "int", nullable: true),
                    date_vente = table.Column<DateTime>(type: "DATE", nullable: false),
                    Price = table.Column<double>(type: "DOUBLE", nullable: false),
                    poids_total = table.Column<double>(type: "DOUBLE", nullable: false),
                    montant_total = table.Column<double>(type: "DOUBLE", nullable: false),
                    numlot = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "DATETIME", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vente", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ecart_direct_Codtype",
                table: "ecart_direct",
                column: "Codtype");

            migrationBuilder.CreateIndex(
                name: "IX_marque_assignment_codmar_refver_codvar",
                table: "marque_assignment",
                columns: new[] { "codmar", "refver", "codvar" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "defaut");

            migrationBuilder.DropTable(
                name: "ecart_direct");

            migrationBuilder.DropTable(
                name: "marque");

            migrationBuilder.DropTable(
                name: "marque_assignment");

            migrationBuilder.DropTable(
                name: "reception");

            migrationBuilder.DropTable(
                name: "user_page_permissions");

            migrationBuilder.DropTable(
                name: "vente");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ecart_e",
                table: "ecart_e");

            migrationBuilder.DropColumn(
                name: "codvar",
                table: "traitement");

            migrationBuilder.DropColumn(
                name: "codbarq",
                table: "tpalette");

            migrationBuilder.DropColumn(
                name: "codebar",
                table: "tpalette");

            migrationBuilder.DropColumn(
                name: "codgrp",
                table: "tpalette");

            migrationBuilder.DropColumn(
                name: "codmar",
                table: "tpalette");

            migrationBuilder.DropColumn(
                name: "codvar",
                table: "tpalette");

            migrationBuilder.DropColumn(
                name: "indice",
                table: "tpalette");

            migrationBuilder.DropColumn(
                name: "lier",
                table: "tpalette");

            migrationBuilder.DropColumn(
                name: "modtrp",
                table: "tpalette");

            migrationBuilder.DropColumn(
                name: "natpal",
                table: "tpalette");

            migrationBuilder.DropColumn(
                name: "nbrbar",
                table: "tpalette");

            migrationBuilder.DropColumn(
                name: "nbrfil",
                table: "tpalette");

            migrationBuilder.DropColumn(
                name: "pdcomc",
                table: "tpalette");

            migrationBuilder.DropColumn(
                name: "pdnetc",
                table: "tpalette");

            migrationBuilder.DropColumn(
                name: "pdproc",
                table: "tpalette");

            migrationBuilder.DropColumn(
                name: "pdsbar",
                table: "tpalette");

            migrationBuilder.DropColumn(
                name: "pdsfil",
                table: "tpalette");

            migrationBuilder.DropColumn(
                name: "prxcon",
                table: "tpalette");

            migrationBuilder.DropColumn(
                name: "prxemb",
                table: "tpalette");

            migrationBuilder.DropColumn(
                name: "prxpal",
                table: "tpalette");

            migrationBuilder.DropColumn(
                name: "tarcol",
                table: "tpalette");

            migrationBuilder.DropColumn(
                name: "tarpal",
                table: "tpalette");

            migrationBuilder.DropColumn(
                name: "numvent",
                table: "ecart_e");

            migrationBuilder.DropColumn(
                name: "pdsvent",
                table: "ecart_e");

            migrationBuilder.AlterColumn<string>(
                name: "unité",
                table: "trait",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true,
                oldCollation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "nomcom",
                table: "trait",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true,
                oldCollation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "matieractive",
                table: "trait",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true,
                oldCollation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "tpalette",
                keyColumn: "nomemb",
                keyValue: null,
                column: "nomemb",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "nomemb",
                table: "tpalette",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(40)",
                oldMaxLength: 40,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "tpalette",
                keyColumn: "codtyp",
                keyValue: null,
                column: "codtyp",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "codtyp",
                table: "tpalette",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "numpal",
                table: "ecart_e",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);
        }
    }
}
