using GreenBasket.Application.DTOs.Auth;
using GreenBasket.Application.Interfaces;
using GreenBasket.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GreenBasket.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthService(UserManager<AppUser> userManager, IConfiguration configuration, IEmailService emailService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO model)
        {
            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Registration failed: {errors}");
            }

            // Assign default Customer role to newly registered account
            await _userManager.AddToRoleAsync(user, "Customer");

            // Generate OTP (6-digit by default using our EmailTokenProvider)
            var otp = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
            
            // Send Email
            string emailBody = $@"
                <h2>Welcome to Green Basket!</h2>
                <p>Your OTP for email verification is: <strong>{otp}</strong></p>
                <p>Please enter this code in the application to verify your account.</p>
            ";
            await _emailService.SendEmailAsync(user.Email, "Green Basket - Verify Your Email", emailBody);

            // Return success without JWT token since email is not verified yet
            return new AuthResponseDTO
            {
                Token = "PendingVerification", // Indicate to frontend that OTP is needed
                UserId = user.Id,
                Email = user.Email!,
                FullName = user.FullName
            };
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                throw new Exception("Invalid email or password.");

            if (!await _userManager.IsEmailConfirmedAsync(user))
                throw new Exception("Please verify your email before logging in.");

            if (!await _userManager.CheckPasswordAsync(user, model.Password))
                throw new Exception("Invalid email or password.");

            var token = await GenerateJwtToken(user);

            return new AuthResponseDTO
            {
                Token = token,
                UserId = user.Id,
                Email = user.Email!,
                FullName = user.FullName
            };
        }

        private async Task<string> GenerateJwtToken(AppUser user)
        {
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public async Task<bool> VerifyEmailAsync(string email, string otp)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) throw new Exception("User not found.");

            var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", otp);
            if (isValid)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
                return true;
            }
            return false;
        }

        public async Task<bool> ResendOtpAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) throw new Exception("User not found.");
            if (user.EmailConfirmed) throw new Exception("Email is already verified.");

            var otp = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
            
            string emailBody = $@"
                <h2>Green Basket</h2>
                <p>Your new OTP for email verification is: <strong>{otp}</strong></p>
            ";
            await _emailService.SendEmailAsync(user.Email!, "Green Basket - Your new OTP", emailBody);

            return true;
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return true; // Don't reveal user existence

            var otp = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
            
            string emailBody = $@"
                <h2>Green Basket - Password Reset</h2>
                <p>You requested a password reset. Your OTP is: <strong>{otp}</strong></p>
                <p>If you didn't request this, you can safely ignore this email.</p>
            ";
            await _emailService.SendEmailAsync(user.Email!, "Green Basket - Password Reset OTP", emailBody);

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) throw new Exception("Invalid request.");

            var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", model.Otp);
            if (!isValid) throw new Exception("Invalid or expired OTP.");

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Password reset failed: {errors}");
            }
            
            return true;
        }
    }
}