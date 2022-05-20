using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Optima.Context;
using Optima.Models.Constant;
using Optima.Models.DTO.AuthDTO;
using Optima.Models.Entities;
using Optima.Services.Interface;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Optima.Services.Implementation
{
    public class AuthenticationService : IAuthenticationService
    {
        
        private readonly IConfiguration _configuration;
                
        private readonly ApplicationDbContext _context;
        
        private readonly UserManager<ApplicationUser> _userManager;
        
        private readonly SignInManager<ApplicationUser> _signInManager;
        
        private readonly RoleManager<ApplicationUserRole> _roleManager;
        
        private readonly IEmailService _emailService;

        private readonly IEncrypt _encrypt;
        
                
        public AuthenticationService(IConfiguration configuration, ApplicationDbContext context,
            UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationUserRole> roleManager,
            IEmailService emailService, IEncrypt encrypt)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _emailService = emailService;
            _encrypt = encrypt;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }
        /// <summary>
        /// Creates the JWT token asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="userRoles">The user roles.</param>
        /// <returns>System.ValueTuple&lt;System.String, DateTime&gt;.</returns>
        public (string, DateTime) CreateJwtTokenAsync(ApplicationUser user, IList<string> userRoles)
        {
            var key = Encoding.ASCII.GetBytes(_configuration["JWT:Secret"]);

            IdentityOptions identityOptions = new IdentityOptions();

            var userClaims = new List<Claim>()
            {
                new Claim(identityOptions.ClaimsIdentity.UserIdClaimType, user.Id.ToString()),
                new Claim(identityOptions.ClaimsIdentity.UserNameClaimType, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("oid", user.Id.ToString()),
                //new Claim("LastLoginDate", user.LastLoginDate == null ? "Not set" : user.LastLoginDate.ToString()),
                //new Claim("FirstName", user.FirstName?.ToString()),
                //new Claim("LastName", user.LastName?.ToString()),
                
            };

            foreach (var userRole in userRoles)
            {
                userClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var signKey = new SymmetricSecurityKey(key);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _configuration.GetSection("JWT:ValidIssuer").Value,
                notBefore: DateTime.UtcNow,
                audience: _configuration.GetSection("JWT:ValidAudience").Value,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration.GetSection("JWT:DurationInMinutes").Value)),
                claims: userClaims,
                signingCredentials: new SigningCredentials(signKey, SecurityAlgorithms.HmacSha256));

            return (new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken), jwtSecurityToken.ValidTo);
        }

        public string BuildRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var randomNumberGenerator = RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
        public async Task<BaseResponse<bool>> UpdateUserLastLogin(string emailAddress, DateTime CurrentDate)
        {
            var result = new BaseResponse<bool>();

            var user = await _userManager.FindByEmailAsync(emailAddress);

            if (user == null)
            {
                result.ResponseMessage = ResponseMessage.ErrorMessage000;
                result.Errors.Add(ResponseMessage.ErrorMessage000);
                return result;
            };


            user.LastLoginDate = CurrentDate;
            await _context.SaveChangesAsync();

            result.Data = true;
            return result;
        }

        public async Task<BaseResponse<RegisterDTO>> Register(RegisterDTO model)
        {

            var result = new BaseResponse<RegisterDTO>();

            var user = new ApplicationUser
            {
                FirstName = model.FirstName,
                Email = model.EmailAddress,
                LastName = model.LastName,
                UserName = model.EmailAddress,
                CreationTime = DateTime.UtcNow,
                EmailConfirmed = false,
                PhoneNumber = model.PhoneNumber,
                IsAccountLocked = false,
            };

            var response = await _userManager.CreateAsync(user, model.Password);

            if (!response.Succeeded)
            {
                foreach (var error in response.Errors)
                {
                    result.Errors.Add(error.Description);
                }
                return result;
            };
            
            string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            
            var emailConfirmationLink = _emailService.GenerateEmailConfirmationLinkAsync(_encrypt.Encrypt(token), user.Email);
                                
            await _emailService.SendAccountVerificationEmail(user.Email, user.FirstName, EmailSubject.EmailConfirmation, emailConfirmationLink);

            result.Data = model;
            return result;
        }

        private ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            JwtSecurityTokenHandler tokenValidator = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JWT:Secret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var parameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateLifetime = false                
            };

            try
            {
                var principal = tokenValidator.ValidateToken(token, parameters, out var securityToken);

                if (!(securityToken is JwtSecurityToken jwtSecurityToken) || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    //_logger.LogError($"Token validation failed");
                    return null;
                }

                return principal;
            }
            catch (Exception e)
            {
               // _logger.LogError($"Token validation failed: {e.Message}");
                return null;
            }
        }

        public async Task<BaseResponse<string>> ConfirmEmail(string token, string email)
        {
            var result = new BaseResponse<string>();

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                result.ResponseMessage = ResponseMessage.ErrorMessage504;
                result.Errors.Add(ResponseMessage.ErrorMessage504);
                return result;
            };

            if (user.EmailConfirmed == true)
            {
                result.ResponseMessage = ResponseMessage.ErrorMessage501;
                result.Errors.Add(ResponseMessage.ErrorMessage501);
                result.Data = email;
                return result;
            }

            var response = await _userManager.ConfirmEmailAsync(user, _encrypt.Decrypt(token));

            if (!response.Succeeded)
            {
                foreach (var error in response.Errors)
                {
                    result.Errors.Add(error.Description);
                }
                return result;
            };

            // send email confirmation notification

            result.ResponseMessage = ResponseMessage.AccountConfirmed;
            result.Data = email;
            return result;
        }               
                
        public async Task<BaseResponse<ForgotPasswordDTO>> ForgotPassword(ForgotPasswordDTO model)
        {
            var result = new BaseResponse<ForgotPasswordDTO>();

            var user = await _userManager.FindByEmailAsync(model.EmailAddress);

            if (user == null)
            {
                result.Errors.Add(ResponseMessage.ErrorMessage504);
                return result;
            };

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var emailConfirmationLink = _emailService.GeneratePasswordResetLinkAsync(_encrypt.Encrypt(token), user.Email);
            await _emailService.SendPasswordResetEmail(user.Email, EmailSubject.PasswordReset, emailConfirmationLink);

            result.ResponseMessage = ResponseMessage.PasswordResetCodeSent;
            result.Data = model;
            return result;
        }
                
        public async Task<BaseResponse<ResetPasswordDTO>> ResetPassword(ResetPasswordDTO model)
        {
            var result = new BaseResponse<ResetPasswordDTO>();

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                result.ResponseMessage = ResponseMessage.ErrorMessage504;
                result.Errors.Add(ResponseMessage.ErrorMessage504);
                return result;
            };

            var decryptedMessage = _encrypt.Decrypt(model.Token);

            var resetResult = await _userManager.ResetPasswordAsync(user, decryptedMessage, model.Password);

            if (!resetResult.Succeeded)
            {
                foreach (var error in resetResult.Errors)
                {
                    result.Errors.Add(error.Description);
                }
                return result;
            };

            result.ResponseMessage = ResponseMessage.PasswordChanged;
            result.Data = new ResetPasswordDTO();
            return result;

            // password reset email confirmation
        }

        public async Task<BaseResponse<JwtResponseDTO>> RefreshToken(string AccessToken, string RefreshToken)
        {
            var result = new BaseResponse<JwtResponseDTO>();

            ClaimsPrincipal claimsPrincipal = GetPrincipalFromToken(AccessToken);

            if (claimsPrincipal == null) return result;

            var id = Guid.Parse(claimsPrincipal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value);      

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                result.ResponseMessage = ResponseMessage.ErrorMessage000;
                result.Errors.Add(ResponseMessage.ErrorMessage000);
                return result;
            }


            var OldToken = await _context.RefreshTokens
                    .Where(f => f.UserId == user.Id
                            && f.Token == RefreshToken
                            && f.ExpiresAt >= DateTime.Now)
                    .FirstOrDefaultAsync();

            if (OldToken == null)
            {
                result.ResponseMessage = ResponseMessage.ErrorMessage500;
                result.Errors.Add(ResponseMessage.ErrorMessage500);
                return result;
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var (token, expiration) = CreateJwtTokenAsync(user, userRoles);

           
            var refreshToken = BuildRefreshToken();

            await RemoveRefreshToken(OldToken);
            await SaveRefreshToken(new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                IssuedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.Now.AddMinutes(Convert.ToInt32(_configuration.GetSection("JWT:RefreshTokenExpiration").Value))
            });

            await SaveChanges();

            var data = new JwtResponseDTO()
            {
                Token = token,
                Expiration = expiration,
                Roles = userRoles,
                RefreshToken = refreshToken
            };

            result.Data = data;
            return result;
        }

        private async Task SaveRefreshToken(RefreshToken model)
        {
            _context.RefreshTokens.Add(model);
            await _context.SaveChangesAsync();
        }
        private async Task RemoveRefreshToken(RefreshToken model)
        {
            _context.RefreshTokens.Remove(model);
            //await _context.SaveChangesAsync();
        }
        private async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<BaseResponse<JwtResponseDTO>> Login(LoginDTO model, DateTime CurrentDateTime)
        {
            var result = new BaseResponse<JwtResponseDTO>();

            var user = await GetUser(model.EmailAddress);

            if (user == null)
            {
                result.ResponseMessage = ResponseMessage.ErrorMessage507;
                result.Errors.Add(ResponseMessage.ErrorMessage507);
                return result;
            };

            if (user.IsAccountLocked == true)
            {
                result.ResponseMessage = ResponseMessage.ErrorMessage503;
                result.Errors.Add(ResponseMessage.ErrorMessage503);
                return result;
            }

            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var response = await _userManager.IsEmailConfirmedAsync(user);

                if (response == false)
                {
                    result.Errors.Add(ResponseMessage.ErrorMessage502);
                    return result;
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                // GET USER PERMISSION
                var (token, expiration) = CreateJwtTokenAsync(user, userRoles);

                await UpdateUserLastLogin(user.Email, CurrentDateTime);

                var refreshToken = BuildRefreshToken();                

                await SaveRefreshToken(new RefreshToken
                {
                    UserId = user.Id,
                    Token = refreshToken,
                    IssuedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.Now.AddMinutes(Convert.ToInt32(_configuration.GetSection("JWT:RefreshTokenExpiration").Value))
                });

                await SaveChanges();

                var data = new JwtResponseDTO()
                {
                    Token = token,
                    Expiration = expiration,
                    Roles = userRoles,
                    RefreshToken = refreshToken
                };
                result.Data = data;
                return result;
            }

            result.ResponseMessage = ResponseMessage.ErrorMessage507;
            result.Errors.Add(ResponseMessage.ErrorMessage507);
            return result;
        }
                
        public async Task<BaseResponse<ChangePasswordDTO>> ChangePassword(ChangePasswordDTO model)
        {
            var result = new BaseResponse<ChangePasswordDTO>();

            var user = await _userManager.FindByEmailAsync(model.EmailAddress);

            if (user == null)
            {
                result.Errors.Add(ResponseMessage.ErrorMessage504);
                return result;
            };

            var changePassword = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (!changePassword.Succeeded)
            {
                foreach (var error in changePassword.Errors)
                {
                    result.Errors.Add(error.Description);
                }
                return result;
            };

            // notify change password

            result.ResponseMessage = ResponseMessage.PasswordChanged;
            result.Data = new ChangePasswordDTO();
            return result;
        }

        /// <summary>
        /// Admins the lockout user.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <returns>Task&lt;ResultModel&lt;System.String&gt;&gt;.</returns>
        public async Task<BaseResponse<string>> LockoutUser(string emailAddress)
        {
            var result = new BaseResponse<string>();

            var user = await GetUser(emailAddress);

            if (user == null)
            {
                result.ResponseMessage = ResponseMessage.ErrorMessage504;
                result.Errors.Add(ResponseMessage.ErrorMessage504);
                return result;
            };

            if (user.IsAccountLocked == true)
            {
                result.Errors.Add(ResponseMessage.ErrorMessage505);
                return result;
            }

            user.IsAccountLocked = true;
            await _context.SaveChangesAsync();

            result.Data = ResponseMessage.ErrorMessage506;
            return result;
        }
                
        public async Task<BaseResponse<string>> UnLockUser(string emailAddress)
        {
            var result = new BaseResponse<string>();

            var user = await GetUser(emailAddress);

            if (user == null)
            {
                result.ResponseMessage = ResponseMessage.ErrorMessage504;
                result.Errors.Add(ResponseMessage.ErrorMessage504);
                return result;
            };

            if (user.IsAccountLocked == false)
            {
                result.Errors.Add(ResponseMessage.ErrorMessage508);
                return result;
            }

            user.IsAccountLocked = false;
            await _context.SaveChangesAsync();

            result.Data = ResponseMessage.AccountUnlocked;
            return result;
        }
                       
        
        private async Task<ApplicationUser> GetUser(string emailAddress)
            => await _userManager.FindByEmailAsync(emailAddress);
        private async Task<string> GetUserById(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
            return user.FirstName;
        }
                
    }
}
