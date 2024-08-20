// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Dtos.Student;


namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to StudentTags
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentTagsController : BaseCompressedApiController
    {

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentTagsController class.
        /// </summary>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentTagsController(ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all student-tags
        /// </summary>
        /// <returns>All <see cref="StudentTag">StudentTags</see></returns>
        [HttpGet]
        [HeaderVersionRoute("/student-tags", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentTags", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<StudentTag>>> GetStudentTagsAsync()
        {
            return new List<StudentTag>();
        }

        /// <summary>
        /// Retrieve (GET) an existing student-tags
        /// </summary>
        /// <param name="guid">GUID of the student-tags to get</param>
        /// <returns>A StudentTags object <see cref="StudentTag"/> in EEDM format</returns>
        [HttpGet]
        [HeaderVersionRoute("/student-tags/{guid}", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentTagsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<StudentTag>> GetStudentTagByGuidAsync([FromRoute] string guid)
        {
            try
            {
                throw new ColleagueWebApiException(string.Format("No student-tags was found for guid {0}.", guid));
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
        }
        /// <summary>
        /// Create (POST) a new StudentTags
        /// </summary>
        /// <param name="StudentTag">DTO of the new StudentTags</param>
        /// <returns>A StudentTag object <see cref="StudentTag"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/student-tags", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentTagsV7")]
        public async Task<ActionResult<StudentTag>> PostStudentTagAsync([FromBody] StudentTag StudentTag)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing StudentTags
        /// </summary>
        /// <param name="guid">GUID of the StudentTags to update</param>
        /// <param name="StudentTag">DTO of the updated StudentTag</param>
        /// <returns>A StudentTag object <see cref="StudentTag"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/student-tags/{guid}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentTagsV7")]
        public async Task<ActionResult<StudentTag>> PutStudentTagAsync([FromRoute] string guid, [FromBody] StudentTag StudentTag)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a StudentTags
        /// </summary>
        /// <param name="guid">GUID to desired StudentTag</param>
        [HttpDelete]
        [Route("/student-tags/{guid}", Name = "DefaultDeleteStudentTags", Order = -10)]
        public async Task<IActionResult> DeleteStudentTagAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}
