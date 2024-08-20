// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.Base
{

    /// <summary>
    /// Exposes Payable Deposit Directives functionality
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PayableDepositDirectivesController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IPayableDepositDirectiveService payableDepositDirectiveService;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
        private const string stepUpAuthenticationHeaderKey = "X-Step-Up-Authentication";

        /// <summary>
        /// Instantiate a new PayableDepositDirectivesController
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="payableDepositDirectiveService"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PayableDepositDirectivesController(ILogger logger, IPayableDepositDirectiveService payableDepositDirectiveService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.logger = logger;
            this.payableDepositDirectiveService = payableDepositDirectiveService;
        }

        /// <summary>
        /// Get all of the current user's PayableDepositDirectives.
        /// </summary>
        /// <returns>A list of a person's PayableDepositDirectives</returns>
        [HttpGet]
        [HeaderVersionRoute("/payable-deposit-directives", 1, true, Name = "GetPayableDepositDirectives")]
        public async Task<ActionResult<IEnumerable<PayableDepositDirective>>> GetPayableDepositDirectivesAsync()
        {
            try
            {
                logger.LogDebug("************Start- Process to get Payable Deposit Directives - Start************");
                var response = await payableDepositDirectiveService.GetPayableDepositDirectivesAsync();
                logger.LogDebug("************End- Process to get Payable Deposit Directives - End************");
                return Ok(response);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown exception occurred");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get a single PayableDepositDirective for the current user
        /// </summary>
        /// <param name="id">Id of the payableDepositDirective</param>
        /// <returns></returns>
        [HttpGet]
        [HeaderVersionRoute("/payable-deposit-directives/{id}", 1, true, Name = "GetPayableDepositDirective")]
        public async Task<ActionResult<PayableDepositDirective>> GetPayableDepositDirectiveAsync([FromRoute] string id)
        {

            if (string.IsNullOrEmpty(id))
            {
                logger.LogDebug("************ Payable Deposit Directive Id must be provided ************");
                return CreateHttpResponseException("payableDepositDirectiveId is required");
            }

            try
            {
                logger.LogDebug("************Start- Process to get Payable Deposit Directive - Start************");
                var payableDepositDirective = await payableDepositDirectiveService.GetPayableDepositDirectiveAsync(id);
                logger.LogDebug("************End- Process to get Payable Deposit Directive - End************");

                return payableDepositDirective;
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, "Access to resource is forbidden");
                return CreateHttpResponseException("You don't have permission to access to this resource", HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateNotFoundException("PayableDepositDirective", id);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown exception occurred");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Create a new PayableDepositDirective resource based on the data in body of the request. The BankingAuthenticationToken.Token property is 
        /// required in the X-Step-Up-Authentication header
        /// if account authentication is enabled.
        /// 
        /// </summary>
        /// <param name="payableDepositDirective">payableDepositDirective object containing data with which to create a PayableDepositDirective resource.</param>
        /// <returns>An ActionResult with the created resource in the Content property of the body and the URI of the created resource in the 
        /// Location Header. The schema of the PayableDepositDirective in the Content property of the response is the same as the schema of the input PayableDepositDirective from the Request</returns>
        [HttpPost]
        [HeaderVersionRoute("/payable-deposit-directives", 1, true, Name = "CreatePayableDepositDirective")]
        public async Task<ActionResult<PayableDepositDirective>> CreatePayableDepositDirectiveAsync([FromBody] PayableDepositDirective payableDepositDirective)
        {
            if (payableDepositDirective == null)
            {
                logger.LogDebug("************ Payable deposit directive cannot be null ************");
                return CreateHttpResponseException("payableDepositDirective object is required in request body");
            }

            var token = GetStepUpAuthenticationHeaderValue();

            try
            {
                logger.LogDebug("************ Start- Process to Create Payable Deposit Directive - Start ************");
                var newPayableDepositDirective = await payableDepositDirectiveService.CreatePayableDepositDirectiveAsync(token, payableDepositDirective);
                var response = Created(Url.Link("GetPayableDepositDirective", new { id = newPayableDepositDirective.Id }), newPayableDepositDirective);
                logger.LogDebug("************ End - Process to Create Payable Deposit Directive - End ************");
                return response;
            }

            // need to catch all the exceptions from the layers below this one
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException ae)
            {
                logger.LogError(ae, ae.Message);
                return CreateHttpResponseException("Input arguments are not valid");
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, "Creating this resource is forbidden");
                return CreateHttpResponseException("You don't have permission to create this resource", HttpStatusCode.Forbidden);
            }
            catch (ApplicationException ae)
            {
                logger.LogError(ae, ae.Message);
                return CreateHttpResponseException("Application exception occurred creating PayableDepositDirective resource");
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred creating PayableDepositDirective resource");
            }
        }

        /// <summary>
        /// Updates a Payable Deposit Directive for the current user. This POST request has PUT characteristics, however, only updates
        /// to the PayableDepositDirective Nickname and IsElectronicPaymentRequested flag are accepted. Any other updates are ignored.
        /// 
        /// The BankingAuthenticationToken.Token property is 
        /// required in the X-Step-Up-Authentication header
        /// if account authentication is enabled.
        /// 
        /// The endpoint will reject updates if:
        ///     1. 400 - A deposit's end date occurs before its start date
        ///     2. 400 - A PayableDepositDirective has malformed RoutingId, InstitutionId, or BranchNumber
        ///     3. 409 - The PayableDepositDirective resource has changed on server - see ChangeDateTime of PayableDepositDirective
        ///     4. 409 - The Business Office has a lock on the resource
        /// 
        /// </summary>
        /// <param name="updatedPayableDepositDirective"></param>
        /// <returns>The updated PayableDepositDirective object</returns>
        [HttpPost]
        [HeaderVersionRoute("/payable-deposit-directives/{id}", 1, true, Name = "UpdatePayableDepositDirective")]
        public async Task<ActionResult<PayableDepositDirective>> UpdatePayableDepositDirectiveAsync([FromBody] PayableDepositDirective updatedPayableDepositDirective)
        {
            if (updatedPayableDepositDirective == null)
            {
                logger.LogDebug("************ Updated Payable Deposit Directive cannot be null when updating Payable deposit directives ************");
                return CreateHttpResponseException("updatedPayableDepositDirective object is required in request body");
            }

            var token = GetStepUpAuthenticationHeaderValue();

            try
            {
                logger.LogDebug("************Start- Process to Update Payable Deposit Directives - Start************");
                var response = await payableDepositDirectiveService.UpdatePayableDepositDirectiveAsync(token, updatedPayableDepositDirective);
                logger.LogDebug("************ End- Process to Update Payable Deposit Directives - End ************");
                return response;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException ae)
            {
                var message = "Input arguments are not valid";
                logger.LogError(ae, message);
                return CreateHttpResponseException(message);
            }

            catch (PermissionsException pe)
            {
                var message = "You are forbidden from updating this resource";
                logger.LogError(pe, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateNotFoundException("PayableDepositDirective", updatedPayableDepositDirective.Id);
            }
            catch (RecordLockException rle)
            {
                logger.LogError(rle, "PersonAddrBnkInfo record is locked in the db.");
                return CreateHttpResponseException(rle.Message, HttpStatusCode.Conflict);
            }

            catch (Exception e)
            {
                logger.LogError(e, "Unknown error occurred");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Delete a Payable Deposit Directive.
        /// 
        /// The BankingAuthenticationToken.Token property is 
        /// required in the X-Step-Up-Authentication header
        /// if account authentication is enabled.
        /// 
        /// The endpoint will reject updates if:
        ///     1. 400 - Something went wrong trying to delete the resource
        ///     2. 403 - You attempt to delete a deposit directive that is not yours, or you do not have permission to edit payable deposits.
        ///     3. 409 - The Business Office has a lock on the resource, or the server was unable to delete the resource.
        /// 
        /// </summary>
        /// <param name="id">The id of the PayableDepositDirective resource to delete</param>    
        /// <returns>204 Status if deletion of the resource was successful</returns>
        [HttpDelete]
        [HeaderVersionRoute("/payable-deposit-directives/{id}", 1, true, Name = "DeletePayableDepositDirective")]
        public async Task<IActionResult> DeletePayableDepositDirectiveAsync([FromRoute] string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                logger.LogDebug("************ Payable deposit directive Id must be provided ************");
                return CreateHttpResponseException("Id is required in request body");
            }

            var token = GetStepUpAuthenticationHeaderValue();

            try
            {
                logger.LogDebug("************Start- Process to Delete Payable Deposit Directive - Start************");
                await payableDepositDirectiveService.DeletePayableDepositDirectiveAsync(token, id);
                logger.LogDebug("************End- Process to Delete Payable Deposit Directive - End************");
                return NoContent();
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException ae)
            {
                var message = "Input arguments are not valid";
                logger.LogError(ae, message);
                return CreateHttpResponseException(message);
            }

            catch (PermissionsException pe)
            {
                var message = "You are forbidden from updating this resource";
                logger.LogError(pe, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateNotFoundException("PayableDepositDirective", id);
            }
            catch (RecordLockException rle)
            {
                logger.LogError(rle, "PersonAddrBnkInfo record is locked in the db.");
                return CreateHttpResponseException(rle.Message, HttpStatusCode.Conflict);
            }
            catch (ApplicationException ae)
            {
                logger.LogError(ae, "Application exception occurred deleting PersonAddrBnkInfo resource");
                return CreateHttpResponseException(ae.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error occurred");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Post a request for authentication to create or update a PayableDepositDirective.
        /// </summary>
        /// <param name="challenge">A challenge object containing the required authentication data</param>
        /// <returns>A BankingAuthenticationToken that can be used to authenticate future Update and Create PayableDepositDirectives. Tokens expire after ten (10) minutes</returns>
        [HttpPost]
        [HeaderVersionRoute("/payable-deposit-directives/{id}", 1, false, RouteConstants.EllucianStepUpAuthenticationFormat, Name = "AuthenticatePayableDepositDirective")]
        [HeaderVersionRoute("/payable-deposit-directives", 1, false, RouteConstants.EllucianStepUpAuthenticationFormat, Name = "AuthenticatePayableDepositDirectives")]
        public async Task<ActionResult<BankingAuthenticationToken>> AuthenticatePayableDepositDirectiveAsync([FromBody] PayableDepositDirectiveAuthenticationChallenge challenge)
        {
            if (challenge == null)
            {
                logger.LogDebug("************Challenge object must be provided ************");
                return CreateHttpResponseException("challenge object is required in body of request");
            }

            try
            {
                logger.LogDebug("************Start- Process to Authenticate Payable Deposit Directive - Start************");
                var response =  await payableDepositDirectiveService.AuthenticatePayableDepositDirectiveAsync(challenge.PayableDepositDirectiveId, challenge.ChallengeValue, challenge.AddressId);
                logger.LogDebug("************End- Process to Authenticate Payable Deposit Directive - End************");
                return response;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (ArgumentNullException ane)
            {
                logger.LogError(ane, ane.Message);
                return CreateHttpResponseException("Invalid Arguments");
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Current user does not have permission to authenticate payable directive", HttpStatusCode.Forbidden);
            }
            catch (BankingAuthenticationException bae)
            {
                logger.LogError(bae, bae.Message);
                return CreateHttpResponseException("Authentication failed", HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateNotFoundException("PayabeDepositDirective", challenge.PayableDepositDirectiveId);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error authenticating payable deposit directive");
                return CreateHttpResponseException(e.Message);
            }
        }

        /// <summary>
        /// Gets the value of the token used for Step-Up Authentication
        /// </summary>
        /// <returns>The first X-Step-Up-Authentication header value, if present. Otherwise null.</returns>
        private string GetStepUpAuthenticationHeaderValue()
        {
            if (Request.Headers.TryGetValue(stepUpAuthenticationHeaderKey, out StringValues token))
            {
                return token.FirstOrDefault();
            }
            return null;
        }
    }
}

