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
                Patient = new UserDto
                {
                    Id = a.Patient.Id,
                    Email = a.Patient.Email,
                    FullName = a.Patient.FullName,
                    Role = a.Patient.Role.ToString(),
                },
                TimeSlotId = a.TimeSlot.Id,
                TimeSlot = new TimeSlotDto
                {
                    Id = a.TimeSlot.Id,
                    StartTime = a.TimeSlot.StartTime,
                    EndTime = a.TimeSlot.EndTime,
                    Doctor = new UserDto
                    {
                        Id = a.TimeSlot.Doctor.Id,
                        FullName = a.TimeSlot.Doctor.FullName,
                        Email = a.TimeSlot.Doctor.Email,
                        Role = a.TimeSlot.Doctor.Role.ToString()
                    },
                    IsBooked = a.TimeSlot.IsBooked,
                },
                Notes = a.Notes
            };

            return appointmentDto;
        }
        public List<AppointmentDto> ConvertToListAppointmentDto(List<Appointment> appointments)
        {
            var appointmentsDto = appointments.Select(a => new AppointmentDto
            {
                Id = a.Id,
                PatientId = a.PatientId,
                Patient = new UserDto
                {
                    Id = a.Patient.Id,
                    Email = a.Patient.Email,
                    FullName = a.Patient.FullName,
                    Role = a.Patient.Role.ToString(),
                },
                TimeSlotId = a.TimeSlot.Id,
                TimeSlot = new TimeSlotDto
                {
                    Id = a.TimeSlot.Id,
                    StartTime = a.TimeSlot.StartTime,
                    EndTime = a.TimeSlot.EndTime,
                    Doctor = new UserDto
                    {
                        Id = a.TimeSlot.Doctor.Id,
                        FullName = a.TimeSlot.Doctor.FullName,
                        Email = a.TimeSlot.Doctor.Email,
                        Role = a.TimeSlot.Doctor.Role.ToString()
                    },
                    IsBooked = a.TimeSlot.IsBooked,
                },
                Notes = a.Notes
            }).ToList();

            return appointmentsDto;
        }

        public List<TimeSlotDto> ConvertToListTimeSlotDto(List<TimeSlot> timeSlots)
        {
            var timeSlotDtos = timeSlots.Select(t => new TimeSlotDto
            {
                Id = t.Id,
                StartTime = t.StartTime,
                EndTime = t.EndTime,
                IsBooked = t.IsBooked,
                Doctor = new UserDto
                {
                    Id = t.Doctor.Id,
                    FullName = t.Doctor.FullName,
                    Email = t.Doctor.Email,
                    Role = t.Doctor.Role.ToString()
                }

            }).ToList();

            return timeSlotDtos;
        }

        public List<UserDto> ConvertToListUserDto(List<User> users)
        {
            var usersDto = users.Select(u => new UserDto { Id = u.Id, Email = u.Email, FullName = u.FullName, Role = u.Role.ToString() }).ToList();
            return usersDto;
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
    }
}
