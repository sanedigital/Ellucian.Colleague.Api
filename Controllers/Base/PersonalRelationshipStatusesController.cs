// Copyright 2014-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to Personal Relationship Statuses data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PersonalRelationshipStatusesController : BaseCompressedApiController
    {
        private readonly IPersonalRelationshipTypeService _personalRelationshipService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PersonalRelationshipStatusesController class.
        /// </summary>
        /// <param name="personalRelationshipService">Service of type <see cref="IPersonalRelationshipTypeService">IPersonalRelationshipService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PersonalRelationshipStatusesController(IPersonalRelationshipTypeService personalRelationshipService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _personalRelationshipService = personalRelationshipService;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 6</remarks>
        /// <summary>
        /// Retrieves all personal relationship statuses.
        /// </summary>
        /// <returns>All PersonalRelationshipStatuses objects.</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/personal-relationship-statuses", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetEedmPersonalRelationshipStatuses", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.PersonalRelationshipStatus>>> GetPersonalRelationshipStatusesAsync()
        {
            try
            {
                bool bypassCache = false;
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }

                var personalRelationshipStatuses = await _personalRelationshipService.GetPersonalRelationshipStatusesAsync(bypassCache);

                if (personalRelationshipStatuses != null && personalRelationshipStatuses.Any())
                {
                    AddEthosContextProperties(await _personalRelationshipService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _personalRelationshipService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              personalRelationshipStatuses.Select(a => a.Id).ToList()));
                }

                return Ok(personalRelationshipStatuses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 6</remarks>
        /// <summary>
        /// Retrieves a personal relationship status by ID.
        /// </summary>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.PersonalRelationshipStatus">PersonalRelationshipStatus.</see></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/personal-relationship-statuses/{id}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPersonalRelationshipStatusByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.PersonalRelationshipStatus>> GetPersonalRelationshipStatusByIdAsync(string id)
        {
            try
            {
                AddEthosContextProperties(
                  await _personalRelationshipService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                  await _personalRelationshipService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { id }));
                return await _personalRelationshipService.GetPersonalRelationshipStatusByGuidAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Updates a PersonalRelationshipStatus.
        /// </summary>
        /// <param name="personalRelationshipStatus"><see cref="PersonalRelationshipStatus">PersonalRelationshipStatus</see> to update</param>
        /// <returns>Newly updated PersonalRelationshipStatus</returns>
        [HttpPut]
        [HeaderVersionRoute("/personal-relationship-statuses/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutPersonalRelationshipStatuses")]
        public async Task<ActionResult<Dtos.PersonalRelationshipStatus>> PutPersonalRelationshipStatusAsync([FromBody] Dtos.PersonalRelationshipStatus personalRelationshipStatus)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a PersonalRelationshipStatus.
        /// </summary>
        /// <param name="personalRelationshipStatus"><see cref="PersonalRelationshipStatus">PersonalRelationshipStatus</see> to create</param>
        /// <returns>Newly created <see cref="PersonalRelationshipStatus">PersonalRelationshipStatus</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/personal-relationship-statuses", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostPersonalRelationshipStatuses")]
        public async Task<ActionResult<Dtos.PersonalRelationshipStatus>> PostPersonalRelationshipStatusAsync([FromBody] Dtos.PersonalRelationshipStatus personalRelationshipStatus)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing PersonalRelationshipStatus
        /// </summary>
        /// <param name="id">Id of the PersonalRelationshipStatus to delete</param>
        [HttpDelete]
        [Route("/personal-relationship-statuses/{id}", Name = "DeletePersonalRelationshipStatuses", Order = -10)]
        public async Task<IActionResult> DeletePersonalRelationshipStatusAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
