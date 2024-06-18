using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MailBackend.Migrations
{
    /// <inheritdoc />
    public partial class postgresInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Kursevi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Sifra = table.Column<string>(type: "text", nullable: false),
                    PunoIme = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kursevi", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Studenti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "text", nullable: false),
                    SubToken = table.Column<string>(type: "text", nullable: true),
                    SubTokenExpires = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RegisteredDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UnsubToken = table.Column<string>(type: "text", nullable: false),
                    isVerified = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Studenti", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudentKursevi",
                columns: table => new
                {
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    KursId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentKursevi", x => new { x.StudentId, x.KursId });
                    table.ForeignKey(
                        name: "FK_StudentKursevi_Kursevi_KursId",
                        column: x => x.KursId,
                        principalTable: "Kursevi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentKursevi_Studenti_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Studenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Kursevi",
                columns: new[] { "Id", "PunoIme", "Sifra" },
                values: new object[,]
                {
                    { 1, "Baze podataka", "db" },
                    { 2, "Baze podataka 2", "db2" },
                    { 3, "Baze podataka 3", "db3" },
                    { 4, "Programski jezici", "pj" },
                    { 5, "Uvod u informacione sisteme", "uis" },
                    { 6, "Jezici i okruzenja za razvoj IS", "joris" },
                    { 7, "Modelovanje poslovnih procesa", "mpp" },
                    { 8, "Analiza i logicko projektovanje IS", "ailp" },
                    { 9, "Poslovni informacioni sistemi", "poslis" },
                    { 10, "Projektovanje informacionih sistema", "pis" },
                    { 11, "Strukture podataka i algoritmi", "spa" },
                    { 12, "Fizicko projektovanje informacionih sistema", "fpis" },
                    { 13, "Administracija baze podataka", "abp" },
                    { 14, "Integrisana softverska resenja", "isr" },
                    { 15, "Informacioni sistemi za upravljanje znanjem", "isuz" },
                    { 16, "ISiT Menadzment", "isitm" },
                    { 17, "Upravljanje razvojem informacionih sistema", "uris" },
                    { 18, "Programski prevodioci", "prev" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentKursevi_KursId",
                table: "StudentKursevi",
                column: "KursId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentKursevi");

            migrationBuilder.DropTable(
                name: "Kursevi");

            migrationBuilder.DropTable(
                name: "Studenti");
        }
    }
}
