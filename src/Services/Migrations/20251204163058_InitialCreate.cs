using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubrogationDemandManagement.Services.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommunicationLogs",
                columns: table => new
                {
                    CommunicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DemandPackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Channel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RecipientsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CcRecipientsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailSubject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EmailBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FromAddress = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DeliveryTrackingId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    InitiatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunicationLogs", x => x.CommunicationId);
                });

            migrationBuilder.CreateTable(
                name: "DemandPackages",
                columns: table => new
                {
                    PackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubrogationCaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MergedCoverLetterPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GeneratedPdfPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PdfHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    PdfSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    PageCount = table.Column<int>(type: "int", nullable: false),
                    BookmarksJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemandPackages", x => x.PackageId);
                });

            migrationBuilder.CreateTable(
                name: "SubrogationCases",
                columns: table => new
                {
                    CaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PolicyNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LossDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InsuredLiabilityPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ThirdPartyLiabilityPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    TotalPaidIndemnity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalPaidExpense = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OutstandingReserves = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RecoverySought = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentBreakdown = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InternalNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubrogationCases", x => x.CaseId);
                });

            migrationBuilder.CreateTable(
                name: "Templates",
                columns: table => new
                {
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Jurisdiction = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LineOfBusiness = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LossType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Phase = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BlobStoragePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Format = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    MergeFields = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Source = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ExternalCMSId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Templates", x => x.TemplateId);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TenantCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SSOProvider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SSOMetadataUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SSOClientId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ParentSystemApiUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ParentSystemApiKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EmailFromAddress = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmailFromName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FeaturesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubscriptionTier = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.TenantId);
                });

            migrationBuilder.CreateTable(
                name: "PackageDocuments",
                columns: table => new
                {
                    PackageDocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DemandPackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BlobStoragePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ExternalDocumentId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsIncluded = table.Column<bool>(type: "bit", nullable: false),
                    IsSensitive = table.Column<bool>(type: "bit", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageDocuments", x => x.PackageDocumentId);
                    table.ForeignKey(
                        name: "FK_PackageDocuments_DemandPackages_DemandPackageId",
                        column: x => x.DemandPackageId,
                        principalTable: "DemandPackages",
                        principalColumn: "PackageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationLogs_DeliveryTrackingId",
                table: "CommunicationLogs",
                column: "DeliveryTrackingId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationLogs_DemandPackageId",
                table: "CommunicationLogs",
                column: "DemandPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationLogs_Status_CreatedAt",
                table: "CommunicationLogs",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationLogs_TenantId",
                table: "CommunicationLogs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandPackages_CaseId_Version",
                table: "DemandPackages",
                columns: new[] { "SubrogationCaseId", "VersionNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_DemandPackages_Status",
                table: "DemandPackages",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DemandPackages_SubrogationCaseId",
                table: "DemandPackages",
                column: "SubrogationCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandPackages_TenantId",
                table: "DemandPackages",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageDocuments_DemandPackageId",
                table: "PackageDocuments",
                column: "DemandPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageDocuments_PackageId_DisplayOrder",
                table: "PackageDocuments",
                columns: new[] { "DemandPackageId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_SubrogationCases_ClaimId",
                table: "SubrogationCases",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_SubrogationCases_CreatedAt",
                table: "SubrogationCases",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SubrogationCases_TenantId",
                table: "SubrogationCases",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SubrogationCases_TenantId_Status",
                table: "SubrogationCases",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Templates_ExternalCMSId",
                table: "Templates",
                column: "ExternalCMSId");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_TenantId",
                table: "Templates",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_TenantId_Jurisdiction_LOB",
                table: "Templates",
                columns: new[] { "TenantId", "Jurisdiction", "LineOfBusiness" });

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_IsActive",
                table: "Tenants",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_TenantCode",
                table: "Tenants",
                column: "TenantCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommunicationLogs");

            migrationBuilder.DropTable(
                name: "PackageDocuments");

            migrationBuilder.DropTable(
                name: "SubrogationCases");

            migrationBuilder.DropTable(
                name: "Templates");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropTable(
                name: "DemandPackages");
        }
    }
}
