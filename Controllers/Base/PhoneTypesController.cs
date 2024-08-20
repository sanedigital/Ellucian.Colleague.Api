// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Filters;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to PhoneType data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PhoneTypesController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly IPhoneTypeService _phoneTypeService;
        private readonly ILogger _logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initializes a new instance of the PhoneTypesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="phoneTypeService">Service of type<see cref="IPhoneTypeService"> IPhoneTypeService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PhoneTypesController(IAdapterRegistry adapterRegistry, IPhoneTypeService phoneTypeService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
             _phoneTypeService = phoneTypeService;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves all phone types.
        /// </summary>
        /// <returns>All <see cref="Dtos.PhoneType2">PhoneType</see> objects.</returns>
        /// 
        [HttpGet]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true }), ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/phone-types", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHeDMPhoneTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.PhoneType2>>> GetPhoneTypesAsync()
        {
            bool bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            try
            {
                var phoneTypes = await _phoneTypeService.GetPhoneTypesAsync(bypassCache);

                if (phoneTypes != null && phoneTypes.Any())
                {
                    AddEthosContextProperties(await _phoneTypeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _phoneTypeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              phoneTypes.Select(a => a.Id).ToList()));
                }
                return Ok(phoneTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves an phone type by ID.
        /// </summary>
        /// <param name="id">Unique ID representing the Phone Type to get</param>
        /// <returns>An <see cref="Dtos.PhoneType2">PhoneType</see> object.</returns>
        [HttpGet]
        [HeaderVersionRoute("/phone-types/{id}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPhoneTypeById", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.PhoneType2>> GetPhoneTypeByIdAsync(string id)
        {
            try
            {
                AddEthosContextProperties(
                   await _phoneTypeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                   await _phoneTypeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { id }));
                return await _phoneTypeService.GetPhoneTypeByGuidAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }


        /// <summary>
        /// Retrieves all phone types.
        /// </summary>
        /// <returns>All <see cref="Dtos.PhoneType">PhoneType </see>objects.</returns>
        /// <note>PhoneTypes are cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/phone-types", 1, false, Name = "GetPhoneTypes")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Base.PhoneType>>> GetAsync()
        {
            try
            {
                return Ok(await _phoneTypeService.GetBasePhoneTypesAsync());
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        #region Delete Methods
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Delete an existing Phone type in Colleague (Not Supported)
        /// </summary>
        /// <param name="id">Unique ID representing the Phone Type to delete</param>
        [HttpDelete]
        [Route("/phone-types/{id}", Name = "DeleteHeDMPhoneTypes", Order = -10)]
        public async Task<IActionResult> DeletePhoneTypesAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion

        #region Put Methods
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Update a Phone Type Record in Colleague (Not Supported)
        /// </summary>
        /// <param name="PhoneType"><see cref="PhoneType2">PhoneType</see> to update</param>
        [HttpPut]
        [HeaderVersionRoute("/phone-types/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutHeDMPhoneTypes")]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.PhoneType2>> PutPhoneTypesAsync([FromBody] Ellucian.Colleague.Dtos.PhoneType2 PhoneType)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion

        #region Post Methods
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Create a Phone Type Record in Colleague (Not Supported)
        /// </summary>
        /// <param name="PhoneType"><see cref="PhoneType">PhoneType</see> to create</param>
        [HttpPost]
        [HeaderVersionRoute("/phone-types", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostHeDMPhoneTypes")]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.PhoneType2>> PostPhoneTypesAsync([FromBody] Ellucian.Colleague.Dtos.PhoneType2 PhoneType)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion
    }
}
