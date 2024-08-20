// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

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
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to PersonPublications
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class PersonPublicationsController : BaseCompressedApiController
    {

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PersonPublicationsController class.
        /// </summary>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PersonPublicationsController(ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all person-publications
        /// </summary>
        /// <returns>All <see cref="Dtos.PersonPublications">PersonPublications</see></returns>
        [HttpGet]
        [HeaderVersionRoute("/person-publications", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetPersonPublications", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.PersonPublications>>> GetPersonPublicationsAsync()
        {
            return new List<Dtos.PersonPublications>();
        }

        /// <summary>
        /// Retrieve (GET) an existing person-publications
        /// </summary>
        /// <param name="guid">GUID of the person-publications to get</param>
        /// <returns>A personPublications object <see cref="Dtos.PersonPublications"/> in EEDM format</returns>
        [HttpGet]
        [HeaderVersionRoute("/person-publications/{guid}", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPersonPublicationsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PersonPublications>> GetPersonPublicationsByGuidAsync([FromRoute] string guid)
        {
            try
            {
                throw new ColleagueWebApiException(string.Format("No person-publications was found for guid {0}.", guid));
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
        }
        /// <summary>
        /// Create (POST) a new personPublications
        /// </summary>
        /// <param name="personPublications">DTO of the new personPublications</param>
        /// <returns>A personPublications object <see cref="Dtos.PersonPublications"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/person-publications", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostPersonPublicationsV10")]
        public async Task<ActionResult<Dtos.PersonPublications>> PostPersonPublicationsAsync([FromBody] Dtos.PersonPublications personPublications)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing personPublications
        /// </summary>
        /// <param name="guid">GUID of the personPublications to update</param>
        /// <param name="personPublications">DTO of the updated personPublications</param>
        /// <returns>A personPublications object <see cref="Dtos.PersonPublications"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/person-publications/{guid}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutPersonPublicationsV10")]
        public async Task<ActionResult<Dtos.PersonPublications>> PutPersonPublicationsAsync([FromRoute] string guid, [FromBody] Dtos.PersonPublications personPublications)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a personPublications
        /// </summary>
        /// <param name="guid">GUID to desired personPublications</param>
        [HttpDelete]
        [Route("/person-publications/{guid}", Name = "DefaultDeletePersonPublications")]
        public async Task<IActionResult> DeletePersonPublicationsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}
