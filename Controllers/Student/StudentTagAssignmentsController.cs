// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to StudentTagAssignments
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentTagAssignmentsController : BaseCompressedApiController
    {
        
        private readonly ILogger _logger;
        /// <summary>
        /// Initializes a new instance of the StudentTagAssignmentsController class.
        /// </summary>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentTagAssignmentsController(ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this._logger = logger;
        }
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all student-tag-assignments
        /// </summary>
        /// <returns>All <see cref="Dtos.StudentTagAssignments">StudentTagAssignments</see></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentTagAssignments))]
        [HttpGet]
        [HeaderVersionRoute("/student-tag-assignments", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentTagAssignments", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<StudentTagAssignments>>> GetStudentTagAssignmentsAsync()
        {
            return new List<StudentTagAssignments>();
        }
        /// <summary>
        /// Retrieve (GET) an existing student-tag-assignments
        /// </summary>
        /// <param name="guid">GUID of the student-tag-assignments to get</param>
        /// <returns>A studentTagAssignments object <see cref="Dtos.StudentTagAssignments"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet]
        [HeaderVersionRoute("/student-tag-assignments/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentTagAssignmentsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentTagAssignments>> GetStudentTagAssignmentsByGuidAsync([FromRoute] string guid)
        {
            try
            {
                throw new ColleagueWebApiException(string.Format("No student-tag-assignments was found for guid {0}.", guid));
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
        }
        /// <summary>
        /// Create (POST) a new studentTagAssignments
        /// </summary>
        /// <param name="studentTagAssignments">DTO of the new studentTagAssignments</param>
        /// <returns>A studentTagAssignments object <see cref="Dtos.StudentTagAssignments"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        [HeaderVersionRoute("/student-tag-assignments", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentTagAssignmentsV1.0.0")]
        public async Task<ActionResult<Dtos.StudentTagAssignments>> PostStudentTagAssignmentsAsync([FromBody] Dtos.StudentTagAssignments studentTagAssignments)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        /// <summary>
        /// Update (PUT) an existing studentTagAssignments
        /// </summary>
        /// <param name="guid">GUID of the studentTagAssignments to update</param>
        /// <param name="studentTagAssignments">DTO of the updated studentTagAssignments</param>
        /// <returns>A studentTagAssignments object <see cref="Dtos.StudentTagAssignments"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut]
        [HeaderVersionRoute("/student-tag-assignments/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentTagAssignmentsV1.0.0")]
        public async Task<ActionResult<Dtos.StudentTagAssignments>> PutStudentTagAssignmentsAsync([FromRoute] string guid, [FromBody] Dtos.StudentTagAssignments studentTagAssignments)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        /// <summary>
        /// Delete (DELETE) a studentTagAssignments
        /// </summary>
        /// <param name="guid">GUID to desired studentTagAssignments</param>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpDelete]
        [Route("/student-tag-assignments/{guid}", Name = "DefaultDeleteStudentTagAssignments", Order = -10)]
        public async Task<IActionResult> DeleteStudentTagAssignmentsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
