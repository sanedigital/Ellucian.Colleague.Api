// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.

using System.ComponentModel;
using Ellucian.Web.License;

using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Api.Licensing;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using System.Net;
using Ellucian.Web.Security;
using System;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// The controller for receive procurement
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class ReceiveProcurementsController : BaseCompressedApiController
    {
        private readonly IReceiveProcurementsService receiveProcurementsService;
        private readonly ILogger logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// This constructor initializes the ReceiveProcurementsController object
        /// </summary>
        /// <param name="receiveProcurementsService">ReceiveProcurements service object</param>
        /// <param name="logger">Logger object</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public ReceiveProcurementsController(IReceiveProcurementsService receiveProcurementsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.receiveProcurementsService = receiveProcurementsService;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves list of procurement receiving items
        /// </summary>
        /// <param name="personId">ID logged in user</param>
        /// <returns>list of Procurement Receving Items DTO</returns>
        /// <accessComments>
        /// Requires Staff record, requires permission UPDATE.RECEIVING
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/receive-procurements/{personId}", 1, false, Name = "GetReceiveProcurementsByPersonId")]
        public async Task<ActionResult<IEnumerable<ReceiveProcurementSummary>>> GetReceiveProcurementsByPersonIdAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                string message = "person Id must be specified.";
                logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            try
            {
                var receiveProcurement = await receiveProcurementsService.GetReceiveProcurementsByPersonIdAsync(personId);
                return Ok(receiveProcurement);
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to get the purchase order for receiving items.", HttpStatusCode.Forbidden);
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
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogError(csee, csee.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Unable to get the purchase order for receiving items.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="procurementAcceptOrReturnItemInformationRequest">Procurement accept return request DTO</param>
        /// <returns>Procurement accept return response DTO</returns>
        /// <accessComments>
        /// Requires Staff record, requires permission UPDATE.RECEIVING
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/receive-procurements", 1, false, Name = "PostAcceptOrReturnProcurementItems")]
        public async Task<ActionResult<ProcurementAcceptReturnItemInformationResponse>> PostAcceptOrReturnProcurementItemsAsync([FromBody] Dtos.ColleagueFinance.ProcurementAcceptReturnItemInformationRequest procurementAcceptOrReturnItemInformationRequest)
        {
            if (procurementAcceptOrReturnItemInformationRequest == null)
            {
                string message = "Must provide a procurementAcceptOrReturnItemInformationRequest object";
                logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                return await receiveProcurementsService.AcceptOrReturnProcurementItemsAsync(procurementAcceptOrReturnItemInformationRequest);
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex.Message);
                return CreateHttpResponseException("Insufficient permissions to accept/return procurement items.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException anex)
            {
                logger.LogError(anex, anex.Message);
                return CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.LogError(csee, csee.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return CreateHttpResponseException("Unable to accept/return the procurement items.", HttpStatusCode.BadRequest);
            }
        }
    }
}
