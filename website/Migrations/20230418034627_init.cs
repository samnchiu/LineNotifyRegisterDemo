using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace website.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LineUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    sub = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    email = table.Column<string>(type: "TEXT", nullable: true),
                    IdToken = table.Column<string>(type: "TEXT", nullable: true),
                    AccessToken = table.Column<string>(type: "TEXT", nullable: true),
                    registed = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SendLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    sender = table.Column<string>(type: "TEXT", nullable: true),
                    receiver = table.Column<string>(type: "TEXT", nullable: true),
                    message = table.Column<string>(type: "TEXT", nullable: true),
                    status = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SendLogs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LineUsers");

            migrationBuilder.DropTable(
                name: "SendLogs");
        }
    }
}
