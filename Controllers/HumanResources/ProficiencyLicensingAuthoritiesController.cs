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
    /// Provides access to ProficiencyLicensingAuthorities
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class ProficiencyLicensingAuthoritiesController : BaseCompressedApiController
    {

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the ProficiencyLicensingAuthoritiesController class.
        /// </summary>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public ProficiencyLicensingAuthoritiesController(ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all proficiency-licensing-authorities
        /// </summary>
        /// <returns>All <see cref="ProficiencyLicensingAuthority">ProficiencyLicensingAuthorities</see></returns>
        [HttpGet]
        [HeaderVersionRoute("/proficiency-licensing-authorities", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetProficiencyLicensingAuthorities", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<ProficiencyLicensingAuthority>>> GetProficiencyLicensingAuthoritiesAsync()
        {
            return new List<ProficiencyLicensingAuthority>();
        }

        /// <summary>
        /// Retrieve (GET) an existing proficiency-licensing-authorities
        /// </summary>
        /// <param name="guid">GUID of the proficiency-licensing-authority to get</param>
        /// <returns>A proficiencyLicensingAuthority object <see cref="ProficiencyLicensingAuthority"/> in EEDM format</returns>
        [HttpGet]
        [HeaderVersionRoute("/proficiency-licensing-authorities/{guid}", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetProficiencyLicensingAuthorityByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<ProficiencyLicensingAuthority>> GetProficiencyLicensingAuthorityByGuidAsync([FromRoute] string guid)
        {
            try
            {
                throw new ColleagueWebApiException(string.Format("No proficiency-licensing-authorities was found for guid {0}.", guid));
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
        }
        /// <summary>
        /// Create (POST) a new proficiencyLicensingAuthority
        /// </summary>
        /// <param name="proficiencyLicensingAuthority">DTO of the new proficiencyLicensingAuthority</param>
        /// <returns>A proficiencyLicensingAuthority object <see cref="ProficiencyLicensingAuthority"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/proficiency-licensing-authorities", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostProficiencyLicensingAuthoritiesV10")]
        public async Task<ActionResult<ProficiencyLicensingAuthority>> PostProficiencyLicensingAuthorityAsync([FromBody] ProficiencyLicensingAuthority proficiencyLicensingAuthority)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing proficiencyLicensingAuthority
        /// </summary>
        /// <param name="guid">GUID of the proficiencyLicensingAuthorities to update</param>
        /// <param name="proficiencyLicensingAuthority">DTO of the updated proficiencyLicensingAuthority</param>
        /// <returns>A proficiencyLicensingAuthority object <see cref="ProficiencyLicensingAuthority"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/proficiency-licensing-authorities/{guid}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutProficiencyLicensingAuthoritiesV10")]
        public async Task<ActionResult<ProficiencyLicensingAuthority>> PutProficiencyLicensingAuthorityAsync([FromRoute] string guid, [FromBody] ProficiencyLicensingAuthority proficiencyLicensingAuthority)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a proficiencyLicensingAuthorities
        /// </summary>
        /// <param name="guid">GUID to desired proficiencyLicensingAuthority</param>
        [HttpDelete]
        [Route("/proficiency-licensing-authorities/{guid}", Name = "DefaultDeleteProficiencyLicensingAuthorities")]
        public async Task<IActionResult> DeleteProficiencyLicensingAuthorityAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}
