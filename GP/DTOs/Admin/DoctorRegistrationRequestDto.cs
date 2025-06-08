namespace GP.DTOs.Admin
{
    public class DoctorRegistrationRequestDto
    {
        public string Id { get; set; }  
        public string UserName { get; set; } 
        public string Specialization { get; set; }
        public string Number { get; set; }
        public string MedicalId { get; set; }
        public string Status { get; set; }
    }
}
