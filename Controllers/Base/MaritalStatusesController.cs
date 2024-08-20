// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Coordination.Base.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Marital Status data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    [Route("/[controller]/[action]")]
    public class MaritalStatusesController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly IDemographicService _demographicService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the MaritalStatusesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="demographicService">Service of type <see cref="IDemographicService">IDemographicService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public MaritalStatusesController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, IDemographicService demographicService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _demographicService = demographicService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves all Marital Statuses.
        /// </summary>
        /// <returns>All <see cref="MaritalStatus">Marital Status codes and descriptions.</see></returns>
        /// <note>MaritalStatus is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/marital-statuses", 1, false, Name = "GetMaritalStatuses")]
        public async Task<ActionResult<IEnumerable<MaritalStatus>>> GetAsync()
        {
            var maritalStatusCollection = await _referenceDataRepository.MaritalStatusesAsync();

            // Get the right adapter for the type mapping
            var maritalStatusDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.MaritalStatus, MaritalStatus>();

            // Map the maritalStatus entity to the program DTO
            var maritalStatusDtoCollection = new List<MaritalStatus>();
            foreach (var maritalStatus in maritalStatusCollection)
            {
                maritalStatusDtoCollection.Add(maritalStatusDtoAdapter.MapToType(maritalStatus));
            }

            return maritalStatusDtoCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves all marital statuses.
        /// </summary>
        /// <returns>All <see cref="MaritalStatus">MaritalStatuses.</see></returns>
        [Obsolete("Obsolete as of HeDM Version 4, use Accept Header Version 4 instead.")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.MaritalStatus>>> GetMaritalStatusesAsync()
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
                return Ok(await _demographicService.GetMaritalStatusesAsync(bypassCache));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves a marital status by GUID.
        /// </summary>
        /// <returns>A <see cref="MaritalStatus">MaritalStatus.</see></returns>
        [Obsolete("Obsolete as of HeDM Version 4, use Accept Header Version 4 instead.")]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.MaritalStatus>> GetMaritalStatusByGuidAsync(string guid)
        {
            try
            {
                return await _demographicService.GetMaritalStatusByGuidAsync(guid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEEDM Version 6</remarks>
        /// <summary>
        /// Retrieves all marital statuses. If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All MaritalStatuses.</returns>
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/marital-statuses", "6.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetDefaultMaritalStatuses", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.MaritalStatus2>>> GetMaritalStatuses2Async()
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
                var maritalStatuses = await _demographicService.GetMaritalStatuses2Async(bypassCache);

                if (maritalStatuses != null && maritalStatuses.Any())
                {
                    AddEthosContextProperties(await _demographicService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _demographicService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              maritalStatuses.Select(a => a.Id).ToList()));
                }
                return Ok(maritalStatuses);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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

        /// <remarks>FOR USE WITH ELLUCIAN EEDM Version 6</remarks>
        /// <summary>
        /// Retrieves a marital status by ID.
        /// </summary>
        /// <returns>A MaritalStatus.</returns>
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/marital-statuses/{id}", "6.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetDefaultMaritalStatusById2", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.MaritalStatus2>> GetMaritalStatusById2Async(string id)
        {
            try
            {
                AddEthosContextProperties(
                   await _demographicService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                   await _demographicService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { id }));
                return await _demographicService.GetMaritalStatusById2Async(id);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
        /// Creates a Marital Status.
        /// </summary>
        /// <param name="maritalStatus"><see cref="MaritalStatus2">MaritalStatus</see> to create</param>
        /// <returns>Newly created <see cref="MaritalStatus2">MaritalStatus</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/marital-statuses", "6.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostMaritalStatusesV6_1_0")]
        public async Task<ActionResult<Dtos.MaritalStatus2>> PostMaritalStatusesAsync([FromBody] Dtos.MaritalStatus2 maritalStatus)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Updates a Marital Status.
        /// </summary>
        /// <param name="id">Id of the Marital Status to update</param>
        /// <param name="maritalStatus"><see cref="MaritalStatus2">MaritalStatus</see> to create</param>
        /// <returns>Updated <see cref="MaritalStatus2">MaritalStatus</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/marital-statuses/{id}", "6.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutMaritalStatusesV6_1_0")]
        public async Task<ActionResult<Dtos.MaritalStatus2>> PutMaritalStatusesAsync([FromRoute] string id, [FromBody] Dtos.MaritalStatus2 maritalStatus)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing Marital Status
        /// </summary>
        /// <param name="id">Id of the Marital Status to delete</param>
        [HttpDelete]
        [Route("/marital-statuses/{id}", Name = "DeleteMaritalStatuses", Order = -10)]
        public async Task<ActionResult<Dtos.MaritalStatus2>> DeleteMaritalStatusesAsync([FromRoute] string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
