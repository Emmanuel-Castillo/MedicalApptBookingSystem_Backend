using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalApptBookingSystem.Migrations
{
    /// <inheritdoc />
    public partial class MadeUserNotRequiredInPatientTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "FK_DoctorProfiles_Users_UserId",
                table: "DoctorProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_PatientProfiles_Users_UserId",
                table: "PatientProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeSlots_DoctorProfiles_DoctorId",
                table: "TimeSlots");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeSlots_DoctorProfiles_DoctorId1",
                table: "TimeSlots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PatientProfiles",
                table: "PatientProfiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DoctorProfiles",
                table: "DoctorProfiles");

            migrationBuilder.RenameTable(
                name: "PatientProfiles",
                newName: "Patients");

            migrationBuilder.RenameTable(
                name: "DoctorProfiles",
                newName: "Doctors");

            migrationBuilder.RenameIndex(
                name: "IX_PatientProfiles_UserId",
                table: "Patients",
                newName: "IX_Patients_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_DoctorProfiles_UserId",
                table: "Doctors",
                newName: "IX_Doctors_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Patients",
                table: "Patients",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Doctors",
                table: "Doctors",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Doctors_DoctorId",
                table: "Appointments",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Patients_PatientId",
                table: "Appointments",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Patients_PatientId1",
                table: "Appointments",
                column: "PatientId1",
                principalTable: "Patients",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorAvailability_Doctors_DoctorId",
                table: "DoctorAvailability",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Doctors_Users_UserId",
                table: "Doctors",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_Users_UserId",
                table: "Patients",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeSlots_Doctors_DoctorId",
                table: "TimeSlots",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeSlots_Doctors_DoctorId1",
                table: "TimeSlots",
                column: "DoctorId1",
                principalTable: "Doctors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Doctors_DoctorId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Patients_PatientId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Patients_PatientId1",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_DoctorAvailability_Doctors_DoctorId",
                table: "DoctorAvailability");

            migrationBuilder.DropForeignKey(
                name: "FK_Doctors_Users_UserId",
                table: "Doctors");

            migrationBuilder.DropForeignKey(
                name: "FK_Patients_Users_UserId",
                table: "Patients");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeSlots_Doctors_DoctorId",
                table: "TimeSlots");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeSlots_Doctors_DoctorId1",
                table: "TimeSlots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Patients",
                table: "Patients");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Doctors",
                table: "Doctors");

            migrationBuilder.RenameTable(
                name: "Patients",
                newName: "PatientProfiles");

            migrationBuilder.RenameTable(
                name: "Doctors",
                newName: "DoctorProfiles");

            migrationBuilder.RenameIndex(
                name: "IX_Patients_UserId",
                table: "PatientProfiles",
                newName: "IX_PatientProfiles_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Doctors_UserId",
                table: "DoctorProfiles",
                newName: "IX_DoctorProfiles_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PatientProfiles",
                table: "PatientProfiles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DoctorProfiles",
                table: "DoctorProfiles",
                column: "Id");

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
                name: "FK_DoctorProfiles_Users_UserId",
                table: "DoctorProfiles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PatientProfiles_Users_UserId",
                table: "PatientProfiles",
                column: "UserId",
                principalTable: "Users",
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
    }
}
