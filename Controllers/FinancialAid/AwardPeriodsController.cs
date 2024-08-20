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
    /// Controller exposes Colleague Financial Aid AwardPeriods resources
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AwardPeriodsController : BaseCompressedApiController
    {
        private readonly IFinancialAidReferenceDataRepository FinancialAidReferenceDataRepository;
        private readonly IAdapterRegistry AdapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the AwardPeriodController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IFinancialAidReferenceDataRepository">IFinancialAidReferenceDataRepository</see></param>
        /// <param name="logger">Transaction logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AwardPeriodsController(IAdapterRegistry adapterRegistry, IFinancialAidReferenceDataRepository referenceDataRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            AdapterRegistry = adapterRegistry;
            FinancialAidReferenceDataRepository = referenceDataRepository;
            this.logger = logger;
        }

        /// <summary>
        /// Get all the AwardPeriods
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>A set of AwardPeriod DTOs</returns> 
        /// <note>AwardPeriod is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/award-periods", 1, true, Name = "GetAwardPeriods")]
        public ActionResult<IEnumerable<AwardPeriod>> GetAwardPeriods()
        {
            try
            {
                var AwardPeriodCollection = FinancialAidReferenceDataRepository.AwardPeriods;

                // Get the right adapter for the type mapping
                var awardPeriodDtoAdapter = AdapterRegistry.GetAdapter<Ellucian.Colleague.Domain.FinancialAid.Entities.AwardPeriod, AwardPeriod>();

                // Map the award periods entity to the award periods DTO
                var awardPeriodDtoCollection = new List<AwardPeriod>();
                foreach (var awardPeriod in AwardPeriodCollection)
                {
                    awardPeriodDtoCollection.Add(awardPeriodDtoAdapter.MapToType(awardPeriod));
                }

                return Ok(awardPeriodDtoCollection);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateNotFoundException("AwardPeriods", "");
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting AwardPeriods resource. See log for details");
            }
        }

    }
}
