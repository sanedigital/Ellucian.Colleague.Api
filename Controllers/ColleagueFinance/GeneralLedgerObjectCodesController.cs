// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// This is the controller for General Ledger object codes.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class GeneralLedgerObjectCodesController : BaseCompressedApiController
    {
        private readonly IGlObjectCodeService glObjectCodeService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the GL object code object.
        /// </summary>
        /// <param name="glObjectCodeService">GL object code service object</param>
        /// <param name="logger">Logger object</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public GeneralLedgerObjectCodesController(IGlObjectCodeService glObjectCodeService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.glObjectCodeService = glObjectCodeService;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves the filtered GL object codes. Uses the filter criteria selected in
        /// the cost centers view because the filter has to persist on the object view.
        /// </summary>
        /// <param name="criteria">Cost center filter criteria.</param>
        /// <returns>GL object codes DTOs that match the filter criteria.</returns>
        /// <accessComments>
        /// The user can only access those GL object codes for which they have
        /// GL account security access granted.
        /// </accessComments>        
        [HttpPost]
        [HeaderVersionRoute("/qapi/general-ledger-object-codes", 1, true, Name = "QueryGeneralLedgerObjectCodes")]
        public async Task<ActionResult<IEnumerable<GlObjectCode>>> QueryGeneralLedgerObjectCodesByPostAsync([FromBody]CostCenterQueryCriteria criteria)
        {
            try
            {
                if (criteria == null)
                {
                    throw new ArgumentNullException("criteria", "The query criteria must be specified.");
                }

                if (criteria.Ids != null && criteria.Ids.Count > 1)
                {
                    throw new ArgumentException("Only 0 or 1 cost center IDs may be specified.");
                }

                return Ok(await glObjectCodeService.QueryGlObjectCodesAsync(criteria));
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
            catch (ArgumentException agex)
            {
                logger.LogError(agex, agex.Message);
                return CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Unable to get the GL object codes.", HttpStatusCode.BadRequest);
            }
        }
    }
}
