using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace soft.square.net.WebApi.Modules.ModuleName.Infrastructure.Data.Migrations.Oracle
{
    /// <inheritdoc />
    public partial class InitialNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "modulename_notes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    TenantId = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: false),
                    CreatedBy = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    Version = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_modulename_notes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_modulename_notes_TenantId",
                table: "modulename_notes",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "modulename_notes");
        }
    }
}
