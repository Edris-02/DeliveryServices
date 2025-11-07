using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryServices.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDeliveryRoute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // This migration is a no-op. The DeliveryRoute removal was already handled
            // in the UpdateOrders migration (20251105143451_UpdateOrders).
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No operations to reverse as Up() is a no-op
        }
    }
}
