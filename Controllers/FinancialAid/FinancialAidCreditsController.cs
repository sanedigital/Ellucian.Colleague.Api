// Copyright 2014-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// The FinancialAidCreditsController returns a student's course credits for either all or active only FA years.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class FinancialAidCreditsController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry AdapterRegistry;
        private readonly IFinancialAidCreditsService FinancialAidCreditsService;
        private readonly ILogger logger;

        /// <summary>
        /// Constructor for the FinancialAidCreditsController
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="financialAidCreditsService">FinancialAidCreditsService</param>
        /// <param name="logger">Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public FinancialAidCreditsController(IAdapterRegistry adapterRegistry, IFinancialAidCreditsService financialAidCreditsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            AdapterRegistry = adapterRegistry;
            FinancialAidCreditsService = financialAidCreditsService;
            this.logger = logger;
        }
        /// <summary>
        /// Get all of a student's course credits and how they apply to FA for either all or active only FA years
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions
        /// can request other users' data
        /// </accessComments>
        /// <param name="studentId">The studentId for which to get the course credits</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to get active award years data only, defaults to true</param>
        /// <returns>A list of AwardYearCredits DTOs</returns>
        [HttpGet]
        [HeaderVersionRoute("/students/financial-aid-credits/{studentId}", 1, true, Name = "GetFinancialAidCredits")]
        public async Task<ActionResult<IEnumerable<Dtos.FinancialAid.AwardYearCredits>>> GetFinancialAidCreditsAsync([FromRoute] string studentId, [FromQuery]bool getActiveYearsOnly = true)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null");
            }
            try
            {
                var awardYearCreds = await FinancialAidCreditsService.GetFinancialAidCreditsAsync(studentId, getActiveYearsOnly);

                var awardYearCredsDtoAdapter = AdapterRegistry.GetAdapter<Domain.FinancialAid.Entities.AwardYearCredits, Dtos.FinancialAid.AwardYearCredits>();

                //Use custom adapters to map the AwardYearCredits entity down to DTOs
                //Custom adapters are necessary due to embedded AwardPeriodCredits and CourseCreditAssocation DTO lists
                return Ok(awardYearCreds.Select(creds =>
                    awardYearCredsDtoAdapter.MapToType(creds)));
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Access to FinancialAidCredits resource is forbidden. See log for more details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException("Unable to find FinancialAidCredits resource. see log for more details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (InvalidOperationException ioe)
            {
                logger.LogError(ioe, ioe.Message);
                return CreateHttpResponseException("Operation is invalid based on state of FinancialAidCredits object. See log for more details.", System.Net.HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred while fetching FinancialAidCredits resource. See log for more details", System.Net.HttpStatusCode.BadRequest);
            }

        }
    }
}
