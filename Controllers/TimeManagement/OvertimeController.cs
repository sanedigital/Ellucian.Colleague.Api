// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.TimeManagement.Services;
using Ellucian.Colleague.Dtos.TimeManagement;
using Ellucian.Data.Colleague.Exceptions;
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


namespace Ellucian.Colleague.Api.Controllers.TimeManagement
{
    /// <summary>
    /// OvertimeController
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.TimeManagement)]
    public class OvertimeController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IOvertimeCalculationService overtimeCalculationService;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
        private const string unexpectedGenericErrorMessage = "Unexpected error occurred while processing the request.";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="overtimeCalculationService"></param>
        /// <param name="logger"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public OvertimeController(IOvertimeCalculationService overtimeCalculationService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.overtimeCalculationService = overtimeCalculationService;
            this.logger = logger;
        }

        /// <summary>
        /// Requests an overtime calculation result for a provided the person and date range.
        /// The person and date range are specified in the OvertimeQueryCriteria object in the request body.
        /// See documentation for OvertimeQueryCriteria for information on individual properties.
        /// 
        /// This endpoint will return an error if:
        ///     1. 400 - The OvertimeQueryCriteria is not provided
        ///     2. 400 - The OvertimeQueryCriteria is incorrectly formatted
        ///     3. 403 - The logged in user does not have permission to access the information requested in the criteria
        ///     4. 400 - An unhandled exception occurs on the server
        /// 
        /// </summary>
        /// <accessComments>
        /// In order to calculate overtime, the current user must:
        /// 1. Be the employee. An employee cannot calculate overtime for other employees.
        /// 2. Be the employee's supervisor. A supervior can only calculate overtime for his/her supervisees.
        /// 3. Be a proxy for the employee's supervisor. A proxy for a supervisor can only calculate overtime for the supervisor's supervisees.
        /// 
        /// If any of these conditions fail, this route will return a 403 Forbidden status and not calculate and return overtime for any of the criteria.
        /// </accessComments>
        /// <param name="criteria">An array of OvertimeQueryCriteria objects. This API will calculate overtime for each of the criteria objects.</param>
        /// <param name="effectivePersonId">If proxying for a supervisor, set the supervisor's person id to this URI query parameter</param>
        /// <returns>An Overtime Calculation Result</returns>
        [HttpPost]
        [HeaderVersionRoute("/qapi/overtime", 1, false, Name = "OvertimeCalculation")]
        public async Task<ActionResult<OvertimeCalculationResult>> QueryByPostOvertime([FromBody] OvertimeQueryCriteria criteria, [FromQuery] string effectivePersonId = null)
        {
            if (criteria == null)
            {
                return CreateHttpResponseException("criteria is required argument", HttpStatusCode.BadRequest);
            }

            try
            {
                return await overtimeCalculationService.CalculateOvertime(criteria, effectivePersonId);
            }
            catch (ArgumentException ae)
            {
                logger.LogError(ae, "Argument error in QueryByPostOvertime endpoint");
                return CreateHttpResponseException(ae.Message, HttpStatusCode.BadRequest);
            }
            catch (ApplicationException ae)
            {
                logger.LogError(ae, "Known error thrown by QueryByPostOvertime endpoint");
                return CreateHttpResponseException(ae.Message, HttpStatusCode.BadRequest);
            }
            catch(PermissionsException pe)
            {
                var message = string.Format("You don't have permission to query overtime for the given criteria - {0}", pe.Message);
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error");
                return CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Use this route to calculate overtime in bulk. This route reduces load on the API servers and is much more effective
        /// for the bulk calculation use case than issuing separate requests to calculate overtime. The ClientQueryId GUID attributes in the OvertimeQueryCriteria
        /// object and the OvertimeCalculationResult object allow the consuming client to "join" any given request to its associated response.
        /// 
        /// The persons and date ranges are specified in the OvertimeQueryCriteria objects in the request body.
        /// See documentation for OvertimeQueryCriteria for information on individual properties.
        /// 
        /// This endpoint will return an error if:
        ///     1. 400 - An OvertimeQueryCriteria is not provided
        ///     2. 400 - Any OvertimeQueryCriteria is incorrectly formatted
        ///     3. 403 - The logged in user does not have permission to access the information requested in the criteria
        ///     4. 400 - An unhandled exception occurs on the server
        /// 
        /// </summary>
        /// <accessComments>
        /// In order to calculate overtime, the current user must:
        /// 1. Be the employee. An employee cannot calculate overtime for other employees.
        /// 2. Be the employee's supervisor. A supervior can only calculate overtime for his/her supervisees.
        /// 3. Be a proxy for the employee's supervisor. A proxy for a supervisor can only calculate overtime for the supervisor's supervisees.
        /// 
        /// If any of these conditions fail, this route will return a 403 Forbidden status and not calculate and return overtime for any of the criteria.
        /// </accessComments>
        /// <param name="criteria">An array of OvertimeQueryCriteria objects. This API will calculate overtime for each of the criteria objects.</param>
        /// <param name="effectivePersonId">If proxying for a supervisor, set the supervisor's person id to this URI query parameter</param>
        /// <returns>A array of Overtime Calculation Results</returns>
        [HttpPost]
        [HeaderVersionRoute("/qapi/overtime", 2, true, Name = "OvertimeCalculations")]
        public async Task<ActionResult<IEnumerable<OvertimeCalculationResult>>> QueryByPostOvertimes([FromBody]IEnumerable<OvertimeQueryCriteria> criteria, [FromQuery] string effectivePersonId = null)
        {
            if (criteria == null)
            {
                return CreateHttpResponseException("criteria is required argument", HttpStatusCode.BadRequest);
            }

            try
            {
                return Ok(await overtimeCalculationService.CalculateOvertime(criteria, effectivePersonId));
            }
            catch (ArgumentException ae)
            {
                logger.LogError(ae, "Argument error in QueryByPostOvertime endpoint");
                return CreateHttpResponseException(ae.Message, HttpStatusCode.BadRequest);
            }
            catch (ApplicationException ae)
            {
                logger.LogError(ae, "Known error thrown by QueryByPostOvertime endpoint");
                return CreateHttpResponseException(ae.Message, HttpStatusCode.BadRequest);
            }
            catch (PermissionsException pe)
            {
                var message = string.Format("You don't have permission to query overtime for the given criteria - {0}", pe.Message);
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}
