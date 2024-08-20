// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provide access to grade mode
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class GradeModesController :BaseCompressedApiController
    {

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the GradeModeController class.
        /// </summary>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public GradeModesController(ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this._logger = logger;
        }

     
        /// <summary>
        /// Retrieves information for all grade modes.
        /// </summary>
        /// <returns>All <see cref="Dtos.GradeMode">GradeModes</see></returns>
        [HttpGet]
        [HeaderVersionRoute("/grade-modes", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetGradeModes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.GradeMode>>> GetGradeModes2Async()
        {
            return new List<Dtos.GradeMode>();
        }

     
        /// <summary>
        /// Retrieves grade mode by id
        /// </summary>
        /// <param name="id">The id of the grade mode</param>
        /// <returns>The requested <see cref="Dtos.GradeMode">GradeMode</see></returns>
        [HttpGet]
        [HeaderVersionRoute("/grade-modes/{id}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetGradeModeById", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.GradeMode>> GetGradeModeById2Async(string id)
        {
            try
            {
                throw new ColleagueWebApiException(string.Format("No grade-modes were found for guid {0}.", id));
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), System.Net.HttpStatusCode.NotFound);
            }
        }

        /// <summary>
        /// Create a grade mode
        /// </summary>
        /// <param name="gradeMode">grade</param>
        /// <returns>A section object <see cref="Dtos.GradeMode"/> in HeDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/grade-modes", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostGradeModeV6")]
        public async Task<ActionResult<Dtos.GradeMode>> PostGradeModeAsync([FromBody] Ellucian.Colleague.Dtos.GradeMode gradeMode)
        {
            //POST is not supported for Colleague but Hedm requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update a grade mode
        /// </summary>
        /// <param name="id">desired id for a grade mode</param>
        /// <param name="gradeMode">grade mode</param>
        /// <returns>A section object <see cref="Dtos.GradeMode"/> in HeDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/grade-modes/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutGradeModeV6")]
        public async Task<ActionResult<Dtos.GradeMode>> PutGradeModeAsync([FromRoute] string id, [FromBody] Dtos.GradeMode gradeMode)
        {
            //POST is not supported for Colleague but Hedm requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) a grade mode
        /// </summary>
        /// <param name="id">id to desired grade mode</param>
        /// <returns>A section object <see cref="Dtos.GradeMode"/> in HeDM format</returns>
        [HttpDelete]
        [Route("/grade-modes/{id}", Name = "DeleteGradeMode", Order = -10)]
        public async Task<ActionResult<Dtos.GradeMode>> DeleteGradeModeByIdAsync(string id)
        {
            //Delete is not supported for Colleague but Hedm requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
