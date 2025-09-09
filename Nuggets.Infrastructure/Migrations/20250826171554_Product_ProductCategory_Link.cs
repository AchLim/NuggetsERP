using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nuggets.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Product_ProductCategory_Link : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "product_product",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductCategoryId",
                table: "product_product",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_product_ProductCategoryId",
                table: "product_product",
                column: "ProductCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_product_product_product_category_ProductCategoryId",
                table: "product_product",
                column: "ProductCategoryId",
                principalTable: "product_category",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_product_product_product_category_ProductCategoryId",
                table: "product_product");

            migrationBuilder.DropIndex(
                name: "IX_product_product_ProductCategoryId",
                table: "product_product");

            migrationBuilder.DropColumn(
                name: "ProductCategoryId",
                table: "product_product");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "product_product",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
