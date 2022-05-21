using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.Config
{
    public static class EmailTemplateUrl
    {
        public const string Test = @"Filestore\EmailTemplate\Test.html";
        public const string AccountVerificationTemplate = @"Filestore\EmailTemplate\Confirm_email.html";
        public const string PasswordResetTemplate = @"Filestore\EmailTemplate\Reset_password.html";
        public const string AccountConfirmationTemplate = @"Filestore\EmailTemplate\AccountConfirmation.html";
        public const string AccountBlockedTemplate = @"Filestore\EmailTemplate\AccountBlocked.html";
        public const string AccountUnBlockedTemplate = @"Filestore\EmailTemplate\AccountUnBlocked.html";
    }
}
