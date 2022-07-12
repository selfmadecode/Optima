using log4net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Optima.Context;
using Optima.Models.Constant;
using Optima.Models.DTO.AuthDTO;
using Optima.Models.DTO.UserDTOs;
using Optima.Models.Entities;
using Optima.Models.Enums;
using Optima.Services.Interface;
using Optima.Utilities;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Optima.Utilities.Helpers.PermisionProvider;

namespace Optima.Services.Implementation
{
    public class AuthenticationService : BaseService, IAuthenticationService
    {
        
        private readonly IConfiguration _configuration;
                
        private readonly ApplicationDbContext _context;
        
        private readonly UserManager<ApplicationUser> _userManager;
        
        private readonly SignInManager<ApplicationUser> _signInManager;
        
        private readonly RoleManager<ApplicationRole> _roleManager;
        
        private readonly IEmailService _emailService;

        private readonly IEncrypt _encrypt;
        private readonly ILog _logger;
        private static Random random = new Random();




        public AuthenticationService(IConfiguration configuration, ApplicationDbContext context,
            UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager,
            IEmailService emailService, IEncrypt encrypt)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _emailService = emailService;
            _encrypt = encrypt;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = LogManager.GetLogger(typeof(AuthenticationService));

        }
        
        public (string, DateTime) CreateJwtTokenAsync(ApplicationUser user, IList<string> userRoles)
        {            

            var key = Encoding.ASCII.GetBytes(_configuration["JWT:Secret"]);

            var userClaims = BuildUserClaims(user, userRoles).Result;            

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

        public async Task<List<Claim>> BuildUserClaims(ApplicationUser user, IList<string> userRoles)
        {
            IdentityOptions identityOptions = new IdentityOptions();
            var claims = await _userManager.GetClaimsAsync(user);            

            var userClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypesHelper.oid, user.Id.ToString()),
                new Claim(ClaimTypesHelper.LastLoginDate, user.LastLoginDate.ToString())
            };

            foreach (var userRole in userRoles)
            {
                userClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            foreach (var claim in claims)
            {
                userClaims.Add(new Claim(claim.Type, claim.Value));
            }

            _logger.Info($"Built User claims successfully");
            return userClaims;
        }

        public string BuildRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var randomNumberGenerator = RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetBytes(randomNumber);

