using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLPhongNET.Migrations
{
    /// <inheritdoc />
    public partial class CreateDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProcessTime",
                table: "RechargeRequest",
                newName: "ProcessedTime");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalPrice",
                table: "ServiceUsage",
                type: "decimal(15,2)",
                precision: 15,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(15,2)",
                oldPrecision: 15,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Services",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "DailyRevenueID",
                table: "RechargeRequest",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RechargeRequest_DailyRevenueID",
                table: "RechargeRequest",
                column: "DailyRevenueID");

            migrationBuilder.AddForeignKey(
                name: "FK_RechargeRequest_DailyRevenue_DailyRevenueID",
                table: "RechargeRequest",
                column: "DailyRevenueID",
                principalTable: "DailyRevenue",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RechargeRequest_DailyRevenue_DailyRevenueID",
                table: "RechargeRequest");

            migrationBuilder.DropIndex(
                name: "IX_RechargeRequest_DailyRevenueID",
                table: "RechargeRequest");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "DailyRevenueID",
                table: "RechargeRequest");

            migrationBuilder.RenameColumn(
                name: "ProcessedTime",
                table: "RechargeRequest",
                newName: "ProcessTime");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalPrice",
                table: "ServiceUsage",
                type: "decimal(15,2)",
                precision: 15,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(15,2)",
                oldPrecision: 15,
                oldScale: 2);
        }
    }
}
