// Copyright 2021-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to person data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PersonContactsController : BaseCompressedApiController
    {
        private readonly IEmergencyInformationService _emergencyInformationService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PersonContactsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="emergencyInformationService">Service of type <see cref="IPersonService">IPersonService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PersonContactsController(IAdapterRegistry adapterRegistry, IEmergencyInformationService emergencyInformationService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _emergencyInformationService = emergencyInformationService;
            this._logger = logger;
        }

        /// <summary>
        /// Gets persons emergency contacts information
        /// </summary>
        /// <param name="page"></param>
        /// <param name="person">Person id filter.</param>
        /// <returns></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, PermissionsFilter(BasePermissionCodes.ViewAnyPersonContact)]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 200 }), ServiceFilter(typeof(EedmResponseFilter))]
        [TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [ValidateQueryStringFilter(new string[] { "person"}, false, true)]
        [HeaderVersionRoute("/person-contacts", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPersonEmergencyContactsAsync", IsEedmSupported = true)]
        public async Task<IActionResult> GetPersonEmergencyContactsAsync(Paging page, [FromQuery] string person = "")
        {
            string criteria = person;

            //valid query parameter but empty argument
            if ((!string.IsNullOrEmpty(criteria)) && (string.IsNullOrEmpty(criteria.Replace("\"", ""))))
            {
                return new PagedActionResult<IEnumerable<Dtos.Employee>>(new List<Dtos.Employee>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }
            if (person == null)
            {
                return new PagedActionResult<IEnumerable<Dtos.Employee>>(new List<Dtos.Employee>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }

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
                _emergencyInformationService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                var pageOfItems = await _emergencyInformationService.GetPersonEmergencyContactsAsync(page.Offset, page.Limit, bypassCache, person);
                
                AddEthosContextProperties(
                    await _emergencyInformationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _emergencyInformationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.PersonContactSubject>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            
        }

        /// <summary>
        /// Gets persons emergency contact information
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Dtos.PersonContactSubject</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(BasePermissionCodes.ViewAnyPersonContact)]
        [HeaderVersionRoute("/person-contacts/{id}", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPersonEmergencyContactsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PersonContactSubject>> GetPersonEmergencyContactsByIdAsync([FromRoute] string id)
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
                _emergencyInformationService.ValidatePermissions(GetPermissionsMetaData());
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("Person contact id is required");
                }

                AddEthosContextProperties(
                    await _emergencyInformationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache), 
                    await _emergencyInformationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { id }));

                return Ok(await _emergencyInformationService.GetPersonEmergencyContactByIdAsync(id));
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
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
        /// Create a new person contact
        /// </summary>
        /// <param name="personContactSubject"></param>
        /// <returns></returns>
        [HttpPost]
        [HeaderVersionRoute("/person-contacts", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostPersonEmergencyContactAsync")]
        public async Task<ActionResult<Dtos.PersonContactSubject>> PostPersonContactAsync([FromBody] Dtos.PersonContactSubject personContactSubject)
        {
            //Create is not supported for Colleague but HEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update an existing person contact
        /// </summary>
        /// <param name="id"></param>
        /// <param name="personContactSubject"></param>
        /// <returns></returns>
        [HttpPut]
        [HeaderVersionRoute("/person-contacts/{id}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutPersonEmergencyContactAsync")]
        public async Task<ActionResult<Dtos.PersonContactSubject>> PutPersonContactAsync([FromRoute] string id, [FromBody] Dtos.PersonContactSubject personContactSubject)
        {
            //Update is not supported for Colleague but HEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete a person contact
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("/person-contacts/{id}", Name = "DeletePersonEmergencyContactAsync", Order = -10)]
        public async Task<IActionResult> DeletePersonContactAsync([FromRoute] string id)
        {
            //Delete is not supported for Colleague but Data Model requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
