using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class ChangesGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_AspNetUsers_PlayerTwoId",
                table: "Rooms");

            migrationBuilder.AlterColumn<long>(
                name: "PlayerTwoId",
                table: "Rooms",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<string>(
                name: "Guid",
                table: "Rooms",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_AspNetUsers_PlayerTwoId",
                table: "Rooms",
                column: "PlayerTwoId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_AspNetUsers_PlayerTwoId",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "Guid",
                table: "Rooms");

            migrationBuilder.AlterColumn<long>(
                name: "PlayerTwoId",
                table: "Rooms",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_AspNetUsers_PlayerTwoId",
                table: "Rooms",
                column: "PlayerTwoId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
