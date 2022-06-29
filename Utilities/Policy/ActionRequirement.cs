using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Utilities.Policy
{
    public class ActionRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public string Status { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionRequirement"/> class.
        /// </summary>
        /// <param name="status">The status.</param>
        public ActionRequirement(string status)
        {
            Status = status;
        }
    }
}
