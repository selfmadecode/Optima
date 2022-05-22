using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.Constant
{
    public class ResponseMessage
    {
        public const string NotificationNotFound = "NOTIFICATION NOT FOUND!";
        public const string NotificationUpdate = "NOTIFICATION UPDATED SUCCESSFULLY";
        public const string NotificationAdded = "NOTIFICATION SUCCESSFULLY ADDED";
        public const string AccountConfirmed = "ACCOUNT SUCCESSFULLY CONFIRMED!";
        public const string PasswordResetCodeSent = "PASSWORD RESET CODE SENT!";
        public const string PasswordChanged = "PASSWORD CHANGED SUCCESSFULLY!";
        public const string AccountUnlocked = "ACCOUNT UNLOCKED!";

        public const string ErrorMessage000 = "USER NOT FOUND!";

        public const string ErrorMessage500 = "REFRESH TOKEN NOT FOUND!";
        public const string ErrorMessage501 = "ACCOUNT ALREADY VERIFIED!";
        public const string ErrorMessage502 = "EMAIL NOT CONFIRMED, KINDLY CONFIRM YOUR ACCOUNT";
        public const string ErrorMessage503 = "ACCOUNT IS LOCKED, CONTACT ADMIN";
        public const string ErrorMessage504 = "INCORRECT USERNAME";
        public const string ErrorMessage505 = "ACCOUNT ALREADY LOCKED";
        public const string ErrorMessage506 = "ACCOUNT LOCKED";
        public const string ErrorMessage507 = "INCORRECT USERNAME OR PASSWORD";
        public const string ErrorMessage508 = "ACCOUNT IS NOT LOCKED";
        public const string ErrorMessage509 = "USER HAS NOT ACCEPTED TERMS AND CONDITION";

        public const string ErrorMessage600 = "USER ACCOUNT NOT FOUND";

        public const string SuccessMessage000 = "SUCCESSFUL";

    }
}
