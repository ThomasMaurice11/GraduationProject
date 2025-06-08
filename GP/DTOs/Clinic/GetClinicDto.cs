using GP.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GP.DTOs.Clinic
{
    public class GetClinicDto
    {
        
        public int ClinicId { get; set; }

        
        public string DoctorId { get; set; }
        public string LocationUrl { get; set; }

        public DoctorDataDto Doctor { get; set; }

        
        public string Name { get; set; }

        public string Address { get; set; }


        public string Details { get; set; }

        public string Number { get; set; }
        
        public string CLinicEmail { get; set; }
        public string Status { get; set; }


    }
}