                _logger.Info($"Generated Refresh Token");

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
            result.ResponseMessage = "LOGGED OUT SUCCESSFULLY";
            return result;
        }

        public async Task<BaseResponse<RegisterDTO>> Register(RegisterDTO model)
        {
            var user = new ApplicationUser
            {
                FullName = model.FullName,
                Email = model.EmailAddress,
                UserName = model.EmailAddress,
                CreationTime = DateTime.UtcNow,
                EmailConfirmed = false,
                HasAcceptedTerms = true,
                PhoneNumber = model.PhoneNumber,
                IsAccountLocked = false,
                UserType = UserTypes.USER
            };

            var response = await _userManager.CreateAsync(user, model.Password);

            if (!response.Succeeded)
            {
                var errors = new List<string>();

                foreach (var error in response.Errors)
                {
                    errors.Add(error.Description);
                }

                return new BaseResponse<RegisterDTO>(ResponseMessage.AccountCreationFailure, errors);
            };
            
            string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var emailConfirmationLink = _emailService.GenerateEmailConfirmationLinkAsync(_encrypt.Encrypt(token), user.Email);

            _logger.Info("SENDING CONFIRMATION LINK...");

            await _emailService.SendAccountVerificationEmail(user.Email, user.FullName, EmailSubject.EmailConfirmation, emailConfirmationLink);

            await CreateUserWalletBalance(user.Id);

            model.Password = "";

            return new BaseResponse<RegisterDTO>(model, ResponseMessage.AccountCreationSuccess);
        }

        private async Task CreateUserWalletBalance(Guid UserId)
        {
            var newUserAccountBalance = new WalletBalance
            {
                UserId = UserId
            };

            _context.WalletBalance.Add(newUserAccountBalance);
            await _context.SaveChangesAsync();

            _logger.Info("CREATED AND SAVED USER WALLET...");
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
                    _logger.Info($"Token validation failed");
                    return null;
                }

                return principal;
            }
            catch (Exception ex)
            {
               _logger.Info($"Token validation failed: {ex.Message}");
                return null;
            }
        }

        public async Task<BaseResponse<string>> ConfirmEmail(string token, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                Errors.Add(ResponseMessage.ErrorMessage504);
                return new BaseResponse<string>(ResponseMessage.ErrorMessage504, Errors);
            };

            if (user.EmailConfirmed == true)
            {
                return new BaseResponse<string>(email, ResponseMessage.AccountConfirmed);
            }

            var response = await _userManager.ConfirmEmailAsync(user, _encrypt.Decrypt(token));

            if (!response.Succeeded)
            {
                foreach (var error in response.Errors)
                {
                    Errors.Add(error.Description);
                }
                return new BaseResponse<string>(ResponseMessage.ErrorMessage504, Errors);
            };

            await _emailService.SendAccountConfirmationEmail(email, user.FullName);
            _logger.Info("Sending Account confirmation email notification....");
                        
            return new BaseResponse<string>(email, ResponseMessage.AccountConfirmed);
        }

        public async Task<BaseResponse<ForgotPasswordDTO>> ForgotPassword(ForgotPasswordDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.EmailAddress);

            if (user == null)
            {
                Errors.Add(ResponseMessage.ErrorMessage504);
                return new BaseResponse<ForgotPasswordDTO>(ResponseMessage.ErrorMessage504, Errors);
            };

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var emailConfirmationLink = _emailService.GeneratePasswordResetLinkAsync(_encrypt.Encrypt(token), user.Email);
            await _emailService.SendPasswordResetEmail(user.Email, EmailSubject.PasswordReset, emailConfirmationLink);

            return new BaseResponse<ForgotPasswordDTO>(model, ResponseMessage.PasswordResetCodeSent);
        }
                
        public async Task<BaseResponse<ResetPasswordDTO>> ResetPassword(ResetPasswordDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                Errors.Add(ResponseMessage.ErrorMessage504);
                return new BaseResponse<ResetPasswordDTO>(ResponseMessage.ErrorMessage504, Errors);
            };

            var decryptedMessage = _encrypt.Decrypt(model.Token);

            var resetResult = await _userManager.ResetPasswordAsync(user, decryptedMessage, model.Password);

            if (!resetResult.Succeeded)
            {
                foreach (var error in resetResult.Errors)
                {
                    Errors.Add(error.Description);
                }
                return new BaseResponse<ResetPasswordDTO>(ResponseMessage.ErrorMessage999, Errors);
            };

            return new BaseResponse<ResetPasswordDTO>(new ResetPasswordDTO(), ResponseMessage.PasswordChanged);
        }

        public async Task<BaseResponse<JwtResponseDTO>> RefreshToken(string AccessToken, string RefreshToken)
        {
            ClaimsPrincipal claimsPrincipal = GetPrincipalFromToken(AccessToken);

            if (claimsPrincipal == null)
            {
                Errors.Add(ResponseMessage.RefreshTokenFailure);
                return new BaseResponse<JwtResponseDTO>(ResponseMessage.RefreshTokenFailure, Errors);
            }

            var id = Guid.Parse(claimsPrincipal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value);      

            var user = await GetUserById(id);
            if (user == null)
            {
                Errors.Add(ResponseMessage.ErrorMessage000);
                return new BaseResponse<JwtResponseDTO>(ResponseMessage.ErrorMessage000, Errors);
            }

            var OldToken = await GetRefreshToken(user.Id, RefreshToken);
            if (OldToken == null)
            {                
                Errors.Add(ResponseMessage.ErrorMessage500);
                return new BaseResponse<JwtResponseDTO>(ResponseMessage.ErrorMessage500, Errors);
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

            return new BaseResponse<JwtResponseDTO>(data);
        }

        private async Task<RefreshToken> GetRefreshToken(Guid UserId, string refreshToken)
        {
            return await _context.RefreshTokens
                .Where(f => f.UserId == UserId && f.Token == refreshToken && f.ExpiresAt >= DateTime.Now)
                .FirstOrDefaultAsync();
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
            var user = await GetUser(model.EmailAddress);
            if (user == null)
            {
                Errors.Add(ResponseMessage.ErrorMessage507);
                return new BaseResponse<JwtResponseDTO>(ResponseMessage.ErrorMessage507, Errors);
            };

            if (user.IsAccountLocked == true)
            {
                Errors.Add(ResponseMessage.ErrorMessage503);
                return new BaseResponse<JwtResponseDTO>(ResponseMessage.ErrorMessage503, Errors);
            }                       

            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var response = await _userManager.IsEmailConfirmedAsync(user);

                if (response == false)
                {
                    _logger.Info($"{user.Email} is not yet confirmed");
                    Errors.Add(ResponseMessage.ErrorMessage502);
                    return new BaseResponse<JwtResponseDTO>(ResponseMessage.ErrorMessage502, Errors);
                }

                // IF THE USER HASN'T ACCEPTED TERMS AND CONDITION
                if (user.HasAcceptedTerms == false)
                {
                    _logger.Info($"{user.Email} has not accepted terms and condition");
                    Errors.Add(ResponseMessage.ErrorMessage509);
                    return new BaseResponse<JwtResponseDTO>(ResponseMessage.ErrorMessage509, Errors);
                }

                // TODO: GET USER PERMISSIONS
                _logger.Info($"Getting user roles: ...{user.Email}");

                var userRoles = await _userManager.GetRolesAsync(user);

                var (token, expiration) = CreateJwtTokenAsync(user, userRoles);

                await UpdateUserLastLogin(user.Email, CurrentDateTime);

                var refreshToken = BuildRefreshToken();                

                await SaveRefreshToken(new RefreshToken
                {
                    UserId = user.Id,
                    Token = refreshToken,
                    IssuedAt = DateTime.UtcNow,
                    //ExpiresAt = DateTime.Now.AddMinutes(Convert.ToInt32(_configuration.GetSection("JWT:RefreshTokenExpiration").Value))
                    ExpiresAt = expiration
                });

                await SaveChanges();
                var claims = await _userManager.GetClaimsAsync(user);

                var data = new JwtResponseDTO()
                {
                    Token = token,
                    Expiration = expiration,
                    Roles = userRoles,
                    RefreshToken = refreshToken,
                    Permissions = claims.Select(x => x.Value).ToList(),
                    //FullName = user.FullName,
                    //UserName = user.UserName
                };

                return new BaseResponse<JwtResponseDTO>(data);
            }

            Errors.Add(ResponseMessage.ErrorMessage507);
            return new BaseResponse<JwtResponseDTO>(ResponseMessage.ErrorMessage507, Errors);
        }

        public async Task<BaseResponse<ChangePasswordDTO>> ChangePassword(ChangePasswordDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.EmailAddress);
            if (user == null)
            {
                Errors.Add(ResponseMessage.ErrorMessage504);
                return new BaseResponse<ChangePasswordDTO>(ResponseMessage.ErrorMessage504, Errors);
            };

            var changePassword = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (!changePassword.Succeeded)
            {
                foreach (var error in changePassword.Errors)
                {
                    Errors.Add(error.Description);
                }                
                return new BaseResponse<ChangePasswordDTO>(ResponseMessage.PasswordChangedFailure, Errors);
            };

            return new BaseResponse<ChangePasswordDTO>(new ChangePasswordDTO(), ResponseMessage.PasswordChanged);
        }

        /// <summary>
        /// Admins the lockout user.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <returns>Task&lt;ResultModel&lt;System.String&gt;&gt;.</returns>
        public async Task<BaseResponse<string>> LockoutUser(string emailAddress)
        {
            var user = await GetUser(emailAddress);
            if (user == null)
            {
                Errors.Add(ResponseMessage.ErrorMessage504);
                return new BaseResponse<string>(ResponseMessage.ErrorMessage504, Errors);
            };

            if (user.IsAccountLocked == true)
            {
                Errors.Add(ResponseMessage.ErrorMessage505);
                return new BaseResponse<string>(ResponseMessage.ErrorMessage505, Errors);
            }

            user.IsAccountLocked = true;
            await _context.SaveChangesAsync();

            _logger.Info("Sending User blocked email notification....");
            await _emailService.SendAccountBlockedEmail(user.Email, user.FullName);

            return new BaseResponse<string>(ResponseMessage.ErrorMessage506);
        }
                
        public async Task<BaseResponse<string>> UnLockUser(string emailAddress)
        {
            var user = await GetUser(emailAddress);
            if (user == null)
            {
                Errors.Add(ResponseMessage.ErrorMessage504);
                return new BaseResponse<string>(ResponseMessage.ErrorMessage504, Errors);
            };

            if (user.IsAccountLocked == false)
            {
                Errors.Add(ResponseMessage.ErrorMessage508);
                return new BaseResponse<string>(ResponseMessage.ErrorMessage508, Errors);
            }

            user.IsAccountLocked = false;
            await _context.SaveChangesAsync();

            _logger.Info("Sending User unblocked email notification....");
            await _emailService.SendAccountUnBlockedEmail(user.Email, user.FullName);

            return new BaseResponse<string>(ResponseMessage.AccountUnlocked);
        }
                       
        
        private async Task<ApplicationUser> GetUser(string emailAddress)
            => await _userManager.FindByEmailAsync(emailAddress);
        private async Task<ApplicationUser> GetUserById(Guid id)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<BaseResponse<AdminDTO>> CreateAdmin(CreateAdminAccountDTO model)
        {
            var user = new ApplicationUser
            {
                UserType = UserTypes.ADMIN,
                FullName = $"{model.FirstName} {model.LastName}",
                NormalizedUserName  = model.EmailAddress.ToLower(),
                UserName = model.EmailAddress.ToLower(),
                EmailConfirmed = false,
                IsAccountLocked = false,
                PhoneNumber = model.PhoneNumber,
                CreationTime = DateTime.UtcNow,
                Email = model.EmailAddress,
                HasAcceptedTerms = true         
            };

            var password = GeneratePassword();
            var response = await _userManager.CreateAsync(user, password);
            // AN ERROR OCCURED WHILE CREATING USER

            if (!response.Succeeded)
            {
                foreach (var error in response.Errors)
                {
                    Errors.Add(error.Description);
                }
                return new BaseResponse<AdminDTO>(Errors.FirstOrDefault(), Errors);
            };

            await AssignPermissionAsync(user.Id, model.Permissions);

            string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var emailConfirmationLink = _emailService.GenerateEmailConfirmationLinkAsync(_encrypt.Encrypt(token), user.Email);

            _logger.Info("SENDING CONFIRMATION LINK...");

            // SEND PASSWORD VIA EMAIL
            await _emailService.SendAdminAccountVerificationEmail(
                user.Email, user.FullName, EmailSubject.EmailConfirmation,
                emailConfirmationLink, password);

            var data = new AdminDTO
            { 
                FullName = user.FullName, 
                DateCreated = user.CreationTime,
                UserName = user.UserName 
            };
            
            return new BaseResponse<AdminDTO>(data, ResponseMessage.CreatedAdmin);
        }

        public async Task<BaseResponse<UpdateClaimDTO>> UpdateClaimsAsync(UpdateClaimDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.EmailAddress);

            if (user == null)
            {
                Errors.Add(ResponseMessage.ErrorMessage504);
                return new BaseResponse<UpdateClaimDTO>(ResponseMessage.ErrorMessage000, Errors);
            };

            //var claims = await _userManager.GetClaimsAsync(user);

            var claims = _context.UserClaims.Where(x => x.UserId == user.Id);
            _context.UserClaims.RemoveRange(claims);
            _context.SaveChanges();

            await AssignPermissionAsync(user.Id, model.Permissions);

            return new BaseResponse<UpdateClaimDTO>(model, ResponseMessage.UpdateAdminClaim);
        }

        public async Task<BaseResponse<AdminDetailsDTO>> GetAdminDetailsAndPermmissionsAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                Errors.Add(ResponseMessage.ErrorMessage504);
                return new BaseResponse<AdminDetailsDTO>(ResponseMessage.ErrorMessage000, Errors);
            };

            var claims = await _userManager.GetClaimsAsync(user);

            var data = new AdminDetailsDTO
            {
                EmailAddress = user.Email,
                Name = user.FullName,
                PhoneNumber = user.PhoneNumber,
                UserId = user.Id,
                Permissions = claims.Select(x => x.Value).ToList()
            };

            return new BaseResponse<AdminDetailsDTO>(data);
        }

        private async Task AssignPermissionAsync(Guid UserId, IList<string> Permmissions)
        {
            var listOfClaims = new List<ApplicationUserClaim>();

            foreach (var permmission in Permmissions)
            {
                var claim = new ApplicationUserClaim
                {
                    UserId = UserId,
                    ClaimValue = permmission,
                    ClaimType = nameof(Permission)
                };
                listOfClaims.Add(claim);
            }

            await _context.UserClaims.AddRangeAsync(listOfClaims);
            _context.SaveChanges();
        }

        private string GeneratePassword()
        {
            var numbers = "982345173";
            var capital = "AQWSDETHFUJNGF";
            var small = "adginfhy";
            var specialChar = "#$%&(";

            var randomNum = new string(Enumerable.Repeat(numbers, 5)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            var randomcapital = new string(Enumerable.Repeat(capital, 1)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            var randomsmall = new string(Enumerable.Repeat(small, 2)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            var randomspecialChar = new string(Enumerable.Repeat(specialChar, 1)
              .Select(s => s[random.Next(s.Length)]).ToArray());


            return randomNum + randomspecialChar + randomsmall + randomcapital;
        }        
    }
}
