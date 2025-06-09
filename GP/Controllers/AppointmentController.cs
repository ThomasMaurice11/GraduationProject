using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using GP.DTOs;
using GP.DTOs.Appointment;
using GP.DTOs.Clinic;
using GP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static GP.DTOs.Slot.SlotDTOs;

namespace GP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AppointmentController : ControllerBase
    {
        private readonly AuthDbContext _context;

        public AppointmentController(AuthDbContext context)
        {
            _context = context;
        }

        private AppointmentDto MapAppointmentToDto(Appointment appointment)
        {
            return new AppointmentDto
            {
                AppointmentId = appointment.AppointmentId,
                ClinicId = appointment.ClinicId,
                Clinic = new GetClinicDto
                {
                    ClinicId = appointment.Clinic?.ClinicId ?? 0,
                    Name = appointment.Clinic?.Name ?? string.Empty,
                    LocationUrl = appointment.Clinic?.LocationUrl ?? string.Empty,
                    Address = appointment.Clinic?.Address ?? string.Empty,
                    CLinicEmail = appointment.Clinic?.CLinicEmail ?? string.Empty,
                    Number = appointment.Clinic?.Number ?? string.Empty,
                    Details = appointment.Clinic?.Details ?? string.Empty,
                    DoctorId = appointment.Clinic?.DoctorId ?? string.Empty,
                    Doctor = appointment.Clinic?.Doctor != null ? new DoctorDataDto
                    {
                        Id = appointment.Clinic.Doctor.Id,
                        UserName = appointment.Clinic.Doctor.UserName,
                        Email = appointment.Clinic.Doctor.Email,
                        Specialization = appointment.Clinic.Doctor.Specialization,
                        Number = appointment.Clinic.Doctor.Number,
                        MedicalId = appointment.Clinic.Doctor.MedicalId
                    } : null
                },
                UserId = appointment.UserId,
                User = new UserDto
                {
                    Id = appointment.User?.Id ?? string.Empty,
                    UserName = appointment.User?.UserName ?? string.Empty,
                    PhoneNumber = appointment.User?.PhoneNumber ?? string.Empty,
                    Email=appointment.User?.Email??string.Empty,
                },
                SlotId = appointment.SlotId,
                Slot = new SlotDto
                {
                    SlotId = appointment.Slot?.SlotId ?? 0,
                    ClinicId = appointment.Slot?.ClinicId ?? 0,
                    StartTime = appointment.Slot?.StartTime.ToString(@"hh\:mm") ?? "00:00",
                    EndTime = appointment.Slot?.EndTime.ToString(@"hh\:mm") ?? "00:00"
                },
                AppointmentDate = appointment.AppointmentDate,
                CreatedAt = appointment.CreatedAt,
                PatientNotes = appointment.PatientNotes,
                DoctorNotes = appointment.DoctorNotes,
                PetName = appointment.PetName,
                Breed = appointment.Breed,
                PetType = appointment.PetType,
                ReasonForVisit = appointment.ReasonForVisit
            };
        }

        [HttpPost]
        public async Task<ActionResult<AppointmentDto>> CreateAppointment(AppointmentCreateDto appointmentDto)
        {
            var userId = User?.FindFirst("id")?.Value;

            // Validate slot exists
            var slot = await _context.Slots.FindAsync(appointmentDto.SlotId);
            if (slot == null) return BadRequest("Invalid slot ID");

            // Check if slot is available
            var isBooked = await _context.Appointments
                .AnyAsync(a => a.SlotId == appointmentDto.SlotId &&
                             a.AppointmentDate.Date == appointmentDto.AppointmentDate.Date);
            if (isBooked) return BadRequest("Slot already booked");

            var appointment = new Appointment
            {
                ClinicId = appointmentDto.ClinicId,
                SlotId = appointmentDto.SlotId,
                AppointmentDate = appointmentDto.AppointmentDate.Date,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                PatientNotes = appointmentDto.PatientNotes,
                PetName = appointmentDto.PetName,
                Breed = appointmentDto.Breed,
                PetType = appointmentDto.PetType,
                ReasonForVisit = appointmentDto.ReasonForVisit
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var newAppointment = await _context.Appointments
                .Include(a => a.Clinic)
                .Include(a => a.Slot)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointment.AppointmentId);

            return CreatedAtAction(nameof(GetAppointment),
                new { id = appointment.AppointmentId },
                MapAppointmentToDto(newAppointment));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AppointmentDto>> GetAppointment(int id)
        {
            var userId = User?.FindFirst("id")?.Value;

            var appointment = await _context.Appointments
                .Include(a => a.Clinic)
                .Include(a => a.Slot)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null) return NotFound();
            if (appointment.UserId != userId)
                return Unauthorized();

            return Ok(MapAppointmentToDto(appointment));
        }

        [HttpGet("my-appointments")]
        public async Task<ActionResult<List<AppointmentDto>>> GetMyAppointments()
        {
            var userId = User?.FindFirst("id")?.Value;

            var appointments = await _context.Appointments
                .Where(a => a.UserId == userId)
                .Include(a => a.Clinic)
                .Include(a => a.Slot)
                .Include(a => a.User)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.Slot.StartTime)
                .ToListAsync();

            return Ok(appointments.Select(MapAppointmentToDto).ToList());
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var userId = User?.FindFirst("id")?.Value;

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();
            if (appointment.UserId != userId)
                return Unauthorized();

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("clinic/{clinicId}")]
        public async Task<ActionResult<List<AppointmentDto>>> GetAppointmentsByClinic(int clinicId)
        {
            // Check if clinic exists
            var clinicExists = await _context.Clinics.AnyAsync(c => c.ClinicId == clinicId);
            if (!clinicExists) return NotFound("Clinic not found");

            var appointments = await _context.Appointments
                .Where(a => a.ClinicId == clinicId)
                .Include(a => a.Clinic)
                .Include(a => a.Slot)
                .Include(a => a.User)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.Slot.StartTime)
                .ToListAsync();

            return Ok(appointments.Select(MapAppointmentToDto).ToList());
        }
    }
}