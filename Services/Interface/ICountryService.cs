﻿using AzureRays.Shared.ViewModels;
using Optima.Models.DTO.CountryDTOs;
using Optima.Utilities.Helpers;
using Optima.Utilities.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Interface
{
    public interface ICountryService
    {
        Task<BaseResponse<bool>> CreateCountry(CreateCountryDTO model, Guid UserId);
        Task<BaseResponse<CountryDTO>> GetCountry(Guid id);
        Task<BaseResponse<bool>> UpdateCountry(UpdateCountryDTO model, Guid UserId);
        Task<BaseResponse<bool>> DeleteCountry(Guid id);
        Task<BaseResponse<PagedList<CountryDTO>>> GetAllCountry(BaseSearchViewModel model);
        Task<BaseResponse<List<CountryDTO>>> GetAllCountry();
    }
}
 