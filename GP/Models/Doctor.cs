using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GP.Models
{
    public class Doctor :ApplicationUser
    {
        
        public string Specialization { get; set; }

        public string Number { get; set; }
   
        public string  MedicalId { get; set; } // Store image as binary data

        public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected

        public ICollection<Clinic> Clinics { get; set; } = new List<Clinic>();

      
    }
}
