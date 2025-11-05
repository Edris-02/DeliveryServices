using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryServices.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDeliveryRoute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_DeliveryRoutes_DeliveryRoutesId",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "DeliveryRoutesId",
                table: "Orders",
                newName: "DeliveryRouteId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_DeliveryRoutesId",
                table: "Orders",
                newName: "IX_Orders_DeliveryRouteId");

            migrationBuilder.AlterColumn<string>(
                name: "RouteName",
                table: "DeliveryRoutes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CourierName",
                table: "DeliveryRoutes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "DeliveryRoutes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "DeliveryRoutes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                table: "DeliveryRoutes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "DeliveryRoutes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_DeliveryRoutes_DeliveryRouteId",
                table: "Orders",
                column: "DeliveryRouteId",
                principalTable: "DeliveryRoutes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_DeliveryRoutes_DeliveryRouteId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "DeliveryRoutes");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "DeliveryRoutes");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "DeliveryRoutes");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "DeliveryRoutes");

            migrationBuilder.RenameColumn(
                name: "DeliveryRouteId",
                table: "Orders",
                newName: "DeliveryRoutesId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_DeliveryRouteId",
                table: "Orders",
                newName: "IX_Orders_DeliveryRoutesId");

            migrationBuilder.AlterColumn<string>(
                name: "RouteName",
                table: "DeliveryRoutes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CourierName",
                table: "DeliveryRoutes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_DeliveryRoutes_DeliveryRoutesId",
                table: "Orders",
                column: "DeliveryRoutesId",
                principalTable: "DeliveryRoutes",
                principalColumn: "Id");
        }
    }
}
