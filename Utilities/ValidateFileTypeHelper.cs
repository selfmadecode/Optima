using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Utilities
{
    public class ValidateFileTypeHelper
    {

        public static bool ValidateFile(string[] allowedExt, string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLower();
            return allowedExt.ToList().Any(x => $".{x}".Equals(ext, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
