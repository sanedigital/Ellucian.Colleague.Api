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
    /// Controller exposes read-only access to Financial Aid AwardStatus data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AwardStatusesController : BaseCompressedApiController
    {
        private readonly IFinancialAidReferenceDataRepository FinancialAidReferenceDataRepository;
        private readonly IAdapterRegistry AdapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// AwardStatuses Controller constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter Registry</param>
        /// <param name="financialAidReferenceDataRepository">FinancialAid Reference Data Repository</param>
        /// <param name="logger">Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AwardStatusesController(IAdapterRegistry adapterRegistry, IFinancialAidReferenceDataRepository financialAidReferenceDataRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            AdapterRegistry = adapterRegistry;
            FinancialAidReferenceDataRepository = financialAidReferenceDataRepository;
            this.logger = logger;
        }

        /// <summary>
        /// Get a list of all Financial Aid Award Status codes from Colleague
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>A collection of AwardStatus data objects</returns>
        /// <note>AwardStatus is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/award-statuses", 1, true, Name = "AwardStatuses")]
        public ActionResult<IEnumerable<AwardStatus>> GetAwardStatuses()
        {
            try
            {
                var AwardStatusCollection = FinancialAidReferenceDataRepository.AwardStatuses;

                //Get the adapter for the type mapping
                var awardStatusDtoAdapter = AdapterRegistry.GetAdapter<Domain.FinancialAid.Entities.AwardStatus, AwardStatus>();

                var awardStatusDtoCollection = new List<AwardStatus>();
                foreach (var status in AwardStatusCollection)
                {
                    awardStatusDtoCollection.Add(awardStatusDtoAdapter.MapToType(status));
                }

                return Ok(awardStatusDtoCollection);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateNotFoundException("AwardStatuses", "");
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting AwardStatuses resource. See log for details");
            }
        }
    }
}
