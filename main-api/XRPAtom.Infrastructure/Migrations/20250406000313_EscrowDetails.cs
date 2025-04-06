using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XRPAtom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EscrowDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RelatedEntityType",
                table: "Transactions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "RelatedEntityId",
                table: "Transactions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "RawResponse",
                table: "Transactions",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000);

            migrationBuilder.CreateTable(
                name: "EscrowDetails",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    EventId = table.Column<string>(type: "text", nullable: false),
                    ParticipantId = table.Column<string>(type: "text", nullable: false),
                    EscrowType = table.Column<string>(type: "text", nullable: false),
                    SourceAddress = table.Column<string>(type: "text", nullable: false),
                    DestinationAddress = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Condition = table.Column<string>(type: "text", nullable: false),
                    Fulfillment = table.Column<string>(type: "text", nullable: false),
                    FinishAfter = table.Column<long>(type: "bigint", nullable: false),
                    XummPayloadId = table.Column<string>(type: "text", nullable: false),
                    FinishPayloadId = table.Column<string>(type: "text", nullable: false),
                    CancelPayloadId = table.Column<string>(type: "text", nullable: false),
                    TransactionHash = table.Column<string>(type: "text", nullable: false),
                    OfferSequence = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EscrowDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EscrowDetails_CurtailmentEvents_EventId",
                        column: x => x.EventId,
                        principalTable: "CurtailmentEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EscrowDetails_EventId",
                table: "EscrowDetails",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EscrowDetails_ParticipantId",
                table: "EscrowDetails",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_EscrowDetails_TransactionHash",
                table: "EscrowDetails",
                column: "TransactionHash");

            migrationBuilder.CreateIndex(
                name: "IX_EscrowDetails_XummPayloadId",
                table: "EscrowDetails",
                column: "XummPayloadId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EscrowDetails");

            migrationBuilder.AlterColumn<string>(
                name: "RelatedEntityType",
                table: "Transactions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RelatedEntityId",
                table: "Transactions",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RawResponse",
                table: "Transactions",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);
        }
    }
}
