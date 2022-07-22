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

        public const string CountryNotFound = "COUNTRY NOT FOUND";
        public const string CountryCreated = "COUNTRY CREATED SUCCESSFULLY";
        public const string CountryNotDistinct = "SAME COUNTRY SELECTED MORE THAN ONCE OR COUNTRY NOT FOUND";
        public const string CountryAlreadyExist = "COUNTRY ALREADY EXIST";
        public const string CountryDoesNotExist = "COUNTRY DOES NOT EXIST";
        public const string CountryCannotBeDeleted = "COUNTRY CANNOT BE DELETED";
        public const string CountryDeleted = "COUNTRY DELETED SUCCESSFULLY";

        public const string CardExist = "GIFT CARD WITH SAME NAME ALREADY EXIST";
        public const string CardCreation = "GIFT CARD CREATED SUCCESSFULLY";
        public const string CardUpdate = "GIFT CARD UPDATED SUCCESSFULLY";
        public const string CardNotFound = "GIFT CARD NOT FOUND";
        public const string CardTypeNotFound = "ONE OR MORE CARDTYPE NOT FOUND FOR THIS CARD";
        public const string CardCountryTypeNotFound = "SAME COUNTRY SELECTED FOR THIS CARDTYPE OR COUNTRY NOT FOUND";
        public const string CardCountryExist = "SELECTED COUNTRY ALREADY EXIST FOR THIS CARD";
        public const string CardTypeConfigured = "CARDTYPE HAS ALREADY BEEN CONFIGURED";
        public const string CardTypeDenominationNotFound = "DENOMINATION(s) DOES NOT EXIST OR DUPLICATE DENOMINATION(s) SELECTED.";
        public const string CardTypeDenominationForCardNotFound = "DENOMINATION(s) DOES NOT EXIST";
        public const string CardConfigSuccess = "CONFIGURED CARD SUCCESSFULLY";
        public const string CardReceiptNotFound = "RECEIPT DOES NOT EXISTS";
        public const string CannotDeleteCard = "CANNOT DELETE CARD";
        public const string DeleteCardType = "CARDTYPE DELETED SUCCESSFULLY";
        public const string CardCreationFailure = "GIFT CARD CREATION FAILED";
        public const string UpdateCardStatus = "CARD STATUS UPDATED SUCCESSFULLY";


        public const string MinAmountError = "MINIMUM WITHDRAWAL IS 500";
        public const string InsufficientError = "INSUFFICIENT FUNDS!";


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


        public const string CardSaleCreation = "SUCCESSFULLY CREATED YOUR GIFT CARD FOR SALE";

        public const string UserUpdate = "SUCCESSFULLY UPDATED YOUR INFORMATION";



        public const string VisaPrefixExist = "VISA PREFIX ALREADY EXIST";
        public const string VisaPrefixCreation = "VISA PREFIX CREATED SUCCESSFULLY";
        public const string VisaPrefixNotFound = "VISA PREFIX DOESN'T EXIST";
        public const string CannotDeleteVisaPrefix = "CANNOT DELETE VISA PREFIX";
        public const string DeleteVisaPrefix = "VISA PREFIX DELETED SUCCCESSFULLY";
        public const string CardNotSpecial = "CARD NOT A SPECIAL PREFIX TYPE";


        public const string UserCardTransactionNotFound = "CARD TRANSACTION DOESN'T EXISTS FOR THIS USER";
        public const string CardTransactionNotFound = "CARD TRANSACTION NOT FOUND";
        public const string CardSoldNotFound = "CARD SOLD NOT FOUND";
        public const string CardCodesNotFound = "CARD CODE(s) DOESN'T EXISTS FOR THIS CARD TRANSACTION";
        public const string CardCodeUpdate = "SUCCESSFULLY UPDATED THE CARD CODES";
        public const string CardTransactionUpdate = "SUCCESSFULLY UPDATED THE CARD TRANSACTION";
        public const string CardNotRegular = "CARD NOT A REGULAR TYPE";


        public const string ReceiptExist = "RECEIPT TYPE ALREADY EXIST";
        public const string ReceiptCreated = "SUCCESSFULLY CREATED THE RECEIPT TYPE";
        public const string ReceiptNotFound = "RECEIPT TYPE NOT FOUND";
        public const string ReceiptDeleted = "RECEIPT TYPE DELETED SUCCESSFULLY";
        public const string ReceiptUpdated = "RECEIPT TYPE UPDATED SUCCESSFULLY";
        public const string CannotDeleteReceipt = "CANNOT DELETE RECEIPT TYPE";
        public const string CardNotAmazon = "CARD NOT AN AMAZON TYPE";


        public const string DeviceTokenNotFound = "DEVICE TOKEN DOESN'T EXISTS FOR THIS USER";
        public const string DeviceTokenDeleted = "SUCCESSFULLY DELETED THE DEVICE TOKEN";
        public const string DeviceTokenCreated = "SUCCESSFULLY CREATED THE DEVICE TOKEN";
        public const string DeviceTokenUpdated = "SUCCESSFULLY UPDATED THE USER DEVICE TOKEN";
        public const string NotificationSent = "NOTIFICATION SENT SUCCESSFULLY";
        public const string NotificationNotSent = "FAILED TO SEND THE NOTIFICATION";


        public const string CreditDebitNotFound = "CREDIT DEBIT TRANSACTION NOT FOUND";
        public const string DebitUpdated = "DEBIT TRANSACTION UPDATED SUCCESSFULLY";

        public const string CreateFaq = "FAQ CREATED SUCCESSFULLY";
        public const string UpdateFaq = "FAQ UPDATED SUCCESSFULLY";
        public const string DeleteFaq = "FAQ DELETED SUCCESSFULLY";
        public const string FaqNotFound = "FAQ DOESN'T EXISTS";


        public const string CreateDenomination = "DENOMINATION CREATED SUCCESSFULLY";
        public const string UpdateDenomination = "DENOMINATION UPDATED SUCCESSFULLY";
        public const string DenominationAlreadyExists = "DENOMINATION ALREADY EXISTS";
        public const string DenominationNotExists = "DENOMINATION DOESN'T EXISTS";
        public const string DeleteDenomination = "DENOMINATION DELETED SUCCESSFULLY";
        public const string DenominationCannotBeDeleted = "DENOMINATION CANNOT BE DELETED";

        public const string CreatedAdmin = "ADMIN CREATED SUCCESSFULLY";
        public const string UpdateAdminClaim = "ADMIN PERMMISSION UPDATED";


    }
}
