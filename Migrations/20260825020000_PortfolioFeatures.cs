// Adds the portfolio-ready employee, attendance, security, audit and access-request schema.
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SecureEmployeePortal.Data;

#nullable disable

namespace SecureEmployeePortal.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260825020000_PortfolioFeatures")]
public partial class PortfolioFeatures : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(name: "AccountStatus", table: "AspNetUsers", type: "int", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<DateTimeOffset>(name: "CreatedAt", table: "AspNetUsers", type: "datetimeoffset", nullable: false, defaultValueSql: "SYSUTCDATETIME()" );
        migrationBuilder.AddColumn<string>(name: "Department", table: "AspNetUsers", type: "nvarchar(120)", maxLength: 120, nullable: false, defaultValue: "Operations");
        migrationBuilder.AddColumn<string>(name: "EmployeeNumber", table: "AspNetUsers", type: "nvarchar(32)", maxLength: 32, nullable: false, defaultValue: "");
        migrationBuilder.AddColumn<string>(name: "JobTitle", table: "AspNetUsers", type: "nvarchar(120)", maxLength: 120, nullable: false, defaultValue: "Employee");
        migrationBuilder.AddColumn<DateTimeOffset>(name: "LastLoginAt", table: "AspNetUsers", type: "datetimeoffset", nullable: true);
        migrationBuilder.AddColumn<DateTimeOffset>(name: "LastPasswordChangeAt", table: "AspNetUsers", type: "datetimeoffset", nullable: true);

        migrationBuilder.AlterColumn<string>(name: "FirstName", table: "AspNetUsers", type: "nvarchar(100)", maxLength: 100, nullable: false, oldClrType: typeof(string), oldType: "nvarchar(max)");
        migrationBuilder.AlterColumn<string>(name: "LastName", table: "AspNetUsers", type: "nvarchar(100)", maxLength: 100, nullable: false, oldClrType: typeof(string), oldType: "nvarchar(max)");

        migrationBuilder.CreateTable(
            name: "AccessRequests",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                RequesterUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                RequestedResource = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                ReviewerUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                DecisionNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                RequestedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                ReviewedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_AccessRequests", x => x.Id));

        migrationBuilder.CreateTable(
            name: "AttendanceRecords",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ClockInAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                ClockOutAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                TotalMinutes = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_AttendanceRecords", x => x.Id));

        migrationBuilder.CreateTable(
            name: "SecurityEvents",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                EventType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                Succeeded = table.Column<bool>(type: "bit", nullable: false),
                Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_SecurityEvents", x => x.Id));

        migrationBuilder.CreateTable(
            name: "AuditLogs",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                ActorUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                TargetUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                ActionType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                PreviousValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                NewValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                Succeeded = table.Column<bool>(type: "bit", nullable: false),
                OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_AuditLogs", x => x.Id));

        migrationBuilder.CreateIndex(name: "IX_AccessRequests_RequesterUserId", table: "AccessRequests", column: "RequesterUserId");
        migrationBuilder.CreateIndex(name: "IX_AccessRequests_Status", table: "AccessRequests", column: "Status");
        migrationBuilder.CreateIndex(name: "IX_AttendanceRecords_UserId_ClockInAt", table: "AttendanceRecords", columns: new[] { "UserId", "ClockInAt" });
        migrationBuilder.CreateIndex(name: "IX_AuditLogs_ActorUserId", table: "AuditLogs", column: "ActorUserId");
        migrationBuilder.CreateIndex(name: "IX_AuditLogs_OccurredAt", table: "AuditLogs", column: "OccurredAt");
        migrationBuilder.CreateIndex(name: "IX_SecurityEvents_OccurredAt", table: "SecurityEvents", column: "OccurredAt");
        migrationBuilder.CreateIndex(name: "IX_SecurityEvents_UserId", table: "SecurityEvents", column: "UserId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "AccessRequests");
        migrationBuilder.DropTable(name: "AttendanceRecords");
        migrationBuilder.DropTable(name: "AuditLogs");
        migrationBuilder.DropTable(name: "SecurityEvents");

        migrationBuilder.DropColumn(name: "AccountStatus", table: "AspNetUsers");
        migrationBuilder.DropColumn(name: "CreatedAt", table: "AspNetUsers");
        migrationBuilder.DropColumn(name: "Department", table: "AspNetUsers");
        migrationBuilder.DropColumn(name: "EmployeeNumber", table: "AspNetUsers");
        migrationBuilder.DropColumn(name: "JobTitle", table: "AspNetUsers");
        migrationBuilder.DropColumn(name: "LastLoginAt", table: "AspNetUsers");
        migrationBuilder.DropColumn(name: "LastPasswordChangeAt", table: "AspNetUsers");

        migrationBuilder.AlterColumn<string>(name: "FirstName", table: "AspNetUsers", type: "nvarchar(max)", nullable: false, oldClrType: typeof(string), oldType: "nvarchar(100)", oldMaxLength: 100);
        migrationBuilder.AlterColumn<string>(name: "LastName", table: "AspNetUsers", type: "nvarchar(max)", nullable: false, oldClrType: typeof(string), oldType: "nvarchar(100)", oldMaxLength: 100);
    }
}
