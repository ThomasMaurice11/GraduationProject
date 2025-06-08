using GP.DTOs.Clinic;
using static GP.DTOs.Slot.SlotDTOs;

namespace GP.DTOs.Appointment
{
    public class AppointmentDto
    {
        public int AppointmentId { get; set; }
        public int ClinicId { get; set; }
        public GetClinicDto Clinic { get; set; }
        public string UserId { get; set; }
        public UserDto User { get; set; }
        public int SlotId { get; set; }
        public SlotDto Slot { get; set; }
        public DateTime AppointmentDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? PatientNotes { get; set; }
        public string? DoctorNotes { get; set; }
        public string PetName { get; set; }
        public string Breed { get; set; }
        public string PetType { get; set; }
        public string? ReasonForVisit { get; set; }

    }

    public class AppointmentCreateDto
    {
        public int ClinicId { get; set; }
        public int SlotId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string? PatientNotes { get; set; }
        public string PetName { get; set; }
        public string Breed { get; set; }
        public string PetType { get; set; }
        public string? ReasonForVisit { get; set; }

    }
}
