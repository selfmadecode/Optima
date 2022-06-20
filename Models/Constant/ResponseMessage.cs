﻿using System;
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
        public const string PasswordChangedFailure = "PASSWORD CHANGED FAILED";
        public const string AccountUnlocked = "ACCOUNT UNLOCKED!";
        public const string DebitRequestSuccess = "DEBIT REQUEST SUBMITED";
        public const string AccountCreationSuccess = "ACCOUNT CREATED SUCCESSFULLY";
        public const string AccountCreationFailure = "ACCOUNT CREATION FAILED";

        public const string RefreshTokenFailure = "USER NOT FOUND";


        public const string TermsAndConditionUpdate = "TERMS AND CONDITION UPDATED SUCCESSFULLY";
        public const string AcceptedTermsAndCondition = "TERMS ACCEPTED SUCCESSFULLY";

        public const string MaxAccountError = "YOU CAN ONLY HAVE A MAXIMUMOF TWO ACCOUNTS";
        public const string BankAccountCreated = "BANK ACCOUNT CREATED";
        public const string BankAccountNotFound = "BANK ACCOUNT NOT FOUND";
        public const string BankAccountDeleted = "BANK ACCOUNT DELETED SUCCESSFULLY";
        public const string BankAccountUpdated = "BANK ACCOUNT UPDATED SUCCESSFULLY";

        public const string CardCreationFailure = "CARD CREATION FAILED";
        public const string CountryNotFound = "COUNTRY NOT FOUND";
        public const string CountryNotDistinct = "SAME COUNTRY SELECTED MORE THAN ONCE OR COUNTRY NOT FOUND";

        public const string CardExist = "CARD WITH SAME NAME ALREADY EXIST";
        public const string CardCreation = "CARD CREATED SUCCESSFULLY";


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
        public const string ErrorMessage601 = "USER ACCOUNT BALANCE NOT FOUND";

        public const string ErrorMessage999 = "SOMETHING WENT WRONG";

        public const string SuccessMessage000 = "SUCCESSFUL";
        public const string FailureMessage000 = "FAILED";

    }
}
