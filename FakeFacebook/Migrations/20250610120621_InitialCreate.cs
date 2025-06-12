using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FakeFacebook.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CHAT_CONTENT",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GROUP_CHAT_ID = table.Column<int>(type: "int", nullable: false),
                    CONTENT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CREATED_TIME = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CREATED_BY = table.Column<int>(type: "int", nullable: false),
                    UPDATED_TIME = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UPDATED_BY = table.Column<int>(type: "int", nullable: false),
                    IS_DELETED = table.Column<bool>(type: "bit", nullable: false),
                    FILE_CODE = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHAT_CONTENT", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "CHAT_GROUPS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GROUP_NAME = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GROUP_AVARTAR = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    STATUS = table.Column<bool>(type: "bit", nullable: false),
                    IS_DELETED = table.Column<bool>(type: "bit", nullable: false),
                    CREATED_BY = table.Column<int>(type: "int", nullable: false),
                    CREATED_TIME = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GROUP_DOUBLE = table.Column<bool>(type: "bit", nullable: false),
                    QUANTITY = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHAT_GROUPS", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "FEELING_POST",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    POST_ID = table.Column<int>(type: "int", nullable: false),
                    CREATED_BY = table.Column<int>(type: "int", nullable: false),
                    LIKE = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FEELING_POST", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "FILE_CHAT",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FILE_CODE = table.Column<int>(type: "int", nullable: false),
                    NAME = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TYPE = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PATH = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NAME_EXTENSION = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CREATED_TIME = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SIZE = table.Column<long>(type: "bigint", nullable: true),
                    IS_DELETED = table.Column<bool>(type: "bit", nullable: false),
                    DELETED_BY = table.Column<int>(type: "int", nullable: false),
                    SERVER_CODE = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FILE_CHAT", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "FILE_INFORMATION",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NAME = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PATH = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TYPE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CREATED_TIME = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CREATED_BY = table.Column<int>(type: "int", nullable: true),
                    UPDATED_TIME = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UPDATED_BY = table.Column<int>(type: "int", nullable: true),
                    IS_DELETED = table.Column<bool>(type: "bit", nullable: false),
                    CODE = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FILE_INFORMATION", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "FRIENDS_DOUBLE",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    USER_CODE1 = table.Column<int>(type: "int", nullable: false),
                    USER_CODE2 = table.Column<int>(type: "int", nullable: false),
                    IS_DELETED = table.Column<bool>(type: "bit", nullable: false),
                    STATUS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CREATED_TIME = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CREATED_BY = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FRIENDS_DOUBLE", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "GROUP_MEMBER",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GROUP_CHAT_ID = table.Column<int>(type: "int", nullable: false),
                    MEMBER_CODE = table.Column<int>(type: "int", nullable: true),
                    STATUS = table.Column<bool>(type: "bit", nullable: false),
                    INVITED_TIME = table.Column<DateTime>(type: "datetime2", nullable: false),
                    INVITED_BY = table.Column<int>(type: "int", nullable: false),
                    DELETED_TIME = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DELETED_BY = table.Column<int>(type: "int", nullable: false),
                    IS_DELETED = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GROUP_MEMBER", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "POST_COMMENT",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    POST_CODE = table.Column<int>(type: "int", nullable: false),
                    CONTENT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CREATED_TIME = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CREATED_BY = table.Column<int>(type: "int", nullable: false),
                    IS_DELETED = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POST_COMMENT", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "POSTS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CONTENT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CREATED_BY = table.Column<int>(type: "int", nullable: false),
                    CREATED_TIME = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LIKE_NUMBER = table.Column<int>(type: "int", nullable: false),
                    COMMENT_NUMBER = table.Column<int>(type: "int", nullable: false),
                    STATUS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IS_DELETED = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POSTS", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "USER_ACCOUNT",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    USER_CODE = table.Column<int>(type: "int", nullable: false),
                    USER_NAME = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    USER_PASSWORD = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IS_DELETED = table.Column<bool>(type: "bit", nullable: false),
                    CREATED_TIME = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UPDATED_TIME = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CREATED_BY = table.Column<int>(type: "int", nullable: true),
                    UPDATED_BY = table.Column<int>(type: "int", nullable: true),
                    IS_ENCRYPTION = table.Column<bool>(type: "bit", nullable: true),
                    ROLE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PERMISSION = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER_ACCOUNT", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "USER_INFORMATION",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NAME = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ADDRESS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMAIL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IS_DELETED = table.Column<bool>(type: "bit", nullable: true),
                    PHONE_NUMBER = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FILE_CODE = table.Column<int>(type: "int", nullable: false),
                    IS_ENCRYPTION = table.Column<bool>(type: "bit", nullable: true),
                    BIRTHDAY = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AVATAR = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CREATED_TIME = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UPDATED_TIME = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UPDATED_BY = table.Column<int>(type: "int", nullable: true),
                    CREATED_BY = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER_INFORMATION", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CHAT_CONTENT");

            migrationBuilder.DropTable(
                name: "CHAT_GROUPS");

            migrationBuilder.DropTable(
                name: "FEELING_POST");

            migrationBuilder.DropTable(
                name: "FILE_CHAT");

            migrationBuilder.DropTable(
                name: "FILE_INFORMATION");

            migrationBuilder.DropTable(
                name: "FRIENDS_DOUBLE");

            migrationBuilder.DropTable(
                name: "GROUP_MEMBER");

            migrationBuilder.DropTable(
                name: "POST_COMMENT");

            migrationBuilder.DropTable(
                name: "POSTS");

            migrationBuilder.DropTable(
                name: "USER_ACCOUNT");

            migrationBuilder.DropTable(
                name: "USER_INFORMATION");
        }
    }
}
