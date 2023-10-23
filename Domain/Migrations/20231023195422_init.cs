using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessKeys_Accounts_CreatedById",
                table: "AccessKeys");

            migrationBuilder.DropForeignKey(
                name: "FK_AccessRoles_Accounts_CreatedById1",
                table: "AccessRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Accounts_CreatedById",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Accounts_CreatedById",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_DTorrents_Accounts_CreatedById",
                table: "DTorrents");

            migrationBuilder.DropForeignKey(
                name: "FK_Movies_Accounts_CreatedById",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "CreatedByAccount",
                table: "Accounts");

            migrationBuilder.RenameColumn(
                name: "CreatedById",
                table: "Movies",
                newName: "CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Movies_CreatedById",
                table: "Movies",
                newName: "IX_Movies_CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "CreatedById",
                table: "DTorrents",
                newName: "CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_DTorrents_CreatedById",
                table: "DTorrents",
                newName: "IX_DTorrents_CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "CreatedById",
                table: "Categories",
                newName: "CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Categories_CreatedById",
                table: "Categories",
                newName: "IX_Categories_CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "CreatedById",
                table: "Accounts",
                newName: "CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Accounts_CreatedById",
                table: "Accounts",
                newName: "IX_Accounts_CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "CreatedById1",
                table: "AccessRoles",
                newName: "CreatedByUserId1");

            migrationBuilder.RenameColumn(
                name: "CreatedById",
                table: "AccessRoles",
                newName: "CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_AccessRoles_CreatedById1",
                table: "AccessRoles",
                newName: "IX_AccessRoles_CreatedByUserId1");

            migrationBuilder.RenameColumn(
                name: "CreatedById",
                table: "AccessKeys",
                newName: "CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_AccessKeys_CreatedById",
                table: "AccessKeys",
                newName: "IX_AccessKeys_CreatedByUserId");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:postgres", ",,");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessKeys_Accounts_CreatedByUserId",
                table: "AccessKeys",
                column: "CreatedByUserId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AccessRoles_Accounts_CreatedByUserId1",
                table: "AccessRoles",
                column: "CreatedByUserId1",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Accounts_CreatedByUserId",
                table: "Accounts",
                column: "CreatedByUserId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Accounts_CreatedByUserId",
                table: "Categories",
                column: "CreatedByUserId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DTorrents_Accounts_CreatedByUserId",
                table: "DTorrents",
                column: "CreatedByUserId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Movies_Accounts_CreatedByUserId",
                table: "Movies",
                column: "CreatedByUserId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessKeys_Accounts_CreatedByUserId",
                table: "AccessKeys");

            migrationBuilder.DropForeignKey(
                name: "FK_AccessRoles_Accounts_CreatedByUserId1",
                table: "AccessRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Accounts_CreatedByUserId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Accounts_CreatedByUserId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_DTorrents_Accounts_CreatedByUserId",
                table: "DTorrents");

            migrationBuilder.DropForeignKey(
                name: "FK_Movies_Accounts_CreatedByUserId",
                table: "Movies");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Movies",
                newName: "CreatedById");

            migrationBuilder.RenameIndex(
                name: "IX_Movies_CreatedByUserId",
                table: "Movies",
                newName: "IX_Movies_CreatedById");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "DTorrents",
                newName: "CreatedById");

            migrationBuilder.RenameIndex(
                name: "IX_DTorrents_CreatedByUserId",
                table: "DTorrents",
                newName: "IX_DTorrents_CreatedById");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Categories",
                newName: "CreatedById");

            migrationBuilder.RenameIndex(
                name: "IX_Categories_CreatedByUserId",
                table: "Categories",
                newName: "IX_Categories_CreatedById");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Accounts",
                newName: "CreatedById");

            migrationBuilder.RenameIndex(
                name: "IX_Accounts_CreatedByUserId",
                table: "Accounts",
                newName: "IX_Accounts_CreatedById");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId1",
                table: "AccessRoles",
                newName: "CreatedById1");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "AccessRoles",
                newName: "CreatedById");

            migrationBuilder.RenameIndex(
                name: "IX_AccessRoles_CreatedByUserId1",
                table: "AccessRoles",
                newName: "IX_AccessRoles_CreatedById1");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "AccessKeys",
                newName: "CreatedById");

            migrationBuilder.RenameIndex(
                name: "IX_AccessKeys_CreatedByUserId",
                table: "AccessKeys",
                newName: "IX_AccessKeys_CreatedById");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgres", ",,");

            migrationBuilder.AddColumn<int>(
                name: "CreatedByAccount",
                table: "Accounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_AccessKeys_Accounts_CreatedById",
                table: "AccessKeys",
                column: "CreatedById",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AccessRoles_Accounts_CreatedById1",
                table: "AccessRoles",
                column: "CreatedById1",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Accounts_CreatedById",
                table: "Accounts",
                column: "CreatedById",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Accounts_CreatedById",
                table: "Categories",
                column: "CreatedById",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DTorrents_Accounts_CreatedById",
                table: "DTorrents",
                column: "CreatedById",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Movies_Accounts_CreatedById",
                table: "Movies",
                column: "CreatedById",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
