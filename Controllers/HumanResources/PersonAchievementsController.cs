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
    /// Provides access to PersonAchievements
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class PersonAchievementsController : BaseCompressedApiController
    {

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PersonAchievementsController class.
        /// </summary>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PersonAchievementsController(ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all person-achievements
        /// </summary>
        /// <returns>All <see cref="PersonAchievement">PersonAchievements</see></returns>
        [HttpGet]
        [HeaderVersionRoute("/person-achievements", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetPersonAchievements", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<PersonAchievement>>> GetPersonAchievementsAsync()
        {
            return new List<PersonAchievement>();
        }

        /// <summary>
        /// Retrieve (GET) an existing person-achievements
        /// </summary>
        /// <param name="guid">GUID of the person-achievements to get</param>
        /// <returns>A personAchievements object <see cref="PersonAchievement"/> in EEDM format</returns>
        [HttpGet]
        [HeaderVersionRoute("/person-achievements/{guid}", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPersonAchievementsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<PersonAchievement>> GetPersonAchievementByGuidAsync([FromRoute] string guid)
        {
            try
            {
                throw new ColleagueWebApiException(string.Format("No person-achievements was found for guid {0}.", guid));
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
        }
        /// <summary>
        /// Create (POST) a new personAchievements
        /// </summary>
        /// <param name="personAchievement">DTO of the new personAchievements</param>
        /// <returns>A personAchievement object <see cref="PersonAchievement"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/person-achievements", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostPersonAchievementsV10")]
        public async Task<ActionResult<PersonAchievement>> PostPersonAchievementAsync([FromBody] PersonAchievement personAchievement)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing personAchievements
        /// </summary>
        /// <param name="guid">GUID of the personAchievements to update</param>
        /// <param name="personAchievement">DTO of the updated personAchievement</param>
        /// <returns>A personAchievement object <see cref="PersonAchievement"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/person-achievements/{guid}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutPersonAchievementsV10")]
        public async Task<ActionResult<PersonAchievement>> PutPersonAchievementAsync([FromRoute] string guid, [FromBody] PersonAchievement personAchievement)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a personAchievements
        /// </summary>
        /// <param name="guid">GUID to desired personAchievement</param>
        [HttpDelete]
        [Route("/person-achievements/{guid}", Name = "DefaultDeletePersonAchievements")]
        public async Task<IActionResult> DeletePersonAchievementAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}
