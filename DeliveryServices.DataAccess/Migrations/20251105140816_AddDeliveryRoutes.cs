using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryServices.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryRoutes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeliveryRoutesId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DeliveryRoutes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RouteName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CourierName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryRoutes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DeliveryRoutesId",
                table: "Orders",
                column: "DeliveryRoutesId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_DeliveryRoutes_DeliveryRoutesId",
                table: "Orders",
                column: "DeliveryRoutesId",
                principalTable: "DeliveryRoutes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_DeliveryRoutes_DeliveryRoutesId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "DeliveryRoutes");

            migrationBuilder.DropIndex(
                name: "IX_Orders_DeliveryRoutesId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryRoutesId",
                table: "Orders");
        }
    }
}
