using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Optima.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Implementation
{
    public class EncryptService : IEncrypt
    {
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IConfiguration _configuration;
        private string Key;
        public EncryptService(IDataProtectionProvider dataProtectionProvider, IConfiguration configuration)
        {
            _dataProtectionProvider = dataProtectionProvider;
            Key = configuration["Encyption:Key"];
        }
        public string Decrypt(string cipherText)
        {
            var protector = _dataProtectionProvider.CreateProtector(Key);
            return protector.Unprotect(cipherText);
        }

        public string Encrypt(string input)
        {
            var protector = _dataProtectionProvider.CreateProtector(Key);
            return protector.Protect(input);
        }
    }
}
