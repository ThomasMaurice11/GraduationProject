using System.ComponentModel.DataAnnotations.Schema;

namespace GP.Models
{
    public class Slot
    {
        public int SlotId { get; set; }

        [ForeignKey("Clinic")]
        public int ClinicId { get; set; }
        public Clinic Clinic { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        // Add these new properties for soft delete functionality
        public bool Disabled { get; set; } = false;
        public DateTime? DisabledDate { get; set; }

        // Navigation property to Appointments (important for the delete logic)
        public ICollection<Appointment> Appointments { get; set; }
    }
}