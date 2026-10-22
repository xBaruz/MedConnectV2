using MedConnect.Models;

namespace MedConnect.Core.Dtos;

public record AppointmentDto(Guid AppointmentDoctorId, DateTime AppointmentDate, string Status);
