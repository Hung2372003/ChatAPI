using System;
using Microsoft.EntityFrameworkCore.Metadata;
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
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CHAT_CONTENT",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GROUP_CHAT_ID = table.Column<int>(type: "int", nullable: true),
                    CONTENT = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CREATED_TIME = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CREATED_BY = table.Column<int>(type: "int", nullable: true),
                    UPDATED_TIME = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UPDATED_BY = table.Column<int>(type: "int", nullable: true),
                    IS_DELETED = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    FILE_CODE = table.Column<int>(type: "int", nullable: true),
                    CONTENT_TIME = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHAT_CONTENT", x => x.ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CHAT_GROUPS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GROUP_NAME = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GROUP_AVARTAR = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    STATUS = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IS_DELETED = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CREATED_BY = table.Column<int>(type: "int", nullable: false),
                    CREATED_TIME = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    GROUP_DOUBLE = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    QUANTITY = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHAT_GROUPS", x => x.ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FEELING_POST",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    POST_ID = table.Column<int>(type: "int", nullable: false),
                    CREATED_BY = table.Column<int>(type: "int", nullable: false),
                    FEELING = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FEELING_POST", x => x.ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FILE_CHAT",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FILE_CODE = table.Column<int>(type: "int", nullable: true),
                    NAME = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TYPE = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PATH = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NAME_EXTENSION = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CREATED_TIME = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SIZE = table.Column<long>(type: "bigint", nullable: true),
                    IS_DELETED = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    DELETED_BY = table.Column<int>(type: "int", nullable: true),
                    SERVER_CODE = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FILE_CHAT", x => x.ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FILE_INFORMATION",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NAME = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PATH = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TYPE = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CREATED_TIME = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CREATED_BY = table.Column<int>(type: "int", nullable: true),
                    UPDATED_TIME = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UPDATED_BY = table.Column<int>(type: "int", nullable: true),
                    IS_DELETED = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CODE = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FILE_INFORMATION", x => x.ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FRIENDS_DOUBLE",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    USER_CODE1 = table.Column<int>(type: "int", nullable: false),
                    USER_CODE2 = table.Column<int>(type: "int", nullable: false),
                    IS_DELETED = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    STATUS = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CREATED_TIME = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CREATED_BY = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FRIENDS_DOUBLE", x => x.ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GROUP_MEMBER",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GROUP_CHAT_ID = table.Column<int>(type: "int", nullable: false),
                    MEMBER_CODE = table.Column<int>(type: "int", nullable: true),
                    STATUS = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    INVITED_TIME = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    INVITED_BY = table.Column<int>(type: "int", nullable: false),
                    DELETED_TIME = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DELETED_BY = table.Column<int>(type: "int", nullable: false),
                    IS_DELETED = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GROUP_MEMBER", x => x.ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "POST_COMMENT",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    POST_CODE = table.Column<int>(type: "int", nullable: false),
                    CONTENT = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CREATED_TIME = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CREATED_BY = table.Column<int>(type: "int", nullable: false),
                    IS_DELETED = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POST_COMMENT", x => x.ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "POSTS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CONTENT = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CREATED_BY = table.Column<int>(type: "int", nullable: false),
                    CREATED_TIME = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LIKE_NUMBER = table.Column<int>(type: "int", nullable: false),
                    COMMENT_NUMBER = table.Column<int>(type: "int", nullable: false),
                    STATUS = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IS_DELETED = table.Column<bool>(type: "tinyint(1)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POSTS", x => x.ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "USER_ACCOUNT",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    USER_CODE = table.Column<int>(type: "int", nullable: false),
                    USER_NAME = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    USER_PASSWORD = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IS_DELETED = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CREATED_TIME = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UPDATED_TIME = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CREATED_BY = table.Column<int>(type: "int", nullable: true),
                    UPDATED_BY = table.Column<int>(type: "int", nullable: true),
                    IS_ENCRYPTION = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    ROLE = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PERMISSION = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PROVIDER = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PROVIDER_SUB = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER_ACCOUNT", x => x.ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "USER_INFORMATION",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NAME = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ADDRESS = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EMAIL = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IS_DELETED = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    PHONE_NUMBER = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FILE_CODE = table.Column<int>(type: "int", nullable: true),
                    IS_ENCRYPTION = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    BIRTHDAY = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    AVATAR = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CREATED_TIME = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UPDATED_TIME = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UPDATED_BY = table.Column<int>(type: "int", nullable: true),
                    CREATED_BY = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER_INFORMATION", x => x.ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "USER_TOKENS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    USER_ID = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TOKEN = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CREATED_TIME = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER_TOKENS", x => x.ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
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

            migrationBuilder.DropTable(
                name: "USER_TOKENS");
        }
    }
}
