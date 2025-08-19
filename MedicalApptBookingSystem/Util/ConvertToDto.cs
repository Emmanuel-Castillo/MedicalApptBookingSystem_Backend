using MedicalApptBookingSystem.Controllers;
using MedicalApptBookingSystem.DTO;
using MedicalApptBookingSystem.Models;

namespace MedicalApptBookingSystem.Util
{
    public class ConvertToDto
    {
        public AppointmentDto ConvertToAppointmentDto(Appointment a)
        {
            var appointmentDto = new AppointmentDto
            {
                Id = a.Id,
                PatientId = a.PatientId,
                Patient = this.ConvertToUserDto(a.Patient),
                TimeSlotId = a.TimeSlot.Id,
                TimeSlot = this.ConvertToTimeSlotDto(a.TimeSlot),
                Notes = a.Notes
            };

            return appointmentDto;
        }
        public List<AppointmentDto> ConvertToListAppointmentDto(List<Appointment> appointments)
        {
            var appointmentsDto = appointments.Select(a => this.ConvertToAppointmentDto(a)).ToList();

            return appointmentsDto;
        }

        public List<TimeSlotDto> ConvertToListTimeSlotDto(List<TimeSlot> timeSlots)
        {
            var timeSlotDtos = timeSlots.Select(t => this.ConvertToTimeSlotDto(t)).ToList();

            return timeSlotDtos;
        }

        public List<UserDto> ConvertToListUserDto(List<User> users)
        {
            var usersDto = users.Select(u => this.ConvertToUserDto(u)).ToList();
            return usersDto;
        }

        public TimeSlotDto ConvertToTimeSlotDto(TimeSlot t)
        {
            var timeSlotDto = new TimeSlotDto
            {
                Id = t.Id,
                StartTime = t.StartTime,
                EndTime = t.EndTime,
                IsBooked = t.IsBooked,
                Doctor = this.ConvertToUserDto(t.Doctor)
            };

            return timeSlotDto;
        }

        public UserDto ConvertToUserDto(User user) {
            var userDto = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString()
            };

            return userDto;
        }

        public List<DoctorAvailabilityDto> ConvertToListDoctorAvailDto(List<DoctorAvailability> doctorAvailList)
        {
            var availDto = doctorAvailList.Select(a => this.ConvertToDoctorAvailDto(a)).ToList();

            return availDto;
        }

        public DoctorAvailabilityDto ConvertToDoctorAvailDto(DoctorAvailability a)
        {
            var availDto = new DoctorAvailabilityDto
            {
                Id = a.Id,
                DayOfWeek = a.DayOfWeek.ToString(),
                Doctor = this.ConvertToUserDto(a.Doctor),
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                StartDate = a.StartDate,
                EndDate = a.EndDate,
            };

            return availDto;
        }
    }
}
