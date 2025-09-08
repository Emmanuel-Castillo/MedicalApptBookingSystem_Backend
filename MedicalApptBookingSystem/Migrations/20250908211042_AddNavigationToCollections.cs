using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalApptBookingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddNavigationToCollections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Users_PatientId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_DoctorAvailability_Users_DoctorId",
                table: "DoctorAvailability");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeSlots_Users_DoctorId",
                table: "TimeSlots");

            migrationBuilder.AddColumn<int>(
                name: "DoctorId1",
                table: "TimeSlots",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DoctorId",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PatientId1",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimeSlots_DoctorId1",
                table: "TimeSlots",
                column: "DoctorId1");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DoctorId",
                table: "Appointments",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientId1",
                table: "Appointments",
                column: "PatientId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_DoctorProfiles_DoctorId",
                table: "Appointments",
                column: "DoctorId",
                principalTable: "DoctorProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_PatientProfiles_PatientId",
                table: "Appointments",
                column: "PatientId",
                principalTable: "PatientProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_PatientProfiles_PatientId1",
                table: "Appointments",
                column: "PatientId1",
                principalTable: "PatientProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorAvailability_DoctorProfiles_DoctorId",
                table: "DoctorAvailability",
                column: "DoctorId",
                principalTable: "DoctorProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeSlots_DoctorProfiles_DoctorId",
                table: "TimeSlots",
                column: "DoctorId",
                principalTable: "DoctorProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeSlots_DoctorProfiles_DoctorId1",
                table: "TimeSlots",
                column: "DoctorId1",
                principalTable: "DoctorProfiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_DoctorProfiles_DoctorId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_PatientProfiles_PatientId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_PatientProfiles_PatientId1",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_DoctorAvailability_DoctorProfiles_DoctorId",
                table: "DoctorAvailability");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeSlots_DoctorProfiles_DoctorId",
                table: "TimeSlots");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeSlots_DoctorProfiles_DoctorId1",
                table: "TimeSlots");

            migrationBuilder.DropIndex(
                name: "IX_TimeSlots_DoctorId1",
                table: "TimeSlots");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_DoctorId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_PatientId1",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "DoctorId1",
                table: "TimeSlots");

            migrationBuilder.DropColumn(
                name: "DoctorId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "PatientId1",
                table: "Appointments");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Users_PatientId",
                table: "Appointments",
                column: "PatientId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorAvailability_Users_DoctorId",
                table: "DoctorAvailability",
                column: "DoctorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeSlots_Users_DoctorId",
                table: "TimeSlots",
                column: "DoctorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
