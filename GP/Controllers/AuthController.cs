using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GP.Models;
using GP.Services;
using GP.DTOs.Auth;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;


[Route("api/[controller]")]

[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtTokenService _jwtTokenService;
    private readonly IEmailService _emailService;

    public AuthController(UserManager<ApplicationUser> userManager, JwtTokenService jwtTokenService, IEmailService emailService)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _emailService = emailService;
    }

    [HttpPost("RegisterUser")]
    public async Task<IActionResult> Register([FromBody] RegisterUserModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber 
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                return Ok("User registered successfully!");
            }

            return BadRequest(result.Errors);
        }
        return BadRequest("Invalid registration details");
    }
    [HttpPost("RegisterAdmin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser { UserName = model.Username, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                return Ok("User registered successfully!");
            }

            return BadRequest(result.Errors);
        }
        return BadRequest("Invalid registration details");
    }




    [HttpPost("RegisterDoctor")]
    public async Task<IActionResult> RegisterDoctor([FromBody] DoctorRegisterModel model)
    {
        if (ModelState.IsValid)
        {
            var doctor = new Doctor
            {
                UserName = model.Username,
                Email = model.Email,
                Specialization = model.Specialization,
                Number = model.Number,
                MedicalId = model.MedicalId,
                
            };

            var result = await _userManager.CreateAsync(doctor, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(doctor, "Doctor");
                return Ok("Doctor registered successfully!");
            }

            return BadRequest(result.Errors);
        }
        return BadRequest("Invalid registration details");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByNameAsync(model.Username);

            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                // Check if the user is a doctor
                var isDoctor = await _userManager.IsInRoleAsync(user, "Doctor");

                if (isDoctor)
                {
                    // Cast the user to Doctor to access the Status property
                    var doctor = user as Doctor;

                    if (doctor == null)
                    {
                        return Unauthorized("Invalid user type.");
                    }

                    // Check the doctor's status
                    switch (doctor.Status)
                    {
                        case "Accepted":
                            var token = await _jwtTokenService.GenerateToken(user);
                            return Ok(new { Token = token });

                        case "Pending":
                            return BadRequest("We are reviewing your registration and will contact you soon.");

                        case "Rejected":
                            return BadRequest("Your registration has been rejected. Please contact the admin.");

                        default:
                            return BadRequest("Invalid status.");
                    }
                }
                else
                {
                    // For non-doctor users, generate the token as usual
                    var token = await _jwtTokenService.GenerateToken(user);
                    return Ok(new { Token = token });
                }
            }

            return Unauthorized("Invalid username or password.");
        }

        return BadRequest("Invalid login details.");
    }
    [HttpGet("getUserById/{id}")]
   
    public async Task<IActionResult> GetUserById(string id)
    {
        var user = await _jwtTokenService.GetUserData(id);
        if (user == null)
        {
            return NotFound("User not found");
        }

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new
        {
            user.Id,
            user.UserName,
            user.Email,
            Roles = roles
        });
    }

    [HttpGet("getUserFromToken")]
    [Authorize]
    public async Task<IActionResult> GetUserFromToken()
    {
        var userId = User.FindFirst("id")?.Value;
        if (userId == null)
        {
            return Unauthorized("Invalid token");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound("User not found");
        }

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new
        {
            user.Id,
            user.UserName,
            user.Email,
            Roles = roles
        });
    }




    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return BadRequest("User not found");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var resetLink = Url.Action("ResetPassword", "Auth", new { email = user.Email, token = encodedToken }, Request.Scheme);

        // Send email
        await _emailService.SendEmailAsync(user.Email, "Reset Password", $"Reset your password using this link: {resetLink}");

        return Ok("Password reset link has been sent to your email.");
    }


    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return BadRequest("User not found");

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
        var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

        if (result.Succeeded)
            return Ok("Password has been reset successfully.");

        return BadRequest(result.Errors);
    }



    //[HttpPost("forgot-password")]
    //public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
    //{
    //    if (!ModelState.IsValid)
    //        return BadRequest("Invalid email address");

    //    var user = await _userManager.FindByEmailAsync(model.Email);
    //    if (user == null)
    //        return Ok("If your email is registered, you will receive a password reset link."); // Don't reveal whether user exists

    //    var token = await _userManager.GeneratePasswordResetTokenAsync(user);

    //    // In a real application, you should encode the token for URL safety
    //    var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

    //    var resetLink = $"http://localhost:3000/reset-password?email={model.Email}&token={encodedToken}";

    //    var emailBody = $"Please reset your password by clicking here: <a href='{resetLink}'>link</a>";

    //    try
    //    {
    //        await _emailService.SendEmailAsync(model.Email, "Reset Your Password", emailBody);
    //        return Ok("Password reset link has been sent to your email.");
    //    }
    //    catch (Exception ex)
    //    {
    //        return StatusCode(500, $"Failed to send email: {ex.Message}");
    //    }
    //}

    //[HttpPost("reset-password")]
    //public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
    //{
    //    if (!ModelState.IsValid)
    //        return BadRequest(ModelState);

    //    var user = await _userManager.FindByEmailAsync(model.Email);
    //    if (user == null)
    //        return BadRequest("Invalid email address");

    //    var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));

    //    var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

    //    if (result.Succeeded)
    //    {
    //        // Optionally, you might want to send a confirmation email here
    //        return Ok("Password has been reset successfully.");
    //    }

    //    return BadRequest(result.Errors);
    //}


}