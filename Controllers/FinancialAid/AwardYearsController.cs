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

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Exposes access to Financial Aid Award Years data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AwardYearsController : BaseCompressedApiController
    {
        private readonly IFinancialAidReferenceDataRepository FinancialAidReferenceDataRepository;
        private readonly IAdapterRegistry AdapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// AwardYearsController constructor
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="financialAidReferenceDataRepository">FinancialAidReferenceDataRepository</param>
        /// <param name="logger">Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AwardYearsController(IAdapterRegistry adapterRegistry, IFinancialAidReferenceDataRepository financialAidReferenceDataRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            AdapterRegistry = adapterRegistry;
            FinancialAidReferenceDataRepository = financialAidReferenceDataRepository;
            this.logger = logger;
        }

        /// <summary>
        /// Get a list of all Financial Aid Years from Colleague
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>A collection of AwardYear data objects</returns>
        /// <note>AwardYear is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/award-years", 1, true, Name = "AwardYears")]
        public ActionResult<IEnumerable<AwardYear>> GetAwardYears()
        {
            try
            {
                var AwardYearCollection = FinancialAidReferenceDataRepository.AwardYears;

                //Get the adapter for the type mapping
                var awardYearDtoAdapter = AdapterRegistry.GetAdapter<Domain.FinancialAid.Entities.AwardYear, AwardYear>();

                //Map the awardyear entity to the awardyear dto
                var awardYearDtoCollection = new List<AwardYear>();
                foreach (var year in AwardYearCollection)
                {
                    awardYearDtoCollection.Add(awardYearDtoAdapter.MapToType(year));
                }

                return Ok(awardYearDtoCollection);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateNotFoundException("AwardYears", "");
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting AwardYears resource. See log for details");
            }
        }
    }
}
