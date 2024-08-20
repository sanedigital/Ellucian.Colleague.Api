// Copyright 2017-2024 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Ellucian.Data.Colleague.Exceptions;
namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to Payroll Deposit Directives
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class PayrollDepositDirectivesController : BaseCompressedApiController
    {
        private const string stepUpAuthenticationHeaderKey = "X-Step-Up-Authentication";
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        private readonly ILogger logger;
        private readonly IPayrollDepositDirectiveService payrollDepositDirectiveService;

        /// <summary>
        /// PayrollDepositDirectives Controller constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="payrollDepositDirectiveService"></param>
        /// <param name="actionContextAccessor">Interface to the action context accessor</param>
        /// <param name="apiSettings"></param>
        public PayrollDepositDirectivesController(ILogger logger, IPayrollDepositDirectiveService payrollDepositDirectiveService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.logger = logger;
            this.payrollDepositDirectiveService = payrollDepositDirectiveService;
        }

        /// <summary>
        /// Gets a list of PayrollDepositDirectives for the Current User
        /// </summary>
        /// <returns>The Current User's PayrollDepositDirectives</returns>
        [HttpGet]
        [HeaderVersionRoute("/payroll-deposit-directives", 1, true, Name = "GetPayrollDepositDirectives")]
        public async Task<ActionResult<IEnumerable<PayrollDepositDirective>>> GetPayrollDepositDirectivesAsync()
        {
            try
            {
                logger.LogDebug("************Start- Process to get Payroll Deposit Directives - Start************");
                var response = await payrollDepositDirectiveService.GetPayrollDepositDirectivesAsync();
                logger.LogDebug("************End- Process to get Payroll Deposit Directives - End************");
                return Ok(response);
            }

            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException knfe)
            {
                var message = "Unable to find current user in payroll file";
                logger.LogError(knfe, message);
                return CreateHttpResponseException(message, HttpStatusCode.NotFound);
            }
            catch (PermissionsException pe)
            {
                var message = "You are forbidden from accessing this resource";
                logger.LogError(pe, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);

            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error occurred");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets a single PayrollDepositDirective from its record identifier. Requested resource must be owned by the current user.
        /// </summary>
        /// <param name="id">The Id of the payroll deposit directive</param>
        /// <returns>The requested payroll deposit directive</returns>
        [HttpGet]
        [HeaderVersionRoute("/payroll-deposit-directives/{id}", 1, true, Name = "GetPayrollDepositDirective")]
        public async Task<ActionResult<PayrollDepositDirective>> GetPayrollDepositDirectiveAsync([FromRoute]string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                logger.LogDebug("************ Payroll Deposit Directive Id must be provided ************");
                return CreateHttpResponseException("PayrollDepositDirective Id must be provided in the URI", HttpStatusCode.BadRequest);
            }
            try
            {
                logger.LogDebug("************Start- Process to get Payroll Deposit Directive - Start************");
                var response = await payrollDepositDirectiveService.GetPayrollDepositDirectiveAsync(id);
                logger.LogDebug("************End- Process to get Payroll Deposit Directive - End************");
                return response;
            }
            catch (KeyNotFoundException knfe)
            {
                var message = string.Format("Unable to find requested deposit {0} in payroll file", id);
                logger.LogError(knfe, message);
                return CreateHttpResponseException(message, HttpStatusCode.NotFound);
            }
            catch (PermissionsException pe)
            {
                var message = "You are forbidden from accessing this resource";
                logger.LogError(pe, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error occurred");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Updates a list of payroll deposit directives. Use this endpoint to batch update directives all at once. You must obtain an authentication token from the URI.
        /// Accept: application/vnd.ellucian-step-up-authentication.v1+json
        /// POST payroll-deposit-directives endpoint 
        ///         
        /// The BankingAuthenticationToken.Token property is 
        /// required in the X-Step-Up-Authentication header 
        /// if account authentication is enabled.
        /// 
        /// </summary>
        /// <param name="payrollDepositDirectives">A list of PayrollDepositDirectives containing the updates.</param>
        /// <returns>The list of PayrollDepositDirectives with the successfully updated properties</returns>
        [HttpPut]
        [HeaderVersionRoute("/payroll-deposit-directives", 1, true, Name = "UpdatePayrollDepositDirectives")]
        public async Task<ActionResult<IEnumerable<PayrollDepositDirective>>> UpdatePayrollDepositDirectivesAsync([FromBody]IEnumerable<PayrollDepositDirective> payrollDepositDirectives)
        {
            if (payrollDepositDirectives == null)
            {
                logger.LogDebug("************ Payroll Deposit Directives cannot be null when updating Payroll deposit directives ************");
                return CreateHttpResponseException("PayrollDepositDirectives cannot be null when updating PayrollDepositDirectives",
                    HttpStatusCode.BadRequest);
            }

            var token = GetStepUpAuthenticationHeaderValue();

            try
            {
                logger.LogDebug("************Start- Process to Update Payroll Deposit Directives - Start************");
                var response = await payrollDepositDirectiveService.UpdatePayrollDepositDirectivesAsync(token, payrollDepositDirectives);
                logger.LogDebug("************End- Process to Update Payroll Deposit Directives - End************");
                return Ok(response);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException knfe)
            {
                var message = string.Format("Unable to find deposit to update in payroll file");
                logger.LogError(knfe, message);
                return CreateHttpResponseException(message, HttpStatusCode.NotFound);
            }
            catch (PermissionsException pe)
            {
                var message = "You are forbidden from accessing this resource";
                logger.LogError(pe, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);

            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error occurred");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Updates a single PayrollDepositDirective. You must obtain an authentication token from the URI.
        /// Accept: application/vnd.ellucian-step-up-authentication.v1+json
        /// POST payroll-deposit-directives endpoint 
        ///         
        /// The BankingAuthenticationToken.Token property is 
        /// required in the X-Step-Up-Authentication header
        /// if account authentication is enabled.
        /// 
        /// </summary>
        /// <param name="id">The id of the directive being updated</param>
        /// <param name="payrollDepositDirective">The PayrollDepositDirective to update</param>
        /// <returns>The updated PayrollDepositDirective</returns>
        [HttpPut]
        [HeaderVersionRoute("/payroll-deposit-directives/{id}", 1, true, Name = "UpdatePayrollDepositDirective")]
        public async Task<ActionResult<PayrollDepositDirective>> UpdatePayrollDepositDirectiveAsync([FromRoute] string id, [FromBody] PayrollDepositDirective payrollDepositDirective)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                logger.LogDebug("************ Id cannot be null when updating Payroll Deposit Directives ************");
                return CreateHttpResponseException("Identifier cannot be null when updating PayrollDepositDirectives",
                    HttpStatusCode.BadRequest);
            }
            if (payrollDepositDirective == null)
            {
                logger.LogDebug("************ Payroll deposit directive cannot be null when updating Payroll Deposit Directive ************");
                return CreateHttpResponseException("PayrollDepositDirectives cannot be null when updating PayrollDepositDirective",
                    HttpStatusCode.BadRequest);
            }
            if (id != payrollDepositDirective.Id)
            {
                logger.LogDebug("************ ID provided must match Id in directive ************");
                return CreateHttpResponseException("Id in URI must match Id in directive",
                    HttpStatusCode.BadRequest);
            }

            var token = GetStepUpAuthenticationHeaderValue();

            try
            {
                logger.LogDebug("************Start- Process to Update Payroll Deposit Directive - Start************");
                return await payrollDepositDirectiveService.UpdatePayrollDepositDirectiveAsync(token, payrollDepositDirective);
                logger.LogDebug("************End- Process to Update Payroll Deposit Directive - End************");
            }
            catch (KeyNotFoundException knfe)
            {
                var message = string.Format("Unable to find requested deposit {0} in payroll file", id);
                logger.LogError(knfe, message);
                return CreateHttpResponseException(message, HttpStatusCode.NotFound);
            }
            catch (PermissionsException pe)
            {
                var message = "You are forbidden from accessing this resource";
                logger.LogError(pe, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error occurred");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }


        /// <summary>
        /// Create a payroll deposit directive. You must obtain an authentication token from the URI.
        /// Accept: application/vnd.ellucian-step-up-authentication.v1+json
        /// POST payroll-deposit-directives endpoint 
        ///         
        /// The BankingAuthenticationToken.Token property is 
        /// required in the X-Step-Up-Authentication header
        /// if account authentication is enabled.
        /// 
        /// </summary>
        /// <param name="payrollDepositDirective">The PayrollDepositDirective to create</param>
        /// <returns>The created PayrollDepositDirective. Response Status will be 201 - Created </returns>
        [HttpPost]
        [HeaderVersionRoute("/payroll-deposit-directives", 1, true, Name = "CreatePayrollDepositDirective")]
        public async Task<ActionResult<PayrollDepositDirective>> CreatePayrollDepositDirectiveAsync([FromBody]PayrollDepositDirective payrollDepositDirective)
        {
            if (payrollDepositDirective == null)
            {
                logger.LogDebug("************ Payroll deposit directive cannot be null when updating Payroll Deposit Directives ************");
                return CreateHttpResponseException("payrollDepositDirective cannot be null when creating PayrollDepositDirectives",
                    HttpStatusCode.BadRequest);
            }
            var token = GetStepUpAuthenticationHeaderValue();
            try
            {
                logger.LogDebug("************ Start- Process to Create Payroll Deposit Directive - Start ************");
                var createdDirective = await payrollDepositDirectiveService.CreatePayrollDepositDirectiveAsync(token, payrollDepositDirective);
                logger.LogDebug("************ End - Process to Create Payroll Deposit Directive - End ************");
                return Created(Url.Link("GetPayrollDepositDirective", new { id = createdDirective.Id }), createdDirective);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException knfe)
            {
                var message = string.Format("Unable to find deposit to create in payroll file");
                logger.LogError(knfe, message);
                return CreateHttpResponseException(message, HttpStatusCode.NotFound);
            }
            catch (PermissionsException pe)
            {
                var message = "You are forbidden from accessing this resource";
                logger.LogError(pe, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);

            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error occurred");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }


        /// <summary>
        /// Deletes a single PayrollDepositDirective using its record identifier. You must obtain an authentication token from the URI.
        /// Accept: application/vnd.ellucian-step-up-authentication.v1+json
        /// POST payroll-deposit-directives endpoint 
        ///         
        /// The BankingAuthenticationToken.Token property is 
        /// required in the X-Step-Up-Authentication header
        /// if account authentication is enabled.
        /// 
        /// </summary>
        /// <param name="id">The Id of the directive to delete</param>
        /// <returns>HTTP Status 204 - No Content</returns>
        [HttpDelete]
        [HeaderVersionRoute("/payroll-deposit-directives/{id}", 1, true, Name = "DeletePayrollDepositDirective")]
        public async Task<IActionResult> DeletePayrollDepositDirectiveAsync([FromRoute]string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                logger.LogDebug("************ Payroll deposit directive Id must be provided ************");
                return CreateHttpResponseException("PayrollDepositDirective Id must me provided in the URI", HttpStatusCode.BadRequest);
            }
            var token = GetStepUpAuthenticationHeaderValue();
            try
            {
                logger.LogDebug("************ Start- Process to Delete Payroll Deposit Directive - Start ************");
                var isSuccess = await payrollDepositDirectiveService.DeletePayrollDepositDirectiveAsync(token, id);
                if (isSuccess)
                {
                    logger.LogDebug("************ Successfully deleted Payroll deposit directive ************");
                    logger.LogDebug("************ End - Process to Delete Payroll Deposit Directive - End ************");
                    return NoContent();
                }
                else
                {
                    logger.LogDebug("************ Failed to delete Payroll deposit directive ************");
                    logger.LogDebug("************ End - Process to Delete Payroll Deposit Directive - End ************");
                    return BadRequest("Unable to delete payroll deposit directieve");
                }
                
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return Unauthorized(invalidSessionErrorMessage);
            }

            catch (KeyNotFoundException knfe)
            {
                var message = string.Format("Unable to find requested deposit {0} in payroll file", id);
                logger.LogError(knfe, message);
                return NotFound();
            }
            catch (PermissionsException pe)
            {
                var message = "You are forbidden from accessing this resource";
                logger.LogError(pe, message);
                return Forbid(message);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error occurred");
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Deletes one or more PayrollDepositDirective a list of record identifiers. You must obtain an authentication token from the URI. Each id must
        /// be provided in the request:
        /// 
        /// Example:
        ///     1. to delete a single PayrollDepositDirective use /payroll-deposit-directives?id=123
        ///     2. to delete multiple PayrollDepositDirectives use /payroll-deposit-directives?id=123&amp;id=456
        /// 
        /// Accept: application/vnd.ellucian-step-up-authentication.v1+json
        /// POST payroll-deposit-directives endpoint 
        /// 
        /// The endpoint will not delete the requested PayrollDepositDirective(s) if:
        ///     1.  400 - no ids are provided in the uri
        ///     2.  403 - User does not have permission to delete requested record ids
        ///     3.  403 - no BankingAuthenticationToken is povided
        ///     4.  404 - requested id(s) do not exist
        ///     
        /// The BankingAuthenticationToken.Token property is 
        /// required in the X-Step-Up-Authentication header
        /// if account authentication is enabled.
        /// 
        /// </summary>
        /// <param name="id">The Id(s) of the directives to delete</param>
        /// <returns>HTTP Status 204 - No Content</returns>
        [HttpDelete]
        [HeaderVersionRoute("/payroll-deposit-directives", 1, true, Name = "DeletePayrollDepositsDirective")]
        public async Task<IActionResult> DeletePayrollDepositDirectivesAsync([FromQuery]IEnumerable<string> id)
        {
            if (!id.Any())
            {
                logger.LogDebug("************ One or more Payroll Deposit Directive must be provided ************");
                return CreateHttpResponseException("One or more PayrollDepositDirective Ids must be provided in the uri", HttpStatusCode.BadRequest);
            }

            var token = GetStepUpAuthenticationHeaderValue();

            try
            {
                logger.LogDebug("************ Start- Process to Delete Multiple Payroll Deposit Directives - Start ************");
                var isSuccess = await payrollDepositDirectiveService.DeletePayrollDepositDirectivesAsync(token, id);
                if (isSuccess)
                {
                    logger.LogDebug("************ Successfully deleted Payroll deposit directives ************");
                    logger.LogDebug("************ End - Process to Delete Multiple Payroll Deposit Directives - End ************");
                    return NoContent();
                }
                else
                {
                    logger.LogDebug("************ Failed to delete Payroll deposit directives ************");
                    logger.LogDebug("************ End - Process to Delete Multiple Payroll Deposit Directives - End ************");
                    return CreateHttpResponseException("Unable to delete payroll deposit directives", HttpStatusCode.BadRequest);
                }
                
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException knfe)
            {
                string message = "Unable to find requested deposit id in payroll file";
                logger.LogError(knfe, message);
                return CreateHttpResponseException(message, HttpStatusCode.NotFound);
            }
            catch (PermissionsException pe)
            {
                var message = "You are forbidden from accessing this resource";
                logger.LogError(pe, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error occurred");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Post a request for authentication to create or update a PayrollDepositDirective.
        /// To create a directive, post authentication for the remainder directive. 
        /// </summary>
        /// <param name="id">id of the deposit directive to authenticate against</param>
        /// <param name="value">the authentication value, which should be the account id of the deposit directive specified by the id in the URI</param>
        /// <returns>A BankingAuthenticationToken object. Tokens expire after ten (10) minutes.</returns>
        [HttpPost]
        [HeaderVersionRoute("/payroll-deposit-directives/{id}", 1, false, RouteConstants.EllucianStepUpAuthenticationFormat, Name = "AuthenticatePayrollDepositDirective")]
        public async Task<ActionResult<BankingAuthenticationToken>> PostPayrollDepositDirectiveAuthenticationAsync([FromRoute]string id, [FromBody]string value)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                logger.LogDebug("************ Payroll Deposit Directive id must be provided ************");
                return CreateHttpResponseException("id of PayrollDepositDirective is required in request URI", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrWhiteSpace(value))
            {
                logger.LogDebug("************ Authentication value/ account id is required in request body ************");
                return CreateHttpResponseException("authentication value is required in request body");
            }

            try
            {
                logger.LogDebug("************ Start- Process to Post Payroll Deposit Directive Authentication - Start ************");
                var response = await payrollDepositDirectiveService.AuthenticateCurrentUserAsync(id, value);
                logger.LogDebug("************ End - Process to Post Payroll Deposit Directive Authentication - End ************");
                return response;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Post a request for authentication to create a PayrollDepositDirective when an employee has no existing directives
        /// </summary>
        /// <returns>A BankingAuthenticationToken object. Tokens expire after ten (10) minutes.</returns>
        [HttpPost]
        [HeaderVersionRoute("/payroll-deposit-directives", 1, false, RouteConstants.EllucianStepUpAuthenticationFormat, Name = "AuthenticatePayrollDepositDirectives")]
        public async Task<ActionResult<BankingAuthenticationToken>> PostPayrollDepositDirectivesAuthenticationAsync()
        {
            try
            {
                logger.LogDebug("************ Start- Process to Authenticate current user - Start ************");
                var response = await payrollDepositDirectiveService.AuthenticateCurrentUserAsync(null, null);
                logger.LogDebug("************ End - Process to Authenticate current user - End ************");
                return response;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Gets the value of the token used for Step-Up Authentication
        /// </summary>
        /// <returns>The first X-Step-Up-Authentication header value, if present. Otherwise null.</returns>
        private string GetStepUpAuthenticationHeaderValue()
        {
            Request.Headers.TryGetValue(stepUpAuthenticationHeaderKey, out StringValues token);
            return token.ToString();
        }
    }
}
