// Copyright 2018-2023 Ellucian Company L.P. and its affiliates.

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
    /// Provides access to AdministrativePeriods
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AdministrativePeriodsController : BaseCompressedApiController
    {

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AdministrativePeriodsController class.
        /// </summary>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AdministrativePeriodsController(ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all person-employment-references
        /// </summary>
        /// <returns>All <see cref="AdministrativePeriod">AdministrativePeriods</see></returns>
        [HttpGet]
        [HeaderVersionRoute("/administrative-periods", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAdministrativePeriods", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<AdministrativePeriod>>> GetAdministrativePeriodsAsync()
        {
            return new List<AdministrativePeriod>();
        }

        /// <summary>
        /// Retrieve (GET) an existing person-employment-references
        /// </summary>
        /// <param name="guid">GUID of the person-employment-references to get</param>
        /// <returns>A AdministrativePeriod object <see cref="AdministrativePeriod"/> in EEDM format</returns>
        [HttpGet]
        [HeaderVersionRoute("/administrative-periods/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetAdministrativePeriodsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<AdministrativePeriod>> GetAdministrativePeriodByGuidAsync([FromRoute] string guid)
        {
            try
            {
                throw new ColleagueWebApiException(string.Format("No administrative period was found for guid {0}.", guid));
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
        }
        /// <summary>
        /// Create (POST) a new AdministrativePeriod
        /// </summary>
        /// <param name="AdministrativePeriod">DTO of the new AdministrativePeriod</param>
        /// <returns>A AdministrativePeriod object <see cref="AdministrativePeriod"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/administrative-periods", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostAdministrativePeriodsV20")]
        public async Task<ActionResult<AdministrativePeriod>> PostAdministrativePeriodsAsync([FromBody] AdministrativePeriod AdministrativePeriod)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing AdministrativePeriod
        /// </summary>
        /// <param name="guid">GUID of the AdministrativePeriod to update</param>
        /// <param name="AdministrativePeriod">DTO of the updated AdministrativePeriod</param>
        /// <returns>A AdministrativePeriod object <see cref="AdministrativePeriod"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/administrative-periods/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutAdministrativePeriodsV20")]
        public async Task<ActionResult<AdministrativePeriod>> PutAdministrativePeriodAsync([FromRoute] string guid, [FromBody] AdministrativePeriod AdministrativePeriod)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a AdministrativePeriod
        /// </summary>
        /// <param name="guid">GUID to desired AdministrativePeriod</param>
        [HttpDelete]
        [Route("/administrative-periods/{guid}", Name = "DefaultDeleteAdministrativePeriods")]
        public async Task<IActionResult> DeleteAdministrativePeriodAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}
