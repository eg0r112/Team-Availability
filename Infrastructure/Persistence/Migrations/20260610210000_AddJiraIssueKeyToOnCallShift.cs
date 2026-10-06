using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TeamAvailability.Infrastructure.Persistence;

#nullable disable

namespace TeamAvailability.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260610210000_AddJiraIssueKeyToOnCallShift")]
public partial class AddJiraIssueKeyToOnCallShift : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "JiraIssueKey",
            table: "on_call_shifts",
            type: "character varying(32)",
            maxLength: 32,
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "JiraIssueKey",
            table: "on_call_shifts");
    }
}
