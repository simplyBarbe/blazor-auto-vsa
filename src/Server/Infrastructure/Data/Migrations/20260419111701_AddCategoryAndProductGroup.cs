using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Server.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryAndProductGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product_groups",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_groups", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_groups_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_categories_name",
                table: "categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_groups_category_id_name",
                table: "product_groups",
                columns: new[] { "category_id", "name" },
                unique: true);

            migrationBuilder.AddColumn<int>(
                name: "group_id",
                table: "products",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(
                """
                INSERT INTO categories (name) VALUES ('General');
                INSERT INTO product_groups (category_id, name)
                SELECT id, 'Misc' FROM categories WHERE name = 'General' LIMIT 1;
                UPDATE products SET group_id = (SELECT id FROM product_groups WHERE name = 'Misc' LIMIT 1)
                WHERE group_id IS NULL;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "group_id",
                table: "products",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_products_group_id",
                table: "products",
                column: "group_id");

            migrationBuilder.AddForeignKey(
                name: "FK_products_product_groups_group_id",
                table: "products",
                column: "group_id",
                principalTable: "product_groups",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_products_product_groups_group_id",
                table: "products");

            migrationBuilder.DropIndex(
                name: "IX_products_group_id",
                table: "products");

            migrationBuilder.DropColumn(
                name: "group_id",
                table: "products");

            migrationBuilder.DropTable(
                name: "product_groups");

            migrationBuilder.DropTable(
                name: "categories");
        }
    }
}
