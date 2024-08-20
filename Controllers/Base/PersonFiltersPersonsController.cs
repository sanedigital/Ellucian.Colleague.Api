// Copyright 2023 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using System.Threading.Tasks;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Configuration;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to Person Filters data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PersonFiltersPersonsController : BaseCompressedApiController
    {
        private readonly IDemographicService _demographicService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PersonFiltersController class.
        /// </summary>
        /// <param name="demographicService">Service of type <see cref="IDemographicService">IDemographicService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="contextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PersonFiltersPersonsController(IDemographicService demographicService, ILogger logger, IActionContextAccessor contextAccessor, ApiSettings apiSettings) 
            : base(contextAccessor, apiSettings)
        {
            _demographicService = demographicService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 6</remarks>
        /// <summary>
        /// Retrieves all person filters.
        /// </summary>
        /// <returns>All PersonFilters objects.</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), ValidateQueryStringFilter(new string[] { "code", "title" })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.PersonFilter))]
        [TypeFilter(typeof(FilteringFilter), Arguments = new object[] {true})]
        [HeaderVersionRoute("/person-filters", "1.0.0", false, RouteConstants.HedtechIntegrationPersonFiltersPersonsFormat,Name = "GetPersonFilterPersonsV1_0_0", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.PersonFilter>>> GetPersonFiltersPersonsAsync(QueryStringFilter criteria)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 6.1.0</remarks>
        /// <summary>
        /// Retrieves a person filter by ID.
        /// </summary>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.Person2">PersonFilter.</see></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/person-filters/{id}", "1.0.0", false, RouteConstants.HedtechIntegrationPersonFiltersPersonsFormat, Name = "GetPersonFilterPersonsByIdV1_0_0", IsEedmSupported = true)]
        //public async Task<IEnumerable<Ellucian.Colleague.Dtos.PersonFilter>> GetPersonFilterPersonsByIdAsync(string id)
        public async Task<ActionResult<Dtos.PersonFilter2>> GetPersonFilterPersonsByIdAsync(string id)
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
                AddEthosContextProperties(
                        await _demographicService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                        await _demographicService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                            new List<string>() { id }));

                return await _demographicService.GetPersonFilterPersonsByGuidAsync(id);
            }
            catch (IntegrationApiException ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <summary>
        /// Updates a PersonFilter - not supported.
        /// </summary>
        /// <param name="personFilter"><see cref="PersonFilter">PersonFilter</see> to update</param>
        /// <returns>Newly updated <see cref="PersonFilter">PersonFilter</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/person-filters/{id}", "1.0.0", false, RouteConstants.HedtechIntegrationPersonFiltersPersonsFormat, Name = "PutPersonFilterPersonsV1_0_0")]

        public async Task<ActionResult<Dtos.PersonFilter>> PutPersonFilterPersonsAsync([FromBody] Dtos.PersonFilter personFilter)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a PersonFilter - not supported
        /// </summary>
        /// <param name="personFilter"><see cref="PersonFilter">PersonFilter</see> to create</param>
        /// <returns>Newly created <see cref="PersonFilter">PersonFilter</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/person-filters", "1.0.0", false, RouteConstants.HedtechIntegrationPersonFiltersPersonsFormat, Name = "PostPersonFilterPersonsV1_0_0")]
        public async Task<ActionResult<Dtos.PersonFilter>> PostPersonFilterPersonsAsync([FromBody] Dtos.PersonFilter personFilter)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

    }
}
