namespace MedConnect.Models;

public class Appointment
{
    public Guid Id { get; set; }
    public Guid AppointmentDoctorId { get; set; }
    public Guid? PatientId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public AppointmentStatus Status { get; set; } = new AppointmentStatus.Available();

    private Appointment() { }

    public Appointment(
        Guid id, 
        Guid appointmentDoctorId, 
        DateTime appointmentDate
    )
    {
        Id = id;
        AppointmentDoctorId = appointmentDoctorId;
        AppointmentDate = appointmentDate;
        Status = new AppointmentStatus.Available();
    }

    public static Appointment Create(Guid appointmentDoctorId, DateTime appointmentDate)
    {
        return new Appointment(Guid.NewGuid(), appointmentDoctorId, appointmentDate);
    }

    public void Book(Guid patientId)
    {
        if (Status is not AppointmentStatus.Available)
        {
            throw new InvalidOperationException("Wizyta nie jest dostępna do rezerwacji.");
        }

        PatientId = patientId;
        Status = new AppointmentStatus.Scheduled();
    }

    public void Cancel()
    {
        if (Status is AppointmentStatus.Completed)
        {
            throw new InvalidOperationException("Nie można odwołać zakończonej wizyty.");
        }

        Status = new AppointmentStatus.Canceled();
    }

    public void Complete()
    {
        if (Status is not AppointmentStatus.Scheduled)
        {
            throw new InvalidOperationException("Tylko zaplanowaną wizytę można zakończyć.");
        }

        Status = new AppointmentStatus.Completed();
    }
}

public abstract record AppointmentStatus
{
    private AppointmentStatus() { }

    public sealed record Available : AppointmentStatus;
    public sealed record Scheduled : AppointmentStatus;
    public sealed record Canceled : AppointmentStatus;
    public sealed record Completed : AppointmentStatus;
}