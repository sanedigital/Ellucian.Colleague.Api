// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Get and Create AwardPackageChangeRequest endpoints
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AwardPackageChangeRequestsController : BaseCompressedApiController
    {
        private readonly IAwardPackageChangeRequestService awardPackageChangeRequestService;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Constructor for AwardPackageChangeRequestsController
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="logger">Logger</param>
        /// <param name="awardPackageChangeRequestService">AwardPackageChangeRequestService</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AwardPackageChangeRequestsController(
            IAdapterRegistry adapterRegistry,
            ILogger logger,
            IAwardPackageChangeRequestService awardPackageChangeRequestService, 
            IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.awardPackageChangeRequestService = awardPackageChangeRequestService;
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
        }

        /// <summary>
        /// Get a list of AwardPackageChangeRequests assigned to the given student 
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.FINANCIAL.AID.INFORMATION permission 
        /// or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">The Colleague PERSON id for whom the AwardPackageChangeRequests apply</param>
        /// <returns>A list of AwardPackageChangeRequests</returns>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/award-package-change-requests", 1, true, Name = "GetStudentAwardPackageChangeRequests")]
        public async Task<ActionResult<IEnumerable<AwardPackageChangeRequest>>> GetAwardPackageChangeRequestsAsync([FromRoute] string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null or empty");
            }

            try
            {
                return Ok(await awardPackageChangeRequestService.GetAwardPackageChangeRequestsAsync(studentId));
            }
            catch (PermissionsException pe)
            {
                var message = "Access to AwardPackageChangeRequest resource is forbidden. See log for more details.";
                logger.LogError(pe, message);
                return CreateHttpResponseException(message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                var message = string.Format("Error getting AwardPackageChangeRequest resources for student {0}", studentId);
                logger.LogError(e, message);
                return CreateHttpResponseException(message);
            }
        }

        /// <summary>
        /// Get an AwardPackageChangeRequest with the given id that's assigned to the given studentId
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.FINANCIAL.AID.INFORMATION permission 
        /// or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">The Colleague PERSON id for whom the AwardPackageChangeRequest applies</param>
        /// <param name="requestId">The Id of the AwardPackageChangeRequest resource. Pending Change Request Ids are found in the StudentAward DTO</param>
        /// <returns>The requested AwardPackageChangeRequest</returns>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/award-package-change-requests/{requestId}", 1, true, Name = "GetStudentAwardPackageChangeRequest")]
        public async Task<ActionResult<AwardPackageChangeRequest>> GetAwardPackageChangeRequestAsync([FromRoute] string studentId, [FromRoute] string requestId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null or empty");
            }
            if (string.IsNullOrEmpty(requestId))
            {
                return CreateHttpResponseException("id cannot be null or empty");
            }

            try
            {
                return await awardPackageChangeRequestService.GetAwardPackageChangeRequestAsync(studentId, requestId);
            }
            catch (PermissionsException pe)
            {
                var message = "Access to AwardPackageChangeRequest resource is forbidden. See log for more details.";
                logger.LogError(pe, message);
                return CreateHttpResponseException(message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                var message = string.Format("Error getting AwardPackageChangeRequest resource {0} for student {1}", requestId, studentId);
                logger.LogError(e, message);
                return CreateHttpResponseException(message);
            }
        }



        /// <summary>
        /// Create a new award package change request based on the data in the awardPackageChangeRequest object form the body of the request. 
        /// All AwardPeriodChangeRequests as part of the AwardPackageChangeRequest may not be created. Clients should inspect the Status property
        /// of each AwardPeriodChangeRequest in the response to verify that its status is Pending. If its status is RejectedBySystem, inspect
        /// the StatusReason to determine why the AwardPeriodChangeRequest was rejected.
        /// </summary>
        /// <accessComments>
        /// Users may create their own data only
        /// </accessComments>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to create a change request</param>
        /// <param name="awardPackageChangeRequest">The AwardPackageChangeRequest object to create</param>
        /// <returns>HttpResponseMessage with Content of AwardPackageChangeRequest. When a change request record is successfully created, Status Code will be 201.</returns>
        /// <exception cref="HttpResponseException">400 - Some error occurred while creating the resource. See the error message for further details </exception>
        /// <exception cref="HttpResponseException">403 - You do not have the proper permission to create AwardPackageChangeRequests</exception>
        /// <exception cref="HttpResponseException">409 - An award package change request already exists for the awardYearId, studentId and awardId specified in the input AwardPackageChangeRequest object. 
        /// The location header in the response indicates the URL of the existing resource.</exception>
        [HttpPost]
        [HeaderVersionRoute("/students/{studentId}/award-package-change-requests", 1, true, Name = "CreateStudentAwardPackageChangeRequest")]
        public async Task<ActionResult<AwardPackageChangeRequest>> PostAwardPackageChangeRequestAsync([FromRoute] string studentId, [FromBody] AwardPackageChangeRequest awardPackageChangeRequest)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId cannot be null or empty");
            }
            if (awardPackageChangeRequest == null)
            {
                return CreateHttpResponseException("awardPackageChangeRequest cannot be null or empty");
            }

            try
            {
                var newAwardPackageChangeRequest = await awardPackageChangeRequestService.CreateAwardPackageChangeRequestAsync(studentId, awardPackageChangeRequest);
                var response = Created(Url.Link("GetStudentAwardPackageChangeRequest", new { studentId = studentId, requestId = newAwardPackageChangeRequest.Id }), newAwardPackageChangeRequest);
                return response;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                return CreateHttpResponseException(csee.Message, System.Net.HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                var message = string.Format("Invalid permissions for creating AwardPackageChangeRequest resource");
                logger.LogError(pe, message);
                return CreateHttpResponseException(message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (ExistingResourceException ere)
            {
                var message = string.Format("Cannot create awardPackageChangeRequest resource that already exists");
                logger.LogError(ere, message);
                SetResourceLocationHeader("GetStudentAwardPackageChangeRequest", new { studentId = studentId, requestId = ere.ExistingResourceId });
                return CreateHttpResponseException(message, System.Net.HttpStatusCode.Conflict);
            }
            catch (Exception e)
            {
                var message = string.Format("Error creating AwardPackageChangeRequest resource for student {0}: {1}", studentId, e.Message);
                logger.LogError(e, message);
                return CreateHttpResponseException(message);
            }
        }
    }
}
