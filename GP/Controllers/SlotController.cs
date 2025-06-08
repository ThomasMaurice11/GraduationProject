using GP.DTOs.Slot;
using GP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static GP.DTOs.Slot.SlotDTOs;

namespace GP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Admin,ClinicAdmin")]
    public class SlotController : ControllerBase
    {
        private readonly AuthDbContext _context;

        public SlotController(AuthDbContext context)
        {
            _context = context;
        }

        private SlotDto MapSlotToDto(Slot slot)
        {
            return new SlotDto
            {
                SlotId = slot.SlotId,
                ClinicId = slot.ClinicId,
                StartTime = slot.StartTime.ToString(@"hh\:mm"),
                EndTime = slot.EndTime.ToString(@"hh\:mm")
            };
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SlotDto>>> GetSlots()
        {
            var slots = await _context.Slots.ToListAsync();
            return Ok(slots.Select(MapSlotToDto));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SlotDto>> GetSlot(int id)
        {
            var slot = await _context.Slots.FindAsync(id);
            if (slot == null) return NotFound();
            return MapSlotToDto(slot);
        }

        [HttpGet("clinic/{clinicId}")]
        public async Task<ActionResult<IEnumerable<SlotDto>>> GetSlotsByClinic(int clinicId)
        {
            var slots = await _context.Slots
                .Where(s => s.ClinicId == clinicId)
                .ToListAsync();
            return Ok(slots.Select(MapSlotToDto));
        }

        [HttpGet("available/{clinicId}")]
        public async Task<ActionResult<IEnumerable<SlotDto>>> GetAvailableSlots(
      int clinicId,
      [FromQuery] DateTime date)
        {
            // Get all booked slot IDs for this clinic on the specified date
            var bookedSlotIds = await _context.Appointments
                .Where(a => a.ClinicId == clinicId &&
                           a.AppointmentDate.Date == date.Date)
                .Select(a => a.SlotId)
                .ToListAsync();

            // Get all slots for this clinic that aren't booked
            var availableSlots = await _context.Slots
                .Where(s => s.ClinicId == clinicId &&
                           !bookedSlotIds.Contains(s.SlotId) && s.Disabled==false)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            return Ok(availableSlots.Select(MapSlotToDto));
        }

        //[HttpPost]
        //public async Task<ActionResult<SlotDto>> CreateSlot(SlotCreateDto slotDto)
        //{
        //    if (!TimeSpan.TryParseExact(slotDto.StartTime, @"hh\:mm", null, out var startTime))
        //        return BadRequest("Invalid StartTime format (HH:mm)");

        //    if (!TimeSpan.TryParseExact(slotDto.EndTime, @"hh\:mm", null, out var endTime))
        //        return BadRequest("Invalid EndTime format (HH:mm)");

        //    if (endTime <= startTime)
        //        return BadRequest("End time must be after start time");

        //    var slot = new Slot
        //    {
        //        ClinicId = slotDto.ClinicId,
        //        StartTime = startTime,
        //        EndTime = endTime
        //    };

        //    _context.Slots.Add(slot);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction(nameof(GetSlot), new { id = slot.SlotId }, MapSlotToDto(slot));
        //}

        [HttpPost]
        public async Task<ActionResult<SlotDto>> CreateSlot(SlotCreateDto slotDto)
        {
            if (!TimeSpan.TryParseExact(slotDto.StartTime, @"hh\:mm", null, out var startTime))
                return BadRequest("Invalid StartTime format (HH:mm)");

            if (!TimeSpan.TryParseExact(slotDto.EndTime, @"hh\:mm", null, out var endTime))
                return BadRequest("Invalid EndTime format (HH:mm)");

            if (endTime <= startTime)
                return BadRequest("End time must be after start time");

            // Check if there's a disabled slot with the same clinic and time
            var existingDisabledSlot = await _context.Slots
                .FirstOrDefaultAsync(s =>
                    s.ClinicId == slotDto.ClinicId &&
                    s.StartTime == startTime &&
                    s.EndTime == endTime &&
                    s.Disabled);

            if (existingDisabledSlot != null)
            {
                // Reactivate the existing slot
                existingDisabledSlot.Disabled = false;
                existingDisabledSlot.DisabledDate = null;
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetSlot),
                    new { id = existingDisabledSlot.SlotId },
                    MapSlotToDto(existingDisabledSlot));
            }

            // No matching disabled slot found - create new one
            var slot = new Slot
            {
                ClinicId = slotDto.ClinicId,
                StartTime = startTime,
                EndTime = endTime,
                //Disabled = false // Explicitly set to false
            };

            _context.Slots.Add(slot);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSlot), new { id = slot.SlotId }, MapSlotToDto(slot));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSlot(int id, SlotUpdateDto slotDto)
        {
            if (id != slotDto.SlotId) return BadRequest();

            if (!TimeSpan.TryParseExact(slotDto.StartTime, @"hh\:mm", null, out var startTime))
                return BadRequest("Invalid StartTime format");

            if (!TimeSpan.TryParseExact(slotDto.EndTime, @"hh\:mm", null, out var endTime))
                return BadRequest("Invalid EndTime format");

            var slot = await _context.Slots.FindAsync(id);
            if (slot == null) return NotFound();

            slot.ClinicId = slotDto.ClinicId;
            slot.StartTime = startTime;
            slot.EndTime = endTime;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Slots.Any(e => e.SlotId == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteSlot(int id)
        //{
        //    var slot = await _context.Slots.FindAsync(id);
        //    if (slot == null) return NotFound();

        //    _context.Slots.Remove(slot);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSlot(int id)
        {
            var slot = await _context.Slots
                .FirstOrDefaultAsync(s => s.SlotId == id);

            var slotInAppointments = await _context.Appointments
                .FirstOrDefaultAsync(s => s.SlotId == id);

            if (slot == null)
            {
                return NotFound();
            }

            if (slotInAppointments!=null)
            {
                // Slot has appointments - disable it instead of deleting
                slot.Disabled = true;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Slot disabled because it has appointments" });
            }
            else
            {
                // No appointments - delete permanently
                _context.Slots.Remove(slot);
                await _context.SaveChangesAsync();
                return NoContent();
            }
        }
    }
}