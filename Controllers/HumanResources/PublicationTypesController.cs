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
    /// Provides access to PublicationTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class PublicationTypesController : BaseCompressedApiController
    {

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PublicationTypesController class.
        /// </summary>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PublicationTypesController(ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all publication-types
        /// </summary>
        /// <returns>All <see cref="Dtos.PublicationTypes">PublicationTypes</see></returns>
        [HttpGet]
        [HeaderVersionRoute("/publication-types", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetPublicationTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.PublicationTypes>>> GetPublicationTypesAsync()
        {
            return new List<Dtos.PublicationTypes>();
        }

        /// <summary>
        /// Retrieve (GET) an existing publication-types
        /// </summary>
        /// <param name="guid">GUID of the publication-types to get</param>
        /// <returns>A publicationTypes object <see cref="Dtos.PublicationTypes"/> in EEDM format</returns>
        [HttpGet]
        [HeaderVersionRoute("/publication-types/{guid}", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPublicationTypesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PublicationTypes>> GetPublicationTypesByGuidAsync([FromRoute] string guid)
        {
            try
            {
                throw new ColleagueWebApiException(string.Format("No publication-types was found for guid {0}.", guid));
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
        }
        /// <summary>
        /// Create (POST) a new publicationTypes
        /// </summary>
        /// <param name="publicationTypes">DTO of the new publicationTypes</param>
        /// <returns>A publicationTypes object <see cref="Dtos.PublicationTypes"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/publication-types", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostPublicationTypesV10")]
        public async Task<ActionResult<Dtos.PublicationTypes>> PostPublicationTypesAsync([FromBody] Dtos.PublicationTypes publicationTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update (PUT) an existing publicationTypes
        /// </summary>
        /// <param name="guid">GUID of the publicationTypes to update</param>
        /// <param name="publicationTypes">DTO of the updated publicationTypes</param>
        /// <returns>A publicationTypes object <see cref="Dtos.PublicationTypes"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/publication-types/{guid}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutPublicationTypesV10")]
        public async Task<ActionResult<Dtos.PublicationTypes>> PutPublicationTypesAsync([FromRoute] string guid, [FromBody] Dtos.PublicationTypes publicationTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) a publicationTypes
        /// </summary>
        /// <param name="guid">GUID to desired publicationTypes</param>
        [HttpDelete]
        [Route("/publication-types/{guid}", Name = "DefaultDeletePublicationTypes", Order = -10)]
        public async Task<IActionResult> DeletePublicationTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
