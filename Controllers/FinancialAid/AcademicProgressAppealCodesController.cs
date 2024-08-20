// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Exposes Academic Progress Appeal Codes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AcademicProgressAppealCodesController : BaseCompressedApiController
    {
        private readonly IFinancialAidReferenceDataRepository financialAidReferenceDataRepository;

        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;
        /// <summary>
        /// Constructor to AcademicProgressStatusesController
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        /// <param name="financialAidReferenceDataRepository"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AcademicProgressAppealCodesController(IAdapterRegistry adapterRegistry, ILogger logger, IFinancialAidReferenceDataRepository financialAidReferenceDataRepository, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
            this.financialAidReferenceDataRepository = financialAidReferenceDataRepository;
        }

        /// <summary>
        /// Get all Academic Progress Appeal Codes objects.
        /// An Academic Progress Appeal Code indicates an appeal of an academic progress evaluation.
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns></returns>
        /// <note>AcademicProgressAppealCode is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/academic-progress-appeal-codes", 1, true, Name = "AcademicProgressAppealCodes")]
        public async Task<ActionResult<IEnumerable<Dtos.FinancialAid.AcademicProgressAppealCode>>> GetAcademicProgressAppealCodesAsync()
        {
            try
            {
                var appealEntities = await financialAidReferenceDataRepository.GetAcademicProgressAppealCodesAsync();
                var appealDtoAdapter = adapterRegistry.GetAdapter<Domain.FinancialAid.Entities.AcademicProgressAppealCode, Dtos.FinancialAid.AcademicProgressAppealCode>();
                return Ok(appealEntities.Select(s => appealDtoAdapter.MapToType(s)));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error occurred getting Academic Progress Appeal Codes");
                return CreateHttpResponseException(e.Message);
            }
        }

    }
}
