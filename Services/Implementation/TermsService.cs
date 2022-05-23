using Microsoft.EntityFrameworkCore;
using Optima.Context;
using Optima.Models.Constant;
using Optima.Models.DTO.TermsDTO;
using Optima.Services.Interface;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Implementation
{
    public class TermsService : ITermsService
    {
        private readonly ApplicationDbContext _context;

        public TermsService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<BaseResponse<bool>> AcceptTermsAndCondition(Guid UserId)
        {
            var result = new BaseResponse<bool>();

            var user = _context.Users.FirstOrDefault(x => x.Id == UserId);

            if (user == null)
            {
                result.ResponseMessage = ResponseMessage.ErrorMessage000;
                result.Errors.Add(ResponseMessage.ErrorMessage000);
                return result;
            };

            user.HasAcceptedTerms = true;
            await  _context.SaveChangesAsync();

            result.ResponseMessage = "TERMS ACCEPTED";
            return result;
        }

        public async Task<BaseResponse<GetRateDTO>> GetTermsAndCondition()
        {
            var result = new BaseResponse<GetRateDTO>();

            var termsAndCondition = await _context.TermsAndConditions.FirstOrDefaultAsync();

            if(termsAndCondition == null)
            {
                return result;
            }

            var termsDto = new GetRateDTO
            {
                DateCreated = termsAndCondition.CreatedOn,
                DateModified = termsAndCondition.ModifiedOn,
                Id = termsAndCondition.Id,
                TermsAndCondition = termsAndCondition.TermsAndConditions
            };

            result.Data = termsDto;
            return result;
        }

        public async Task<BaseResponse<Guid>> UpdateTermsAndCondition(Guid modifiedBy, UpdateTermsDTO model)
        {
            var result = new BaseResponse<Guid>();

            var termsAndCondition = await _context.TermsAndConditions.FirstOrDefaultAsync();

            if (termsAndCondition == null)
            {
                return result;
            }

            termsAndCondition.TermsAndConditions = model.TermsAndCondition;
            termsAndCondition.ModifiedBy = modifiedBy;
            termsAndCondition.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            result.ResponseMessage = "UPDATED SUCCESSFULLY";
            result.Data = termsAndCondition.Id;

            return result;
        }
    }
}
