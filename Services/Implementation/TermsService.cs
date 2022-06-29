using Microsoft.EntityFrameworkCore;
using Optima.Context;
using Optima.Models.Constant;
using Optima.Models.DTO.TermsDTO;
using Optima.Models.Entities;
using Optima.Models.Enums;
using Optima.Services.Interface;
using Optima.Utilities;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Implementation
{
    public class TermsService : BaseService, ITermsService
    {
        private readonly ApplicationDbContext _context;

        public TermsService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<BaseResponse<bool>> AcceptTermsAndCondition(AcceptTerms model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == model.emailAddress);

            if (user == null)
            {
                Errors.Add(ResponseMessage.ErrorMessage000);
                return new BaseResponse<bool>(ResponseMessage.ErrorMessage000, Errors);
            };

            user.HasAcceptedTerms = true;
            await  _context.SaveChangesAsync();
                        
            return new BaseResponse<bool>(true, ResponseMessage.AcceptedTermsAndCondition);
        }

        public async Task<BaseResponse<GetTermsDTO>> GetTermsAndCondition()
        {
            var termsAndCondition = await _context.TermsAndConditions.FirstOrDefaultAsync();

            if(termsAndCondition == null)
            {
                Errors.Add(ResponseMessage.ErrorMessage999);
                return new BaseResponse<GetTermsDTO>(ResponseMessage.ErrorMessage999, Errors);
            }

            var termsDto = new GetTermsDTO
            {
                DateCreated = termsAndCondition.CreatedOn,
                DateModified = termsAndCondition.ModifiedOn,
                Id = termsAndCondition.Id,
                TermsAndCondition = termsAndCondition.TermsAndConditions
            };

            return new BaseResponse<GetTermsDTO>(termsDto, ResponseMessage.SuccessMessage000);
        }

        public async Task<BaseResponse<Guid>> UpdateTermsAndCondition(Guid modifiedBy, DateTime currentDateTime, UpdateTermsDTO model)
        {
            var termsAndCondition = await _context.TermsAndConditions.FirstOrDefaultAsync();

            if (termsAndCondition == null)
            {
                await UpdateTermsAndConditions(modifiedBy, currentDateTime, model);                
                return new BaseResponse<Guid>(termsAndCondition.Id, ResponseMessage.TermsAndConditionUpdate);
            }

            termsAndCondition.TermsAndConditions = model.TermsAndCondition;
            termsAndCondition.ModifiedBy = modifiedBy;
            termsAndCondition.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();           

            return new BaseResponse<Guid>(termsAndCondition.Id, ResponseMessage.TermsAndConditionUpdate);
        }

        private async Task UpdateTermsAndConditions(Guid modifiedBy, DateTime currentDateTime, UpdateTermsDTO model)
        {
            var newTermsAndCondition = new TermsAndCondition
            {
                CreatedBy = modifiedBy,
                CreatedOn = currentDateTime,
                TermsAndConditions = model.TermsAndCondition
            };

            _context.TermsAndConditions.Add(newTermsAndCondition);
            await _context.SaveChangesAsync();
        }
                
    }
}
