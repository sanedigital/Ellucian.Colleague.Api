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
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to HousingResidentTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class HousingResidentTypesController : BaseCompressedApiController
    {
        private readonly IHousingResidentTypesService _housingResidentTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the HousingResidentTypesController class.
        /// </summary>
        /// <param name="housingResidentTypesService">Service of type <see cref="IHousingResidentTypesService">IHousingResidentTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public HousingResidentTypesController(IHousingResidentTypesService housingResidentTypesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _housingResidentTypesService = housingResidentTypesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all housingResidentTypes
        /// </summary>
        /// <returns>List of HousingResidentTypes <see cref="Dtos.HousingResidentTypes"/> objects representing matching housingResidentTypes</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/housing-resident-types", 8, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHousingResidentTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.HousingResidentTypes>>> GetHousingResidentTypesAsync()
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                var housingResidentTypes = await _housingResidentTypesService.GetHousingResidentTypesAsync(bypassCache);

                if (housingResidentTypes != null && housingResidentTypes.Any())
                {
                    AddEthosContextProperties(await _housingResidentTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _housingResidentTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              housingResidentTypes.Select(a => a.Id).ToList()));
                }
                return Ok(housingResidentTypes);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Read (GET) a housingResidentTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired housingResidentTypes</param>
        /// <returns>A housingResidentTypes object <see cref="Dtos.HousingResidentTypes"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/housing-resident-types/{guid}", 8, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHousingResidentTypesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.HousingResidentTypes>> GetHousingResidentTypesByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddEthosContextProperties(
                   await _housingResidentTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                   await _housingResidentTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _housingResidentTypesService.GetHousingResidentTypesByGuidAsync(guid);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create (POST) a new housingResidentTypes
        /// </summary>
        /// <param name="housingResidentTypes">DTO of the new housingResidentTypes</param>
        /// <returns>A housingResidentTypes object <see cref="Dtos.HousingResidentTypes"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/housing-resident-types", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostHousingResidentTypesV8")]
        public async Task<ActionResult<Dtos.HousingResidentTypes>> PostHousingResidentTypesAsync([FromBody] Dtos.HousingResidentTypes housingResidentTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing housingResidentTypes
        /// </summary>
        /// <param name="guid">GUID of the housingResidentTypes to update</param>
        /// <param name="housingResidentTypes">DTO of the updated housingResidentTypes</param>
        /// <returns>A housingResidentTypes object <see cref="Dtos.HousingResidentTypes"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/housing-resident-types/{guid}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutHousingResidentTypesV8")]
        public async Task<ActionResult<Dtos.HousingResidentTypes>> PutHousingResidentTypesAsync([FromRoute] string guid, [FromBody] Dtos.HousingResidentTypes housingResidentTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a housingResidentTypes
        /// </summary>
        /// <param name="guid">GUID to desired housingResidentTypes</param>
        [HttpDelete]
        [Route("/housing-resident-types/{guid}", Name = "DefaultDeleteHousingResidentTypes", Order = -10)]
        public async Task<IActionResult> DeleteHousingResidentTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}
