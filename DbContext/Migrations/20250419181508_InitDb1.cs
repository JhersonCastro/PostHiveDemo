using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DbContext.Migrations
{
    /// <inheritdoc />
    public partial class InitDb1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportReasons",
                columns: table => new
                {
                    ReportReasonId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportReason = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportReasons", x => x.ReportReasonId);
                });

            migrationBuilder.CreateTable(
                name: "REPORT",
                columns: table => new
                {
                    ReportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    ReportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReportReasonId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REPORT", x => x.ReportId);
                    table.ForeignKey(
                        name: "FK_REPORT_POST_PostId",
                        column: x => x.PostId,
                        principalTable: "POST",
                        principalColumn: "PostId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_REPORT_ReportReasons_ReportReasonId",
                        column: x => x.ReportReasonId,
                        principalTable: "ReportReasons",
                        principalColumn: "ReportReasonId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BAN",
                columns: table => new
                {
                    BanId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AdminReason = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BanDuration = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReportId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BAN", x => x.BanId);
                    table.ForeignKey(
                        name: "FK_BAN_REPORT_ReportId",
                        column: x => x.ReportId,
                        principalTable: "REPORT",
                        principalColumn: "ReportId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BAN_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RELATIONSHIP_RelationshipUserIdA",
                table: "RELATIONSHIP",
                column: "RelationshipUserIdA");

            migrationBuilder.CreateIndex(
                name: "IX_RELATIONSHIP_UserId",
                table: "RELATIONSHIP",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BAN_ReportId",
                table: "BAN",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_BAN_UserId",
                table: "BAN",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_REPORT_PostId",
                table: "REPORT",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_REPORT_ReportReasonId",
                table: "REPORT",
                column: "ReportReasonId");

            migrationBuilder.AddForeignKey(
                name: "FK_RELATIONSHIP_users_RelationshipUserIdA",
                table: "RELATIONSHIP",
                column: "RelationshipUserIdA",
                principalTable: "users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RELATIONSHIP_users_UserId",
                table: "RELATIONSHIP",
                column: "UserId",
                principalTable: "users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RELATIONSHIP_users_RelationshipUserIdA",
                table: "RELATIONSHIP");

            migrationBuilder.DropForeignKey(
                name: "FK_RELATIONSHIP_users_UserId",
                table: "RELATIONSHIP");

            migrationBuilder.DropTable(
                name: "BAN");

            migrationBuilder.DropTable(
                name: "REPORT");

            migrationBuilder.DropTable(
                name: "ReportReasons");

            migrationBuilder.DropIndex(
                name: "IX_RELATIONSHIP_RelationshipUserIdA",
                table: "RELATIONSHIP");

            migrationBuilder.DropIndex(
                name: "IX_RELATIONSHIP_UserId",
                table: "RELATIONSHIP");
        }
    }
}
