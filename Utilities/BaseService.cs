using log4net;
using Optima.Models.Entities;
using Optima.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Utilities
{
    public class BaseService
    {
        private readonly ILog _logger;
        private static Random random = new Random();

        public BaseService()
        {
            _logger = LogManager.GetLogger(typeof(BaseService));
        }
        public List<string> Errors { get; set; } = new List<string>();

        public string GenerateCreditDebitTransactionRef(CreditDebit lastCreditDebit, TransactionType transactionType)
        {
            if (lastCreditDebit == null)
            {
                return GenerateCode(SetTransactionPrefix(transactionType));
            };

            var transactionRefArray = lastCreditDebit.TransactionReference.Split('-'); // BEFORE SPLIT CRE-2392302 AFTER( CRE 2392302)

            double generatedRefNum;
            try
            {
                generatedRefNum = double.Parse(transactionRefArray[1]) + 1;

            }
            catch (Exception ex)
            {
                _logger.Error($"Error occured when trying to parse {lastCreditDebit.TransactionReference}..." + ex.Message);
                generatedRefNum = 000000;
            }

            return SetTransactionPrefix(transactionType) + generatedRefNum.ToString();
        }
        private string SetTransactionPrefix(TransactionType transactionType)
        {
            string prefix;
            switch (transactionType)
            {
                case TransactionType.Credit:
                    prefix = "CRE";
                    break;
                case TransactionType.Debit:
                    prefix = "DEB";
                    break;
                default:
                    prefix = "UNK";
                    break;
            }
            return prefix;
        }
        private string GenerateCode(string prefeix)
        {
            const string chars = "0123456789";

            var newTransactionRef = $"{prefeix.ToUpper()}-" + new string(Enumerable.Repeat(chars, 4)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            return newTransactionRef;
        }
    }
}
