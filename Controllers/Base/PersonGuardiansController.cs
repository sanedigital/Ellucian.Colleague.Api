// Copyright 2016 -2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Ellucian.Colleague.Domain.Base;
using Ellucian.Web.Security;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides person guardian relationships data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PersonGuardiansController : BaseCompressedApiController
    {
        private readonly IPersonGuardianRelationshipService _personGuardianRelationshipService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Person guardian constructor
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="personGuardianRelationshipService"></param>
        /// <param name="logger"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PersonGuardiansController(IAdapterRegistry adapterRegistry, IPersonGuardianRelationshipService personGuardianRelationshipService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _personGuardianRelationshipService = personGuardianRelationshipService;
            this._logger = logger;
        }

        #region GET Methods
        
        /// <summary>
        /// Retrieves active personal guardian relationship for relationship id
        /// </summary>
        /// <returns>PersonGuardianRelationship object for a personal guardian relationship.</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(BasePermissionCodes.ViewAnyPersonGuardian)]
        [HttpGet]
        [HeaderVersionRoute("/person-guardians/{id}", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPersonalGuardianRelationshipById", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PersonGuardianRelationship>> GetPersonGuardianRelationshipByIdAsync([FromRoute] string id)
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
                _personGuardianRelationshipService.ValidatePermissions(GetPermissionsMetaData());
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("Id cannot be null.");
                }
                 AddEthosContextProperties(
                 await _personGuardianRelationshipService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                 await _personGuardianRelationshipService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                     new List<string>() { id }));

                return await _personGuardianRelationshipService.GetPersonGuardianRelationshipByIdAsync(id);
            }
            catch (ArgumentNullException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
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
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Returns all or filtered records
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="person"></param>
        /// <returns>Tuple containing list of PersonGuardianRelationships <see cref="Dtos.PersonGuardianRelationship"/> objects.</returns>
        [HttpGet, PermissionsFilter(BasePermissionCodes.ViewAnyPersonGuardian)]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 200 }), ServiceFilter(typeof(EedmResponseFilter))]
        [TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [ValidateQueryStringFilter(new string[] { "person" }, false, true)]
        [HttpGet]
        [HeaderVersionRoute("/person-guardians", 7, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetPersonGuardiansRelationships", IsEedmSupported = true)]
        public async Task<IActionResult> GetPersonGuardianRelationshipsAllAndFilterAsync(Paging page, [FromQuery] string person = "")
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
                _personGuardianRelationshipService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                if (person == null)
                {
                    return new PagedActionResult<IEnumerable<Dtos.PersonGuardianRelationship>>(new List<Dtos.PersonGuardianRelationship>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
                }

                var pageOfItems = await _personGuardianRelationshipService.GetPersonGuardianRelationshipsAllAndFilterAsync(page.Offset, page.Limit, person);
                AddEthosContextProperties(
                   await _personGuardianRelationshipService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _personGuardianRelationshipService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.PersonGuardianRelationship>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        #endregion

        #region PUT method
        /// <summary>
        /// Updates personal guardian relationship
        /// </summary>
        /// <param name="id"></param>
        /// <param name="personalGuardianRelationship">personGuardianRelationship</param>
        /// <returns></returns>
        [HttpPut]
        [HeaderVersionRoute("/person-guardians/{id}", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutPersonGuardian")]
        public async Task<ActionResult<Dtos.PersonalRelationship>> PutPersonGuardianRelationshipAsync([FromRoute] string id, [FromBody] Dtos.PersonGuardianRelationship personalGuardianRelationship)
        {
            //PUT is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion

        #region POST method
        /// <summary>
        /// Create new personal guardian relationship
        /// </summary>
        /// <param name="personGuardianRelationship">personGuardianRelationship</param>
        /// <returns></returns>
        //[HttpPost]
        [HttpPost]
        [HeaderVersionRoute("/person-guardians", 7, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostPersonGuardian")]
        public async Task<ActionResult<Dtos.PersonalRelationship>> PostPersonGuardianRelationshipAsync([FromBody] Dtos.PersonGuardianRelationship personGuardianRelationship)
        {
            //POST is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));        
        }
        #endregion
        
        #region DELETE method
        /// <summary>
        /// Delete of personal guardian relationship is not supported
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("/person-guardians/{id}", Name = "DeletePersonGuardian", Order = -10)]
        public async Task<IActionResult> DeletePersonGuardianRelationshipAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion
    }
}
