// Copyright 2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;


namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{   /// <summary>
    /// Provides Procurement Return Reason information
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class ProcurementReturnReasonController: BaseCompressedApiController
    {
        private readonly IProcurementReturnReasonService procurementReturnReasonService;
        private readonly ILogger logger;
        /// <summary>
        /// Constructor to initialize ProcurementReturnReasonController object.
        /// </summary>
        /// <param name="procurementReturnReasonService"></param>
        /// <param name="logger"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public ProcurementReturnReasonController(IProcurementReturnReasonService procurementReturnReasonService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.procurementReturnReasonService = procurementReturnReasonService;
            this.logger = logger;
        }

        /// <summary>
        /// Get all of the Return Reason codes.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [HeaderVersionRoute("/procurement-return-reasons", 1, false, Name = "GetProcurementReturnReasons")]
        public async Task<ActionResult<IEnumerable<ProcurementReturnReason>>> GetProcurementReturnReasonsAsync()
        {
             try
            {
                var procurementReturnReasonCodes = await procurementReturnReasonService.GetProcurementReturnReasonsAsync();
                return Ok(procurementReturnReasonCodes);
            }
            // Application exceptions will be caught below.
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to get the return reason codes.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException anex)
            {
                logger.LogError(anex, anex.Message);
                return CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (KeyNotFoundException knfex)
            {
                logger.LogError(knfex, knfex.Message);
                return CreateHttpResponseException("Record not found.", HttpStatusCode.NotFound);
            }
            // Application exceptions will be caught below.
            catch (Exception e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException("Unable to get Return Reason Codes.", HttpStatusCode.BadRequest);
            }

        }
    }
}
