using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTimestamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<byte[]>(
                name: "CreatedAt",
                table: "Rooms",
                type: "bytea",
                rowVersion: true,
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "bytea");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "CreatedAt",
                table: "Rooms",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldRowVersion: true,
                oldNullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
