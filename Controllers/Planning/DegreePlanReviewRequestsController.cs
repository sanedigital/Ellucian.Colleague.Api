// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Planning.Services;
using Ellucian.Colleague.Dtos.Planning;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.Planning
{
    /// <summary>
    /// Provides access to degree plan review requests
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Planning)]
    public class DegreePlanReviewRequestsController : BaseCompressedApiController
    {
        private readonly IDegreePlanService _degreePlanService;
        private readonly ILogger _logger;

        /// <summary>
        /// DegreePlanReviewRequestsController class constructor
        /// </summary>
        /// <param name="degreePlanService"></param>
        /// <param name="logger"></param>
        /// <param name="apiSettings"></param>
        /// <param name="actionContextAccessor"></param>
        public DegreePlanReviewRequestsController(IDegreePlanService degreePlanService, ILogger logger, ApiSettings apiSettings, IActionContextAccessor actionContextAccessor) : base(actionContextAccessor, apiSettings)
        {
            _degreePlanService = degreePlanService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves a student's degree plan submitted for review
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <accessComments>
        /// Degree plan review request can be accessed by any of the following permissions:
        /// ALL.ACCESS.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// VIEW.ANY.ADVISEE
        /// UPDATE.ADVISOR.ASSIGNMENTS
        /// </accessComments>
        /// <returns><see cref="DegreePlanReviewRequest">DegreePlanReviewRequest</see> Dto</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this degree plan</exception>
        [HttpPost]
        [HeaderVersionRoute("/qapi/degree-plans", 1, true, Name = "QueryDegreePlanReviewRequests")]
        public async Task<ActionResult<IEnumerable<DegreePlanReviewRequest>>> QueryDegreePlanReviewRequests([FromBody]DegreePlansSearchCriteria criteria, int pageSize = int.MaxValue, int pageIndex = 1)
        {
            _logger.LogInformation("Entering QueryDegreePlanReviewRequests");
            try
            {
                if (criteria != null && !string.IsNullOrEmpty(criteria.AdviseeKeyword))
                {
                    return Ok(await _degreePlanService.SearchReviewRequestDegreePlans(criteria, pageSize, pageIndex));
                }
                else
                {
                    return Ok(await _degreePlanService.GetReviewRequestedDegreePlans(criteria, pageSize, pageIndex));
                }
            }
            catch (PermissionsException peex)
            {
                _logger.LogInformation("User is not authorized to query degree plans where a review has been requested.");
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString(), "Unable to get QueryDegreePlanReviewRequests");
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves a student's degree plan submitted for review
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <accessComments>
        /// Degree plan review request can be accessed by any of the following permissions:
        /// ALL.ACCESS.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// VIEW.ANY.ADVISEE
        /// UPDATE.ADVISOR.ASSIGNMENTS
        /// </accessComments>
        /// <returns><see cref="DegreePlanReviewRequest">DegreePlanReviewRequest</see> Dto</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this degree plan</exception>
        [HttpPost]
        [HeaderVersionRoute("/qapi/degree-plans", 1, false, RouteConstants.EllucianPersonSearchExactMatchFormat, Name = "QueryDegreePlanReviewRequestsForExactMatch")]
        public async Task<ActionResult<IEnumerable<DegreePlanReviewRequest>>> QueryDegreePlanReviewRequestsForExactMatchAsync([FromBody]DegreePlansSearchCriteria criteria, int pageSize = int.MaxValue, int pageIndex = 1)
        {
            _logger.LogInformation("Entering QueryDegreePlanReviewRequests");
            try
            {
                if (criteria != null && !string.IsNullOrEmpty(criteria.AdviseeKeyword))
                {
                    return Ok(await _degreePlanService.SearchReviewRequestDegreePlansForExactMatchAsync(criteria, pageSize, pageIndex));
                }
                else
                {
                    return Ok(await _degreePlanService.GetReviewRequestedDegreePlans(criteria, pageSize, pageIndex));
                }
            }
            catch (PermissionsException peex)
            {
                _logger.LogInformation("User is not authorized to query degree plans where a review has been requested.");
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                string message = "Session has expired while retrieving degree plan submitted for review";
                _logger.LogError(csee, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString(), "Unable to get QueryDegreePlanReviewRequests");
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Post Degree plan review request assignment details
        /// </summary>
        /// <param name="degreePlanReviewRequest"></param>
        /// <accessComments>
        /// Degree plan review assignment can be created any of the following permissions:
        /// UPDATE.ADVISOR.ASSIGNMENTS
        /// </accessComments>
        /// <returns>HttpResponseMessage with Content of <see cref="DegreePlanReviewRequest">DegreePlanReviewRequest</see></returns>
        /// <exception cref="HttpResponseException">403 - You do not have the proper permission to create DegreePlanReviewRequest</exception>
        [HttpPost]
        [HeaderVersionRoute("/degree-plan-review-request", 1, true, Name = "PostDegreePlanReviewAssignment")]
        public async Task<ActionResult<DegreePlanReviewRequest>> PostAsync([FromBody] DegreePlanReviewRequest degreePlanReviewRequest)
        {
            if (degreePlanReviewRequest == null)
            {
                throw new ArgumentNullException("degreePlanReviewRequest", "degreePlanReviewRequest cannot be empty/null.");
            }

            if (string.IsNullOrEmpty(degreePlanReviewRequest.Id))
            {
                throw new ArgumentNullException("degreePlanReviewRequest", "Degree Plan Id cannot be null.");
            }

            try
            {
                var results = await _degreePlanService.UpdateAdvisorAssignment(degreePlanReviewRequest);
                return Created(string.Empty, results);
            }
            catch (PermissionsException peex)
            {
                _logger.LogInformation("User is not authorized to assign advisor for review requested degree plan.");
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                string message = "Session has expired while posting degree plan review request assignment details";
                _logger.LogError(csee, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString(), "Unable to PostAsync degree plan review assignment");
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}
