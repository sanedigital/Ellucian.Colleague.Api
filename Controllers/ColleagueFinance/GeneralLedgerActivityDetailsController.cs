// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// This is the controller for GL activity details.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class GeneralLedgerActivityDetailsController : BaseCompressedApiController
    {
        private readonly IGeneralLedgerActivityDetailService generalLedgerActivityDetailsService;
        private readonly ILogger logger;

        /// <summary>
        /// GeneralLedgerActivityDetailsController class constructor.
        /// </summary>
        /// <param name="generalLedgerActivityDetailsService">GL activity details service object.</param>
        /// <param name="logger">Logger object.</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public GeneralLedgerActivityDetailsController(IGeneralLedgerActivityDetailService generalLedgerActivityDetailsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.generalLedgerActivityDetailsService = generalLedgerActivityDetailsService;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves all the actuals and encumbrances activity detail for the GL account and the fiscal year.
        /// </summary>
        /// <param name="criteria"><see cref="GlActivityDetailQueryCriteria">Query criteria</see>includes the GL account and the fiscal year for the query.</param>
        /// <returns>List of GL activity detail DTOs for the specified GL account for the specified fiscal year.</returns>
        /// <accessComments>
        /// The user can only access transactions for a GL account for which they have
        /// GL account security access granted.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/general-ledger-activity-details", 1, true, Name = "QueryGeneralLedgerActivityDetails")]
        public async Task<ActionResult<GlAccountActivityDetail>> QueryGeneralLedgerActivityDetailsByPostAsync([FromBody] GlActivityDetailQueryCriteria criteria)
        {
            try
            {
                if (criteria == null)
                {
                    throw new ArgumentNullException("criteria", "The query criteria must be specified.");
                }

                if (string.IsNullOrEmpty(criteria.GlAccount))
                {
                    throw new ArgumentNullException("GlAccount", "A GL account must be specified.");
                }

                if (string.IsNullOrEmpty(criteria.FiscalYear))
                {
                    throw new ArgumentNullException("FiscalYear", "A fiscal year must be specified.");
                }


                return await generalLedgerActivityDetailsService.QueryGlAccountActivityDetailAsync(criteria.GlAccount, criteria.FiscalYear);
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to access the GL account activity.", HttpStatusCode.Forbidden);
            }
            catch (ConfigurationException cnex)
            {
                logger.LogError(cnex, cnex.Message);
                return CreateHttpResponseException("Invalid configuration.", HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException anex)
            {
                logger.LogError(anex, anex.Message);
                return CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogDebug(csee, "Colleague session expired - unable to get activity for the GL account");
                return CreateHttpResponseException("Colleague session expired - unable to get activity for the GL account", HttpStatusCode.Unauthorized);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Unable to get activity for the GL account.", HttpStatusCode.BadRequest);
            }
        }
    }
}
