using System.ComponentModel.DataAnnotations;

namespace GP.DTOs.Slot
{
    public class SlotDTOs
    {
        public class SlotDto
        {
            public int SlotId { get; set; }
            public int ClinicId { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }
        }

        public class SlotCreateDto
        {
            public int ClinicId { get; set; }
            [Required]
            [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$",
             ErrorMessage = "Time must be in HH:mm format (e.g., 07:30)")]
            public string StartTime { get; set; }

            [Required]
            [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$",
                ErrorMessage = "Time must be in HH:mm format (e.g., 08:30)")]
            public string EndTime { get; set; }
        }

        public class SlotUpdateDto
        {
            public int SlotId { get; set; }
            public int ClinicId { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }
        }
    }
}
