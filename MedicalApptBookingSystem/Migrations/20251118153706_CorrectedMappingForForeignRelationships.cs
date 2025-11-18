using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalApptBookingSystem.Migrations
{
    /// <inheritdoc />
    public partial class CorrectedMappingForForeignRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Patients_PatientId1",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeSlots_Doctors_DoctorId1",
                table: "TimeSlots");

            migrationBuilder.DropIndex(
                name: "IX_TimeSlots_DoctorId1",
                table: "TimeSlots");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_PatientId1",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "DoctorId1",
                table: "TimeSlots");

            migrationBuilder.DropColumn(
                name: "PatientId1",
                table: "Appointments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DoctorId1",
                table: "TimeSlots",
                type: "int",
                nullable: true);

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
                name: "IX_Appointments_PatientId1",
                table: "Appointments",
                column: "PatientId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Patients_PatientId1",
                table: "Appointments",
                column: "PatientId1",
                principalTable: "Patients",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeSlots_Doctors_DoctorId1",
                table: "TimeSlots",
                column: "DoctorId1",
                principalTable: "Doctors",
                principalColumn: "Id");
        }
    }
}
