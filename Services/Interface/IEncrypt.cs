using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Interface
{
    public interface IEncrypt
    {
        public string Encrypt(string message);

        public string Decrypt(string cipherText);
    }
}
