// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to ResidencyType data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class ResidencyTypesController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly IStudentService _studentService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the ResidencyTypesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="studentService">Service of type<see cref="IStudentService"> IResidencyTypesService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public ResidencyTypesController(IAdapterRegistry adapterRegistry, IStudentService studentService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _studentService = studentService;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves all Residency types.
        /// </summary>
        /// <returns>All <see cref="Dtos.ResidentType">ResidencyTypes</see> objects.</returns>
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true }), ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/residency-types", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetResidencyTypesV7", IsEedmSupported = true)]
        [HeaderVersionRoute("/residency-types", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetResidencyTypesV6", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.ResidentType>>> GetResidencyTypesAsync()
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
                var items = await _studentService.GetResidentTypesAsync(bypassCache);

                if (items != null && items.Any())
                {
                    AddEthosContextProperties(await _studentService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                      await _studentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      items.Select(a => a.Id).ToList()));
                }

                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves an Residency type by ID.
        /// </summary>
        /// <param name="guid">Unique ID representing the Residency Type to get</param>
        /// <returns>An <see cref="Dtos.ResidentType">ResidentType</see> object.</returns>
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/residency-types/{guid}", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultResidencyTypeByGuid", IsEedmSupported = true)]
        [HeaderVersionRoute("/residency-types/{guid}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetResidencyTypeByIdAsyncV6", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.ResidentType>> GetResidencyTypeByIdAsync(string guid)
        {
            bool bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }

            try
            {
                var item = await _studentService.GetResidentTypeByIdAsync(guid);

                if (item != null)
                {
                    AddEthosContextProperties(await _studentService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                      await _studentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { item.Id }));
                }

                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        #region Delete Methods
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Delete an existing Residency type in Colleague (Not Supported)
        /// </summary>
        /// <param name="guid">Unique guid ID representing the Residency Type to delete</param>
        [HttpDelete]
        [Route("/residency-types/{guid}", Name = "DefaultDeleteResidencyType", Order = -10)]
        public async Task<IActionResult> DeleteResidencyTypeAsync(string guid)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion

        #region Put Methods
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Update a Residency Type Record in Colleague (Not Supported)
        /// </summary>
        /// <param name="residencyType"><see cref="ResidentType">ResidencyType</see> to update</param>
        [HttpPut]
        [Route("/residency-types/{id}", Name = "DefaultPutResidencyTypeV6", Order = -10)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.ResidentType>> PutResidencyTypeAsync([FromBody] Ellucian.Colleague.Dtos.ResidentType residencyType)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion

        #region Post Methods
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Create a Residency Type Record in Colleague (Not Supported)
        /// </summary>
        /// <param name="residencyType"><see cref="ResidentType">ResidencyTypes</see> to create</param>
        [HttpPost]
        [Route("/residency-types", Name = "DefaultPostResidencyType", Order = -10)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.ResidentType>> PostResidencyTypeAsync([FromBody] Ellucian.Colleague.Dtos.ResidentType residencyType)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion
    }
}
