// Copyright 2014-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Exposes access to Financial Aid Award Categories data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AwardCategoriesController : BaseCompressedApiController
    {
        private readonly IFinancialAidReferenceDataRepository FinancialAidReferenceDataRepository;
        private readonly IAdapterRegistry AdapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// AwardCategoriesController constructor
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="financialAidReferenceDataRepository">FinancialAidReferenceDataRepository</param>
        /// <param name="logger">Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AwardCategoriesController(IAdapterRegistry adapterRegistry, IFinancialAidReferenceDataRepository financialAidReferenceDataRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            AdapterRegistry = adapterRegistry;
            FinancialAidReferenceDataRepository = financialAidReferenceDataRepository;
            this.logger = logger;
        }

        /// <summary>
        /// Get a list of all Financial Aid Award Categories
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>A collection of AwardCategories</returns>
        /// <note>AwardCategory is cached for 24 hours.</note>
        [Obsolete("Obsolete as of Api version 1.8, use version 2 of this API")]
        [HttpGet]
        [HeaderVersionRoute("/award-categories", 1, false, Name = "AwardCategories")]
        public ActionResult<IEnumerable<AwardCategory>> GetAwardCategories()
        {
            try
            {
                var AwardCategoryCollection = FinancialAidReferenceDataRepository.AwardCategories;

                //Get the adapter for the type mapping
                var awardCategoryDtoAdapter = AdapterRegistry.GetAdapter<Domain.FinancialAid.Entities.AwardCategory, AwardCategory>();

                //Map the awardyear entity to the awardyear dto
                var awardCategoryDtoCollection = new List<AwardCategory>();
                foreach (var category in AwardCategoryCollection)
                {
                    awardCategoryDtoCollection.Add(awardCategoryDtoAdapter.MapToType(category));
                }

                return Ok(awardCategoryDtoCollection);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateNotFoundException("AwardCategories", "");
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting AwardCategories resource. See log for details");
            }
        }

        /// <summary>
        /// Get a list of all Financial Aid Award Category2 DTOs
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>A collection of AwardCategory2 DTOs</returns>
        /// <note>AwardCategory is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/award-categories", 2, true, Name = "AwardCategories2")]
        public async Task<ActionResult<IEnumerable<AwardCategory2>>> GetAwardCategories2Async()
        {
            try
            {
                //var AwardCategoryCollection = FinancialAidReferenceDataRepository.AwardCategories;
                var AwardCategoryCollection = await FinancialAidReferenceDataRepository.GetAwardCategoriesAsync();

                //Get the adapter for the type mapping
                var awardCategoryDtoAdapter = AdapterRegistry.GetAdapter<Domain.FinancialAid.Entities.AwardCategory, AwardCategory2>();

                //Map the awardyear entity to the awardyear2 dto
                var awardCategoryDtoCollection = new List<AwardCategory2>();
                foreach (var category in AwardCategoryCollection)
                {
                    awardCategoryDtoCollection.Add(awardCategoryDtoAdapter.MapToType(category));
                }

                return awardCategoryDtoCollection;
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateNotFoundException("AwardCategories", "");
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting AwardCategories resource. See log for details");
            }
        }
    }
}
