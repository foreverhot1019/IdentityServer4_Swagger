using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MyIdentityServer.Data.Migrations
{
    public partial class ChgIdentityUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AddColumn<string>(
                name: "Resource",
                table: "AspNetUsers",
                maxLength: 50,
                nullable: false,
                defaultValue: "-",
                defaultValueSql:"'-'");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.CreateTable(
                name: "MenuAction",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(nullable: false),
                    ADDID = table.Column<string>(maxLength: 50, nullable: true),
                    ADDWHO = table.Column<string>(maxLength: 20, nullable: true),
                    ADDTS = table.Column<DateTime>(nullable: false),
                    EDITID = table.Column<string>(maxLength: 50, nullable: true),
                    EDITWHO = table.Column<string>(maxLength: 20, nullable: true),
                    EDITTS = table.Column<DateTime>(nullable: true),
                    Name = table.Column<string>(maxLength: 20, nullable: false),
                    Code = table.Column<string>(maxLength: 20, nullable: false),
                    Sort = table.Column<string>(maxLength: 20, nullable: false),
                    Description = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuAction", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "MenuItem",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(nullable: false),
                    ADDID = table.Column<string>(maxLength: 50, nullable: true),
                    ADDWHO = table.Column<string>(maxLength: 20, nullable: true),
                    ADDTS = table.Column<DateTime>(nullable: false),
                    EDITID = table.Column<string>(maxLength: 50, nullable: true),
                    EDITWHO = table.Column<string>(maxLength: 20, nullable: true),
                    EDITTS = table.Column<DateTime>(nullable: true),
                    Scope = table.Column<string>(nullable: true),
                    Title = table.Column<string>(maxLength: 20, nullable: false),
                    Description = table.Column<string>(maxLength: 100, nullable: true),
                    Code = table.Column<string>(maxLength: 20, nullable: false),
                    Url = table.Column<string>(maxLength: 100, nullable: false),
                    Controller = table.Column<string>(maxLength: 50, nullable: true),
                    IconCls = table.Column<string>(maxLength: 50, nullable: true),
                    ParentId = table.Column<int>(nullable: true),
                    Resource = table.Column<string>(maxLength: 50, nullable: false, defaultValueSql: "'-'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItem", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MenuItem_MenuItem_ParentId",
                        column: x => x.ParentId,
                        principalTable: "MenuItem",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoleMenu",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(nullable: false),
                    ADDID = table.Column<string>(maxLength: 50, nullable: true),
                    ADDWHO = table.Column<string>(maxLength: 20, nullable: true),
                    ADDTS = table.Column<DateTime>(nullable: false),
                    EDITID = table.Column<string>(maxLength: 50, nullable: true),
                    EDITWHO = table.Column<string>(maxLength: 20, nullable: true),
                    EDITTS = table.Column<DateTime>(nullable: true),
                    RoleName = table.Column<string>(maxLength: 20, nullable: false),
                    RoleId = table.Column<string>(maxLength: 50, nullable: false),
                    MenuId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleMenu", x => x.ID);
                    table.ForeignKey(
                        name: "FK_RoleMenu_MenuItem_MenuId",
                        column: x => x.MenuId,
                        principalTable: "MenuItem",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MenuItem_ParentId",
                table: "MenuItem",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleMenu_MenuId",
                table: "RoleMenu",
                column: "MenuId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MenuAction");

            migrationBuilder.DropTable(
                name: "RoleMenu");

            migrationBuilder.DropTable(
                name: "MenuItem");

            migrationBuilder.DropColumn(
                name: "Resource",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string));
        }
    }
}
