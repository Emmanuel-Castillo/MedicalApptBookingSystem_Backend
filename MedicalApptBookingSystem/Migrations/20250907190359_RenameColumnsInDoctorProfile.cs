using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalApptBookingSystem.Migrations
{
    /// <inheritdoc />
    public partial class RenameColumnsInDoctorProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorProfiles_Users_DoctorId",
                table: "DoctorProfiles");

            migrationBuilder.RenameColumn(
                name: "DoctorId",
                table: "DoctorProfiles",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_DoctorProfiles_DoctorId",
                table: "DoctorProfiles",
                newName: "IX_DoctorProfiles_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorProfiles_Users_UserId",
                table: "DoctorProfiles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorProfiles_Users_UserId",
                table: "DoctorProfiles");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "DoctorProfiles",
                newName: "DoctorId");

            migrationBuilder.RenameIndex(
                name: "IX_DoctorProfiles_UserId",
                table: "DoctorProfiles",
                newName: "IX_DoctorProfiles_DoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorProfiles_Users_DoctorId",
                table: "DoctorProfiles",
                column: "DoctorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
