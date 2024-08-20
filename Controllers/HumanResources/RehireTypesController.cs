// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to Rehire Types data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class RehireTypesController : BaseCompressedApiController
    {
        private readonly IRehireTypeService _rehireTypeService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the RehireTypesController class.
        /// </summary>
        /// <param name="rehireTypeService">Service of type <see cref="IDemographicService">IDemographicService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public RehireTypesController(IRehireTypeService rehireTypeService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _rehireTypeService = rehireTypeService;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Retrieves all rehire types.
        /// </summary>
        /// <returns>All RehireType objects.</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/rehire-types", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetRehireTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.RehireType>>> GetRehireTypesAsync()
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
                var allRehireTypes = await _rehireTypeService.GetRehireTypesAsync(bypassCache);

                if (allRehireTypes != null && allRehireTypes.Any())
                {
                    AddEthosContextProperties(await _rehireTypeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _rehireTypeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              allRehireTypes.Select(a => a.Id).ToList()));
                }

                return Ok(allRehireTypes);                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Retrieves a rehire type by ID.
        /// </summary>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.RehireType">RehireType.</see></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/rehire-types/{id}", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetRehireTypeById", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.RehireType>> GetRehireTypeByIdAsync(string id)
        {
            try
            {
                AddEthosContextProperties(
                    await _rehireTypeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _rehireTypeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));
                return await _rehireTypeService.GetRehireTypeByGuidAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Updates a RehireType.
        /// </summary>
        /// <param name="rehireType"><see cref="RehireType">RehireType</see> to update</param>
        /// <returns>Newly updated <see cref="RehireType">RehireType</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/rehire-types/{id}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutRehireTypesV7")]
        public async Task<ActionResult<Dtos.RehireType>> PutRehireTypeAsync([FromBody] Dtos.RehireType rehireType)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a RehireType.
        /// </summary>
        /// <param name="rehireType"><see cref="RehireType">RehireType</see> to create</param>
        /// <returns>Newly created <see cref="RehireType">RehireType</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/rehire-types", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostRehireTypesV7")]
        public async Task<ActionResult<Dtos.RehireType>> PostRehireTypeAsync([FromBody] Dtos.RehireType rehireType)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing RehireType
        /// </summary>
        /// <param name="id">Id of the RehireType to delete</param>
        [HttpDelete]
        [Route("/rehire-types/{id}", Name = "DeleteRehireTypes")]
        public async Task<IActionResult> DeleteRehireTypeAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
