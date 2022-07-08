using log4net;
using Microsoft.EntityFrameworkCore;
using Optima.Context;
using Optima.Models.Constant;
using Optima.Models.DTO.FaqsDTO;
using Optima.Models.Entities;
using Optima.Services.Interface;
using Optima.Utilities;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Implementation
{
    public class FaqService : BaseService, IFaqService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILog _logger;

        public FaqService(ApplicationDbContext context)
        {
            _context = context;
            _logger = LogManager.GetLogger(typeof(FaqService));
        }

        /// <summary>
        /// CREATES FAQ
        /// </summary>
        /// <param name="model">The Model</param>
        /// <param name="UserId">The UserId</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> Create(CreateFaqDTO model, Guid UserId)
        {
            var newFaq = new Faq
            {
                Question = model.Question,
                Answer = model.Answer,
                CreatedBy = UserId,
            };

            await _context.Faqs.AddAsync(newFaq);
            await _context.SaveChangesAsync();

            _logger.Info("Faq Created Successfully ...At ExecutionPint: Create");
            return new BaseResponse<bool>(true, ResponseMessage.CreateFaq);
        }

        /// <summary>
        /// DELETE FAQ
        /// </summary>
        /// <param name="id">The Id</param>
        /// <param name="UserId">The UserId</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> Delete(Guid id)
        {
            var faq = await _context.Faqs.FirstOrDefaultAsync(x => x.Id == id);

            if (faq is null)
            {
                Errors.Add(ResponseMessage.FaqNotFound);
                return new BaseResponse<bool>(ResponseMessage.FaqNotFound, Errors);
            }

            _context.Faqs.Remove(faq);
            await _context.SaveChangesAsync();

            _logger.Info("Faq Deleted Successfully ...At ExecutionPint: Delete");
            return new BaseResponse<bool>(true, ResponseMessage.DeleteFaq);
        }

        /// <summary>
        /// GET AN FAQ
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Task&lt;BaseResponse&lt;FaqDTO&gt;&gt;.</returns>
        public async Task<BaseResponse<FaqDTO>> Get(Guid id)
        {
            var faq = await _context.Faqs.FirstOrDefaultAsync(x => x.Id == id);

            if (faq is null)
            {
                Errors.Add(ResponseMessage.FaqNotFound);
                return new BaseResponse<FaqDTO>(ResponseMessage.FaqNotFound, Errors);
            }

            Faq faqDTO = faq;
            return new BaseResponse<FaqDTO>(faqDTO, ResponseMessage.SuccessMessage000);
        }

        /// <summary>
        /// GET ALL FAQS
        /// </summary>
        /// <returns>Task&lt;BaseResponse&lt;List&lt;FaqDTO&gt;&gt;&gt;.</returns>
        public async Task<BaseResponse<List<FaqDTO>>> Get()
        {
            var faqs = await _context.Faqs.OrderByDescending(x => x.CreatedOn).ToListAsync();

            var faqDTOs = faqs.Select(x => (FaqDTO)x).ToList();
          
            return new BaseResponse<List<FaqDTO>>(faqDTOs, faqDTOs.Count, $"FOUND{faqs.Count} FAQ(s).");
        }

        /// <summary>
        /// UPDATE FAQ
        /// </summary>
        /// <param name="id">The Id</param>
        /// <param name="model">The Model</param>
        /// <param name="UserId">The UserId</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> Update(Guid id, UpdateFaqDTO model, Guid UserId)
        {
            var faq = await _context.Faqs.FirstOrDefaultAsync(x => x.Id == id);

            if (faq is null)
            {
                Errors.Add(ResponseMessage.FaqNotFound);
                return new BaseResponse<bool>(ResponseMessage.FaqNotFound, Errors);
            }

            faq.Question = string.IsNullOrWhiteSpace(model.Question) ? faq.Question : model.Question;
            faq.Answer = string.IsNullOrWhiteSpace(model.Answer) ? faq.Answer : model.Answer;
            faq.ModifiedBy = UserId;
            faq.ModifiedOn = DateTime.UtcNow;

            _context.Faqs.Update(faq);
            await _context.SaveChangesAsync();

            _logger.Info("Faq Updated Successfully");
            return new BaseResponse<bool>(true, ResponseMessage.UpdateFaq);
        }
    }
}
