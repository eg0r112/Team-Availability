using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TeamAvailability.Infrastructure.Persistence;

#nullable disable

namespace TeamAvailability.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260610213000_RemoveRowVersionFromOnCallShift")]
public partial class RemoveRowVersionFromOnCallShift : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "RowVersion",
            table: "on_call_shifts");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<byte[]>(
            name: "RowVersion",
            table: "on_call_shifts",
            type: "bytea",
            rowVersion: true,
            nullable: false,
            defaultValue: new byte[0]);
    }
}
