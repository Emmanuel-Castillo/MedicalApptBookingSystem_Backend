using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalApptBookingSystem.Migrations
{
    /// <inheritdoc />
    public partial class InsertProfilesForExistingPatients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert profiles for existing doctors
            migrationBuilder.Sql(@"
            INSERT INTO PatientProfiles (HeightImperial, WeightImperial, UserId)
            SELECT 0.0, 0.0, Id
            FROM Users
            WHERE Role = 0; -- UserRole.Patient = 0
        ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
