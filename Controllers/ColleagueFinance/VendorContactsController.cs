// Copyright 2021-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.ModelBinding;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;



namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to VendorContacts
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class VendorContactsController : BaseCompressedApiController
    {
        private readonly IVendorContactsService _vendorContactsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the VendorContactsController class.
        /// </summary>
        /// <param name="vendorContactsService">Service of type <see cref="IVendorContactsService">IVendorContactsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public VendorContactsController(IVendorContactsService vendorContactsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _vendorContactsService = vendorContactsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all vendorContacts
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria"></param>
        /// <returns>List of VendorContacts <see cref="Dtos.VendorContacts"/> objects representing matching vendorContacts</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, PermissionsFilter(ColleagueFinancePermissionCodes.ViewVendorContacts)]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("criteria", typeof(VendorContacts))]
        [TypeFilter(typeof(PagingFilter), Arguments = new object[] { true, 100 })]
        [HeaderVersionRoute("/vendor-contacts", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetVendorContacts", IsEedmSupported = true)]
        public async Task<IActionResult> GetVendorContactsAsync(Paging page, QueryStringFilter criteria)
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
                _vendorContactsService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }
                var criteriaObj = GetFilterObject<Dtos.VendorContacts>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.VendorContacts>>(new List<Dtos.VendorContacts>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                var pageOfItems = await _vendorContactsService.GetVendorContactsAsync(page.Offset, page.Limit, criteriaObj, bypassCache);

                AddEthosContextProperties(
                  await _vendorContactsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _vendorContactsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.VendorContacts>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

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
        /// Read (GET) a vendorContacts using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired vendorContacts</param>
        /// <returns>A vendorContacts object <see cref="Dtos.VendorContacts"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, PermissionsFilter(ColleagueFinancePermissionCodes.ViewVendorContacts)]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/vendor-contacts/{guid}", "1.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetVendorContactsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.VendorContacts>> GetVendorContactsByGuidAsync(string guid)
        {
            var bypassCache = false;
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
                _vendorContactsService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                   await _vendorContactsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _vendorContactsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _vendorContactsService.GetVendorContactsByGuidAsync(guid);
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
        /// Create (POST) a new vendorContacts
        /// </summary>
        /// <param name="vendorContacts">DTO of the new vendorContacts</param>
        /// <returns>A vendorContacts object <see cref="Dtos.VendorContacts"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/vendor-contacts", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostVendorContactsV1.0.0")]
        public async Task<ActionResult<Dtos.VendorContacts>> PostVendorContactsAsync([FromBody] Dtos.VendorContacts vendorContacts)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update (PUT) an existing vendorContacts
        /// </summary>
        /// <param name="guid">GUID of the vendorContacts to update</param>
        /// <param name="vendorContacts">DTO of the updated vendorContacts</param>
        /// <returns>A vendorContacts object <see cref="Dtos.VendorContacts"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/vendor-contacts/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutVendorContactsV1.0.0")]
        public async Task<ActionResult<Dtos.VendorContacts>> PutVendorContactsAsync([FromRoute] string guid, [FromBody] Dtos.VendorContacts vendorContacts)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) a vendorContacts
        /// </summary>
        /// <param name="guid">GUID to desired vendorContacts</param>
        [HttpDelete]
        [Route("/vendor-contacts/{guid}", Name = "DefaultDeleteVendorContacts", Order = -10)]
        public async Task<IActionResult> DeleteVendorContactsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #region vendor-contact-initiation-process v1.0.0

        /// <summary>
        /// Create (POST) a new vendor-contact-initiation-Process.
        /// </summary>
        /// <param name="vendorContactInitiationProcess"></param>
        /// <returns></returns>
        [HttpPost]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HeaderVersionRoute("/vendor-contact-initiation-process", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostHedmVendorContactInitiationProcessV1_0_0", IsEedmSupported = true)]
        public async Task<ActionResult<object>> PostVendorContactInitiationProcessAsync([ModelBinder(typeof(EedmModelBinder))] VendorContactInitiationProcess vendorContactInitiationProcess)
        {
            try
            {
                var returnObject = await _vendorContactsService.CreateVendorContactInitiationProcessAsync(vendorContactInitiationProcess);

                var resourceName = string.Empty;
                var resourceGuid = string.Empty;
                var version = string.Empty;

                var type = returnObject.GetType();
                if (type == typeof(Dtos.VendorContacts))
                {
                    resourceName = "vendor-contacts";
                    resourceGuid = (returnObject as Dtos.VendorContacts).Id;
                    version = "1.0.0";
                }
                else
                {
                    resourceName = "person-matching-requests";
                    resourceGuid = (returnObject as Dtos.PersonMatchingRequests).Id;
                    version = "1.0.0";
                }
                string customMediaType = string.Format(IntegrationCustomMediaType, resourceName, version);
                CustomMediaTypeAttributeFilter.SetCustomMediaType(customMediaType);

                //store dataprivacy list and get the extended data to store 
                var resource = new Web.Http.EthosExtend.EthosResourceRouteInfo()
                {
                    ResourceName = resourceName,
                    ResourceVersionNumber = version,
                    BypassCache = true
                };

                AddEthosContextProperties(await _vendorContactsService.GetDataPrivacyListByApi(resourceName, true),
                   await _vendorContactsService.GetExtendedEthosDataByResource(resource, new List<string>() { resourceGuid }));

                return returnObject;
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
            catch (ConfigurationException e)
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
        /// Update a Vendor Contact Initiation Process in Colleague (Not Supported).
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="vendorContactsDto"></param>
        /// <returns></returns>
        [HttpPut]
        [HeaderVersionRoute("/vendor-contact-initiation-process/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutHedmVendorContactInitiationProcessV1_0_0")]
        public IActionResult PutVendorContactInitiationProcess([FromRoute] string guid, [FromBody] VendorContacts vendorContactsDto)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Get a Vendor Contact Initiation Process in Colleague (Not Supported).
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        [HttpGet]
        [HeaderVersionRoute("/vendor-contact-initiation-process/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHedmVendorContactInitiationProcessV1_0_0")]
        public IActionResult GetVendorContactInitiationProcess([FromRoute] string guid = null)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing Vendor Contact Initiation Process.
        /// </summary>
        /// <param name="guid"></param>
        [HttpDelete]
        [HeaderVersionRoute("/vendor-contact-initiation-process/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DeleteHedmVendorContactInitiationProcessV1_0_0")]
        public IActionResult DeleteVendorContactInitiationProcess([FromRoute] string guid)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion
    }
}
