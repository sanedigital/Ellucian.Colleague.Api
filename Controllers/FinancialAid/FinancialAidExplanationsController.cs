// Copyright 2018-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// FinancialAidExplanationsController provides access to 
    /// financial aid explanations data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.FinancialAid)]
    public class FinancialAidExplanationsController : BaseCompressedApiController
    {
        private readonly IFinancialAidReferenceDataRepository FinancialAidReferenceDataRepository;
        private readonly IAdapterRegistry AdapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// FinancialAidExplanationsController constructor
        /// </summary>
        /// <param name="adapterRegistry">adapter registry</param>
        /// <param name="financialAidReferenceDataRepository">financialAidReferenceDataRepository</param>
        /// <param name="logger">logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public FinancialAidExplanationsController(IAdapterRegistry adapterRegistry, IFinancialAidReferenceDataRepository financialAidReferenceDataRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            AdapterRegistry = adapterRegistry;
            this.logger = logger;
            FinancialAidReferenceDataRepository = financialAidReferenceDataRepository;
        }

        /// <summary>
        /// Gets all financial aid explanations
        /// </summary>
        /// <accessComments>Any authenticated user can get these resources</accessComments>
        /// <returns>a list of FinancialAidExplanation DTOs</returns>
        /// <note>FinancialAidExplanation is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/financial-aid-explanations", 1, true, Name = "GetFinancialAidExplanations")]
        public async Task<ActionResult<IEnumerable<FinancialAidExplanation>>> GetFinancialAidExplanationsAsync()
        {
            try
            {
                var explanationEntities = await FinancialAidReferenceDataRepository.GetFinancialAidExplanationsAsync();

                var faExplanationsEntityToDtoAdapter = AdapterRegistry.GetAdapter<Domain.FinancialAid.Entities.FinancialAidExplanation, FinancialAidExplanation>();
                var explanationDtos = new List<FinancialAidExplanation>();
                foreach(var entity in explanationEntities)
                {
                    explanationDtos.Add(faExplanationsEntityToDtoAdapter.MapToType(entity));
                }
                return explanationDtos;
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting financial aid explanations resource");
            }
        }
    }
}
