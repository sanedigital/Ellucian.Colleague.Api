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
using Ellucian.Colleague.Dtos.HumanResources;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to EmploymentProficiencyLevels
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class EmploymentProficiencyLevelsController : BaseCompressedApiController
    {

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the EmploymentProficiencyLevelsController class.
        /// </summary>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public EmploymentProficiencyLevelsController(ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all employment-proficiency-levels
        /// </summary>
        /// <returns>All <see cref="EmploymentProficiencyLevel">EmploymentProficiencyLevels</see></returns>
        [HttpGet]
        [HeaderVersionRoute("/employment-proficiency-levels", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetEmploymentProficiencyLevels", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<EmploymentProficiencyLevel>>> GetEmploymentProficiencyLevelsAsync()
        {
            return new List<EmploymentProficiencyLevel>();
        }

        /// <summary>
        /// Retrieve (GET) an existing employment-proficiency-level
        /// </summary>
        /// <param name="guid">GUID of the employment-proficiency-levels to get</param>
        /// <returns>A employmentProficiencyLevels object <see cref="EmploymentProficiencyLevel"/> in EEDM format</returns>
        [HttpGet]
        [HeaderVersionRoute("/employment-proficiency-levels/{guid}", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetEmploymentProficiencyLevelsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<EmploymentProficiencyLevel>> GetEmploymentProficiencyLevelByGuidAsync([FromRoute] string guid)
        {
            try
            {
                throw new ColleagueWebApiException(string.Format("No employment-proficiency-levels were found for guid {0}.", guid));
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
        }
        /// <summary>
        /// Create (POST) a new employmentProficiencyLevel
        /// </summary>
        /// <param name="employmentProficiencyLevel">DTO of the new employmentProficiencyLevel</param>
        /// <returns>A employmentProficiencyLevels object <see cref="EmploymentProficiencyLevels"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/employment-proficiency-levels", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostEmploymentProficiencyLevelsV10")]
        public async Task<ActionResult<EmploymentProficiencyLevel>> PostEmploymentProficiencyLevelAsync([FromBody] EmploymentProficiencyLevel employmentProficiencyLevel)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing employmentProficiencyLevels
        /// </summary>
        /// <param name="guid">GUID of the employmentProficiencyLevels to update</param>
        /// <param name="employmentProficiencyLevel">DTO of the updated employmentProficiencyLevels</param>
        /// <returns>A employmentProficiencyLevel object <see cref="EmploymentProficiencyLevel"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/employment-proficiency-levels/{guid}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutEmploymentProficiencyLevelsV10")]
        public async Task<ActionResult<EmploymentProficiencyLevel>> PutEmploymentProficiencyLevelAsync([FromRoute] string guid, [FromBody] EmploymentProficiencyLevel employmentProficiencyLevel)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a employmentProficiencyLevels
        /// </summary>
        /// <param name="guid">GUID to desired employmentProficiencyLevel</param>
        [HttpDelete]
        [Route("/employment-proficiency-levels/{guid}", Name = "DefaultDeleteEmploymentProficiencyLevels")]
        public async Task<IActionResult> DeleteEmploymentProficiencyLevelAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}
