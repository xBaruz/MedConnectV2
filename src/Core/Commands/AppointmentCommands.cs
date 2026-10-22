namespace MedConnect.Core.Commands;

public record CreateAppointmentCommand(
    Guid AppointmentDoctorId,
    DateTime AppointmentDate
);

