// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.Filters;
using Ellucian.Web.Http.ModelBinding;

using Newtonsoft.Json.Linq;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Constraints;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to Vendors
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class VendorsController : BaseCompressedApiController
    {
        private readonly IVendorsService _vendorsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the VendorsController class.
        /// </summary>
        /// <param name="vendorsService">Service of type <see cref="IVendorsService">IVendorsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public VendorsController(IVendorsService vendorsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _vendorsService = vendorsService;
            this._logger = logger;
        }
        #region EEDM vendors v8
        /// <summary>
        /// Return all vendors
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria">The default named query implementation for filtering</param>
        /// <returns>List of Vendors <see cref="Vendors"/> objects representing matching vendors</returns>
        [HttpGet, PermissionsFilter(new string[] { ColleagueFinancePermissionCodes.ViewVendors, ColleagueFinancePermissionCodes.UpdateVendors })]
        [QueryStringFilterFilter("criteria", typeof(Vendors))]
        [TypeFilter(typeof(PagingFilter), Arguments = new object[] { true, 100 })]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/vendors", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetVendorsV8", IsEedmSupported = true)]
        public async Task<IActionResult> GetVendorsAsync(Paging page, QueryStringFilter criteria)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (page == null)
            {
                page = new Paging(100, 0);
            }

            var criteriaObj = GetFilterObject<Vendors>(_logger, "criteria");

            if (CheckForEmptyFilterParameters())
                return new PagedActionResult<IEnumerable<Vendors>>(new List<Vendors>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            var criteriaValue = new Dtos.Filters.VendorFilter();
            if (criteriaObj.VendorDetail != null && criteriaObj.VendorDetail.Institution != null && !string.IsNullOrEmpty(criteriaObj.VendorDetail.Institution.Id))
                criteriaValue.vendorDetail = criteriaObj.VendorDetail.Institution.Id;
            if (criteriaObj.VendorDetail != null && criteriaObj.VendorDetail.Organization != null && !string.IsNullOrEmpty(criteriaObj.VendorDetail.Organization.Id))
                criteriaValue.vendorDetail = criteriaObj.VendorDetail.Organization.Id;
            if (criteriaObj.VendorDetail != null && criteriaObj.VendorDetail.Person != null && !string.IsNullOrEmpty(criteriaObj.VendorDetail.Person.Id))
                criteriaValue.vendorDetail = criteriaObj.VendorDetail.Person.Id;
            if (criteriaObj.Classifications != null && criteriaObj.Classifications.Any() && !string.IsNullOrEmpty(criteriaObj.Classifications.FirstOrDefault().Id))
            {
                criteriaValue.classifications = criteriaObj.Classifications.FirstOrDefault().Id;
            }
            if (criteriaObj.Statuses != null && criteriaObj.Statuses.Any())
            {
                criteriaValue.statuses = new List<string>();
                foreach (var status in criteriaObj.Statuses)
                {
                    criteriaValue.statuses.Add(status.ToString());
                }
            }
            if (criteriaObj.relatedReference != null && criteriaObj.relatedReference.Any())
            {
                // Not supported in Colleague therefore always return an empty set.
                return new PagedActionResult<IEnumerable<Vendors>>(new List<Vendors>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }

            try
            {
                _vendorsService.ValidatePermissions(GetPermissionsMetaData());
                var pageOfItems = await _vendorsService.GetVendorsAsync(page.Offset, page.Limit, criteriaValue, bypassCache);

                AddEthosContextProperties(await _vendorsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _vendorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Vendors>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
        /// Read (GET) a vendor using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired vendor</param>
        /// <returns>A vendor object <see cref="Dtos.Vendors"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { ColleagueFinancePermissionCodes.ViewVendors, ColleagueFinancePermissionCodes.UpdateVendors })]
        [HttpGet]
        [HeaderVersionRoute("/vendors/{guid}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetVendorsByGuidV8", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Vendors>> GetVendorsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }

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
                _vendorsService.ValidatePermissions(GetPermissionsMetaData());
                var vendor = await _vendorsService.GetVendorsByGuidAsync(guid);

                if (vendor != null)
                {

                    AddEthosContextProperties(await _vendorsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _vendorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { vendor.Id }));
                }


                return vendor;

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
        /// Create (POST) a new vendor
        /// </summary>
        /// <param name="vendor">DTO of the new vendor</param>
        /// <returns>A vendor object <see cref="Dtos.Vendors"/> in EEDM format</returns>
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(ColleagueFinancePermissionCodes.UpdateVendors)]
        [HttpPost]
        [HeaderVersionRoute("/vendors", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostVendorsV8", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Vendors>> PostVendorsAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.Vendors vendor)
        {
            if (vendor == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null vendor argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            try
            {
                _vendorsService.ValidatePermissions(GetPermissionsMetaData());
                ValidateVendor(vendor);

                var vendorDetail = vendor.VendorDetail;

                if ((vendorDetail.Institution != null) && ((string.IsNullOrEmpty(vendorDetail.Institution.Id))
                     || (string.Equals(vendorDetail.Institution.Id, Guid.Empty.ToString()))))
                {
                    throw new ArgumentNullException("Vendor.VendorDetail.Institution", "The institution id is required when submitting a vendorDetail institution. ");
                }
                if ((vendorDetail.Organization != null) && ((string.IsNullOrEmpty(vendorDetail.Organization.Id))
                     || (string.Equals(vendorDetail.Organization.Id, Guid.Empty.ToString()))))
                {
                    throw new ArgumentNullException("Vendor.VendorDetail.Organization", "The organization id is required when submitting a vendorDetail organization. ");
                }
                if ((vendorDetail.Person != null) && ((string.IsNullOrEmpty(vendorDetail.Person.Id))
                    || (string.Equals(vendorDetail.Person.Id, Guid.Empty.ToString()))))
                {
                    throw new ArgumentNullException("Vendor.VendorDetail.Person", "The person id is required when submitting a vendorDetail person. ");
                }

                //call import extend method that needs the extracted extension data and the config
                await _vendorsService.ImportExtendedEthosData(await ExtractExtendedData(await _vendorsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var vendorReturn = await _vendorsService.PostVendorAsync(vendor);

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(await _vendorsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _vendorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { vendorReturn.Id }));

                return vendorReturn;
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
        /// Update (PUT) an existing vendor
        /// </summary>
        /// <param name="guid">GUID of the vendor to update</param>
        /// <param name="vendor">DTO of the updated vendor</param>
        /// <returns>A vendor object <see cref="Dtos.Vendors"/> in EEDM format</returns>
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(ColleagueFinancePermissionCodes.UpdateVendors)]
        [HttpPut]
        [HeaderVersionRoute("/vendors/{guid}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutVendorsV8", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Vendors>> PutVendorsAsync([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.Vendors vendor)
        {
            var validationResult = ValidateUpdateRequest(guid, vendor);
            if (validationResult != null)
            {
                return validationResult;
            }
            try
            {
                _vendorsService.ValidatePermissions(GetPermissionsMetaData());
                //get Data Privacy List
                var dpList = await _vendorsService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _vendorsService.ImportExtendedEthosData(await ExtractExtendedData(await _vendorsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var mergedVendor =
                    await PerformPartialPayloadMerge(vendor, async () => await _vendorsService.GetVendorsByGuidAsync(guid),
                    dpList, _logger);

                if (vendor.VendorDetail == null)
                {
                    throw new ArgumentNullException("The vendorDetail is required when submitting a vendor.");
                }

                if (vendor.VendorDetail.Institution != null || mergedVendor.VendorDetail.Institution != null)
                {
                    if (vendor.VendorDetail.Institution == null || mergedVendor.VendorDetail.Institution == null || vendor.VendorDetail.Institution.Id != mergedVendor.VendorDetail.Institution.Id)
                    {
                        throw new ArgumentException("Updates to vendorDetail are not permitted.");
                    }
                }

                if (vendor.VendorDetail.Organization != null || mergedVendor.VendorDetail.Organization != null)
                {
                    if (vendor.VendorDetail.Organization == null || mergedVendor.VendorDetail.Organization == null || vendor.VendorDetail.Organization.Id != mergedVendor.VendorDetail.Organization.Id)
                    {
                        throw new ArgumentException("Updates to vendorDetail are not permitted.");
                    }
                }

                if (vendor.VendorDetail.Person != null || mergedVendor.VendorDetail.Person != null)
                {
                    if (vendor.VendorDetail.Person == null || mergedVendor.VendorDetail.Person == null || vendor.VendorDetail.Person.Id != mergedVendor.VendorDetail.Person.Id)
                    {
                        throw new ArgumentException("Updates to vendorDetail are not permitted.");
                    }
                }

                ValidateVendor(mergedVendor);

                var vendorReturn = await _vendorsService.PutVendorAsync(guid, mergedVendor);

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(dpList,
                    await _vendorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return vendorReturn;
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
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
        /// Delete (DELETE) a vendor
        /// </summary>
        /// <param name="guid">GUID to desired vendor</param>
        [HttpDelete]
        [Route("/vendors/{guid}", Name = "DefaultDeleteVendors", Order = -10)]
        public async Task<IActionResult> DeleteVendorsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Helper method to validate vendors PUT/POST.
        /// </summary>
        /// <param name="vendor">Vendors DTO object of type <see cref="Dtos.Vendors"/></param>

        private void ValidateVendor(Vendors vendor)
        {
            if (vendor == null)
            {
                throw new ArgumentNullException("Vendor", "The id is required when submitting a vendor. ");
            }

            if (vendor.EndOn != null)
            {
                throw new ArgumentNullException("Vendor.EndOn", "The endOn date can not be updated when submitting a vendor. ");
            }

            if (vendor.VendorDetail == null)
            {
                throw new ArgumentNullException("Vendor.VendorDetail", "The vendorDetail is required when submitting a vendor. ");
            }

            var vendorDetail = vendor.VendorDetail;
            if ((vendorDetail.Institution == null) && (vendorDetail.Organization == null) && (vendorDetail.Person == null))
            {
                throw new ArgumentNullException("Vendor.VendorDetail", "Either a Institution, Organizatation, or Person is required when submitting a vendorDetail. ");
            }

            if ((vendorDetail.Organization != null) && ((vendorDetail.Person != null) || (vendorDetail.Institution != null)))
            {
                throw new ArgumentNullException("Vendor.VendorDetail", "Only one of either an organization, person or institution can be specified as a vendor. ");
            }
            if ((vendorDetail.Person != null) && ((vendorDetail.Organization != null) || (vendorDetail.Institution != null)))
            {
                throw new ArgumentNullException("Vendor.VendorDetail", "Only one of either an organization, person or institution can be specified as a vendor. ");
            }
            if ((vendorDetail.Institution != null) && ((vendorDetail.Person != null) || (vendorDetail.Organization != null)))
            {
                throw new ArgumentNullException("Vendor.VendorDetail", "Only one of either an organization, person or institution can be specified as a vendor. ");
            }


            if (vendor.Classifications != null)
            {
                foreach (var classification in vendor.Classifications)
                {
                    if (string.IsNullOrEmpty(classification.Id))
                        throw new ArgumentNullException("Vendor.Classification", "The classification id is required when submitting classifications. ");
                }
            }

            if (vendor.PaymentTerms != null)
            {
                foreach (var paymentTerm in vendor.PaymentTerms)
                {
                    if (string.IsNullOrEmpty(paymentTerm.Id))
                        throw new ArgumentNullException("Vendor.PaymentTerms", "The paymentTerms id is required when submitting paymentTerms. ");
                }
            }

            if (vendor.VendorHoldReasons != null)
            {
                foreach (var vendorHoldReason in vendor.VendorHoldReasons)
                {
                    if (string.IsNullOrEmpty(vendorHoldReason.Id))
                    {
                        throw new ArgumentNullException("Vendor.VendorHoldReasons", "The vendorHoldReason id is required when submitting vendorHoldReasons. ");
                    }
                }
            }

        }


        /// <summary>
        /// Validate the request on Put meets conditions for guid consistency 
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="request"></param>
        private ActionResult<Dtos.Vendors> ValidateUpdateRequest(string guid, BaseModel2 request)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (request == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(request.Id))
            {
                request.Id = guid.ToLowerInvariant();
            }
            else if ((string.Equals(guid, Guid.Empty.ToString())) || (string.Equals(request.Id, Guid.Empty.ToString())))
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID empty",
                    IntegrationApiUtility.GetDefaultApiError("GUID must be specified.")));
            }
            else if (guid.ToLowerInvariant() != request.Id.ToLowerInvariant())
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }
            return null;
        }

        #endregion

        #region EEDM Vendors v11.1.0

        /// <summary>
        /// Return all vendors
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria">The default named query implementation for filtering</param>
        ///  <param name="vendorDetail">Vendor detail id GUId filter as in person or organization or institution guid</param>
        /// <returns>List of Vendors <see cref="Dtos.Vendors2"/> objects representing matching vendors</returns>
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { ColleagueFinancePermissionCodes.ViewVendors, ColleagueFinancePermissionCodes.UpdateVendors })]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("criteria", typeof(Vendors2))]
        [QueryStringFilterFilter("vendorDetail", typeof(Dtos.Filters.VendorDetail))]
        [TypeFilter(typeof(PagingFilter), Arguments = new object[] { true, 100 })]

        [HttpGet]
        [HeaderVersionRoute("/vendors", "11.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetVendors", IsEedmSupported = true)]
        public async Task<IActionResult> GetVendorsAsync2(Paging page, QueryStringFilter criteria, QueryStringFilter vendorDetail)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (page == null)
            {
                page = new Paging(100, 0);
            }

            string vendorDetails = "";
            string vendorDetailFilterValue = string.Empty;
            var vendorDetailFilterObj = GetFilterObject<Dtos.Filters.VendorDetail>(_logger, "vendorDetail");
            if ((vendorDetailFilterObj != null) && (vendorDetailFilterObj.vendorDetail != null))
            {
                vendorDetails = vendorDetailFilterObj.vendorDetail.Id;
            }


            List<string> relatedReferences = null, statuses = null, classifications = null, types = null;
            string taxId = string.Empty;

            var criteriaObj = GetFilterObject<Vendors2>(_logger, "criteria");

            if (CheckForEmptyFilterParameters())
                return new PagedActionResult<IEnumerable<Vendors2>>(new List<Vendors2>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            if (criteriaObj.VendorDetail != null && criteriaObj.VendorDetail.Institution != null && !string.IsNullOrEmpty(criteriaObj.VendorDetail.Institution.Id))
                vendorDetails = criteriaObj.VendorDetail.Institution.Id;
            if (criteriaObj.VendorDetail != null && criteriaObj.VendorDetail.Organization != null && !string.IsNullOrEmpty(criteriaObj.VendorDetail.Organization.Id))
                vendorDetails = criteriaObj.VendorDetail.Organization.Id;
            if (criteriaObj.VendorDetail != null && criteriaObj.VendorDetail.Person != null && !string.IsNullOrEmpty(criteriaObj.VendorDetail.Person.Id))
                vendorDetails = criteriaObj.VendorDetail.Person.Id;
            if (criteriaObj.Classifications != null && criteriaObj.Classifications.Any())
            {
                classifications = criteriaObj.Classifications.Select(vc => vc.Id).ToList();
            }
            if (criteriaObj.Statuses != null && criteriaObj.Statuses.Any())
            {
                statuses = new List<string>();
                foreach (var status in criteriaObj.Statuses)
                {
                    statuses.Add(status.ToString());
                }
            }
            // to avoid breaking change, we are supporting both at this time. 
            if (criteriaObj.relatedReference != null && criteriaObj.relatedReference.Any())
            {
                relatedReferences = criteriaObj.relatedReference.Select(rr => rr.Type.ToString()).ToList();
                // We don't support "paymentVendor" therefore return an empty set.
                if (relatedReferences.Contains("PaymentVendor"))
                    return new PagedActionResult<IEnumerable<Vendors2>>(new List<Vendors2>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }
            if (criteriaObj.RelatedVendor != null && criteriaObj.RelatedVendor.Any())
            {
                relatedReferences = criteriaObj.RelatedVendor.Select(rr => rr.Type.ToString()).ToList();
                // We don't support "paymentVendor" therefore return an empty set.
                if (relatedReferences.Contains("PaymentVendor"))
                    return new PagedActionResult<IEnumerable<Vendors2>>(new List<Vendors2>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }
            if (criteriaObj.Types != null && criteriaObj.Types.Any())
            {
                types = new List<string>();
                foreach (var type in criteriaObj.Types)
                {
                    types.Add(type.ToString());
                }
            }
            if (!string.IsNullOrEmpty(criteriaObj.TaxId))
            {
                taxId = criteriaObj.TaxId;
            }

            try
            {
                _vendorsService.ValidatePermissions(GetPermissionsMetaData());
                var pageOfItems = await _vendorsService.GetVendorsAsync2(page.Offset, page.Limit, vendorDetails, classifications,
                    statuses, relatedReferences, types, taxId, bypassCache);

                AddEthosContextProperties(await _vendorsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _vendorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.Vendors2>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

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
        /// Read (GET) a vendor using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired vendor</param>
        /// <returns>A vendor object <see cref="Dtos.Vendors2"/> in EEDM format</returns>
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { ColleagueFinancePermissionCodes.ViewVendors, ColleagueFinancePermissionCodes.UpdateVendors })]

        [HttpGet]
        [HeaderVersionRoute("/vendors/{guid}", "11.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetVendorsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Vendors2>> GetVendorsByGuidAsync2(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }

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
                _vendorsService.ValidatePermissions(GetPermissionsMetaData());
                var vendor = await _vendorsService.GetVendorsByGuidAsync2(guid);

                if (vendor != null)
                {

                    AddEthosContextProperties(await _vendorsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _vendorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { vendor.Id }));
                }


                return vendor;
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
        /// Create (POST) a new vendor
        /// </summary>
        /// <param name="vendor">DTO of the new vendor</param>
        /// <returns>A vendor object <see cref="Dtos.Vendors"/> in EEDM format</returns>
        [HttpPost, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(ColleagueFinancePermissionCodes.UpdateVendors)]
        [HeaderVersionRoute("/vendors", "11.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostVendorsV11.1.0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Vendors2>> PostVendorsAsync2([ModelBinder(typeof(EedmModelBinder))] Dtos.Vendors2 vendor)
        {
            try
            {
                _vendorsService.ValidatePermissions(GetPermissionsMetaData());
                //call import extend method that needs the extracted extension data and the config
                await _vendorsService.ImportExtendedEthosData(await ExtractExtendedData(await _vendorsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var vendorReturn = await _vendorsService.PostVendorAsync2(vendor);

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(await _vendorsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _vendorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { vendorReturn.Id }));

                return vendorReturn;
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
        /// Static helper method to convert a repository error into an integration API error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="code"></param>
        /// <param name="guid"></param>
        /// <param name="id"></param>
        /// <param name="httpStatusCode"></param>
        /// <returns>An integration API error</returns>
        private static IntegrationApiError ConvertToIntegrationApiError(string message, string code = null, string guid = null,
            string id = null, System.Net.HttpStatusCode httpStatusCode = System.Net.HttpStatusCode.BadRequest)
        {
            if (string.IsNullOrEmpty(code))
                code = "Global.Internal.Error";

            return new IntegrationApiError()
            {
                Code = code,
                Message = message,
                Guid = !string.IsNullOrEmpty(guid) ? guid : null,
                Id = !string.IsNullOrEmpty(id) ? id : null,
                StatusCode = httpStatusCode
            };
        }

        /// <summary>
        /// Update (PUT) an existing vendor
        /// </summary>
        /// <param name="guid">GUID of the vendor to update</param>
        /// <param name="vendor">DTO of the updated vendor</param>
        /// <returns>A vendor object <see cref="Dtos.Vendors2"/> in EEDM format</returns>
        [HttpPut, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(ColleagueFinancePermissionCodes.UpdateVendors)]
        [HeaderVersionRoute("/vendors/{guid}", "11.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutVendorsV11.1.0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Vendors2>> PutVendorsAsync2([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.Vendors2 vendor)
        {
            try
            {
                _vendorsService.ValidatePermissions(GetPermissionsMetaData());
                //get Data Privacy List
                var dpList = await _vendorsService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _vendorsService.ImportExtendedEthosData(await ExtractExtendedData(await _vendorsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));
                // get original DTO
                Dtos.Vendors2 origVendor = null;
                try
                {
                    origVendor = await _vendorsService.GetVendorsByGuidAsync2(guid);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to get vendor by guid");
                }

                //get the merged DTO. 

                var mergedVendor =
                    await PerformPartialPayloadMerge(vendor, origVendor, dpList, _logger);
                //issue error for those things that cannot be updated.
                if (mergedVendor != null && origVendor != null)
                {
                    var integrationApiException = new IntegrationApiException();
                    //vendor detail cannot be changed.
                    if (mergedVendor.VendorDetail == null)
                    {
                        integrationApiException.AddError(ConvertToIntegrationApiError("Updates to vendorDetail are not permitted.", "Validation.Exception", mergedVendor.Id));
                    }
                    else
                    {
                        if (origVendor.VendorDetail != null && mergedVendor.VendorDetail != null)
                        {
                            if (origVendor.VendorDetail.Institution != null && mergedVendor.VendorDetail.Institution != null && origVendor.VendorDetail.Institution.Id != mergedVendor.VendorDetail.Institution.Id)
                            {
                                integrationApiException.AddError(ConvertToIntegrationApiError("Updates to vendorDetail are not permitted.", "Validation.Exception", mergedVendor.Id));
                            }
                            if (origVendor.VendorDetail.Organization != null && mergedVendor.VendorDetail.Organization != null && origVendor.VendorDetail.Organization.Id != mergedVendor.VendorDetail.Organization.Id)
                            {
                                integrationApiException.AddError(ConvertToIntegrationApiError("Updates to vendorDetail are not permitted.", "Validation.Exception", mergedVendor.Id));
                            }
                            if (origVendor.VendorDetail.Person != null && mergedVendor.VendorDetail.Person != null && origVendor.VendorDetail.Person.Id != mergedVendor.VendorDetail.Person.Id)
                            {
                                integrationApiException.AddError(ConvertToIntegrationApiError("Updates to vendorDetail are not permitted.", "Validation.Exception", mergedVendor.Id));
                            }
                            if (origVendor.VendorDetail.Organization != null && mergedVendor.VendorDetail.Organization == null)
                            {
                                integrationApiException.AddError(ConvertToIntegrationApiError("Updates to vendorDetail are not permitted.", "Validation.Exception", mergedVendor.Id));
                            }
                            if (origVendor.VendorDetail.Person != null && mergedVendor.VendorDetail.Person == null)
                            {
                                integrationApiException.AddError(ConvertToIntegrationApiError("Updates to vendorDetail are not permitted.", "Validation.Exception", mergedVendor.Id));
                            }
                            if (origVendor.VendorDetail.Institution != null && mergedVendor.VendorDetail.Institution == null)
                            {
                                integrationApiException.AddError(ConvertToIntegrationApiError("Updates to vendorDetail are not permitted.", "Validation.Exception", mergedVendor.Id));
                            }


                        }
                    }
                    //startDate cannot be changed.
                    if (origVendor.StartOn != mergedVendor.StartOn)
                    {
                        integrationApiException.AddError(ConvertToIntegrationApiError("Update to startOn date is not permitted.", "Validation.Exception", mergedVendor.Id));
                    }
                    //default address cannot be changed.
                    if (!CompareAddress(origVendor.DefaultAddresses, mergedVendor.DefaultAddresses))
                    {
                        integrationApiException.AddError(ConvertToIntegrationApiError("The default addresses for a vendor cannot be updated.", "Validation.Exception", mergedVendor.Id));
                    }
                    if (integrationApiException != null && integrationApiException.Errors != null && integrationApiException.Errors.Any())
                    {
                        throw integrationApiException;
                    }

                }

                var vendorReturn = await _vendorsService.PutVendorAsync2(guid, mergedVendor);

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(await _vendorsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _vendorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { vendorReturn.Id }));

                return vendorReturn;
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
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

        #endregion

        #region EEDM Vendors Maximum

        /// <summary>
        /// Return all vendors
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria">The default named query implementation for filtering</param>
        ///  <param name="vendorDetail">Vendor detail id GUId filter as in person or organization or institution guid</param>
        /// <returns>List of Vendors <see cref="Dtos.VendorsMaximum"/> objects representing matching vendors</returns>
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(ColleagueFinancePermissionCodes.ViewVendors)]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("criteria", typeof(VendorsMaximum))]
        [QueryStringFilterFilter("vendorDetail", typeof(Dtos.Filters.VendorDetail))]
        [TypeFilter(typeof(PagingFilter), Arguments = new object[] { true, 100 })]

        [HttpGet]
        [HeaderVersionRoute("/vendors", "1.1.0", false, RouteConstants.HedtechIntegrationVendorsMaximumMediaTypeFormat, Name = "GetVendorsMaximumV1.1.0", IsEedmSupported = true)]
        public async Task<IActionResult> GetVendorsMaximumAsync(Paging page, QueryStringFilter criteria, QueryStringFilter vendorDetail)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (page == null)
            {
                page = new Paging(100, 0);
            }

            string vendorDetails = "";
            string vendorDetailFilterValue = string.Empty;
            var vendorDetailFilterObj = GetFilterObject<Dtos.Filters.VendorDetail>(_logger, "vendorDetail");
            if ((vendorDetailFilterObj != null) && (vendorDetailFilterObj.vendorDetail != null))
            {
                vendorDetails = vendorDetailFilterObj.vendorDetail.Id;
            }

            var criteriaObj = GetFilterObject<VendorsMaximum>(_logger, "criteria");

            if (CheckForEmptyFilterParameters())
                return new PagedActionResult<IEnumerable<VendorsMaximum>>(new List<VendorsMaximum>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            try
            {
                _vendorsService.ValidatePermissions(GetPermissionsMetaData());
                var pageOfItems = await _vendorsService.GetVendorsMaximumAsync(page.Offset, page.Limit, criteriaObj, vendorDetails, bypassCache);

                AddEthosContextProperties(await _vendorsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _vendorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.VendorsMaximum>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

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
        /// Read (GET) a vendor using a GUID
        /// </summary>
        /// <param name="id">GUID to desired vendor</param>
        /// <returns>A vendor object <see cref="Dtos.VendorsMaximum"/> in EEDM format</returns>
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(ColleagueFinancePermissionCodes.ViewVendors)]

        [HttpGet]
        [HeaderVersionRoute("/vendors/{id}", "1.1.0", false, RouteConstants.HedtechIntegrationVendorsMaximumMediaTypeFormat, Name = "GetVendorsMaximumByGuidV1.1.0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.VendorsMaximum>> GetVendorsMaximumByGuidAsync(string id)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                _vendorsService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                   await _vendorsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _vendorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { id }));
                return await _vendorsService.GetVendorsMaximumByGuidAsync(id);
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
        /// Create (POST) a new vendorsMaximum
        /// </summary>
        /// <param name="vendorsMaximum">DTO of the new vendorsMaximum</param>
        /// <returns>A vendorsMaximum object <see cref="Dtos.VendorsMaximum"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/vendors", "1.1.0", false, RouteConstants.HedtechIntegrationVendorsMaximumMediaTypeFormat, Name = "PostVendorsMaximumV1.1.0")]
        public async Task<ActionResult<Dtos.VendorsMaximum>> PostVendorsMaximumAsync([FromBody] Dtos.VendorsMaximum vendorsMaximum)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing vendorsMaximum
        /// </summary>
        /// <param name="guid">GUID of the vendorsMaximum to update</param>
        /// <param name="vendorsMaximum">DTO of the updated vendorsMaximum</param>
        /// <returns>A vendorsMaximum object <see cref="Dtos.VendorsMaximum"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/vendors/{guid}", "1.1.0", false, RouteConstants.HedtechIntegrationVendorsMaximumMediaTypeFormat, Name = "PutVendorsMaximumV1.1.0")]
        public async Task<ActionResult<Dtos.VendorsMaximum>> PutVendorsMaximumAsync([FromRoute] string guid, [FromBody] Dtos.VendorsMaximum vendorsMaximum)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a vendorsMaximum
        /// </summary>
        /// <param name="guid">GUID to desired vendorsMaximum</param>
        [HttpDelete]
        [HeaderVersionRoute("/vendors/{guid}", "1.1.0", false, RouteConstants.HedtechIntegrationVendorsMaximumMediaTypeFormat, Name = "DeleteVendorsMaximumV1.1.0")]
        public async Task<IActionResult> DeleteVendorsMaximumAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        #endregion


        #region Helper Methods
        private bool CompareAddress(List<VendorsAddressesDtoProperty> origAddrs, List<VendorsAddressesDtoProperty> mergedAddrs)
        {
            bool isEqual = true;
            // if both of them are null then they are equal.
            if (origAddrs == null && mergedAddrs == null)
            {
                return isEqual;
            }
            // if one of them is null
            if (origAddrs == null && mergedAddrs != null)
            {
                isEqual = false;
                return isEqual;
            }
            // if one of them is null
            if (origAddrs != null && mergedAddrs == null)
            {
                isEqual = false;
                return isEqual;
            }
            if (origAddrs != null && mergedAddrs != null)
            {
                //check the count to make sure they both have same number of addresses otherwise throw an exception
                if (origAddrs.Count != mergedAddrs.Count)
                {
                    isEqual = false;
                }
                else
                {
                    if (origAddrs != null && origAddrs.Any())
                    {
                        foreach (var addr in origAddrs)
                        {
                            var addrId = string.Empty;
                            var usageId = string.Empty;
                            if (addr.Address != null)
                                addrId = addr.Address.Id;
                            if (addr.Usage != null)
                                usageId = addr.Usage.Id;
                            var compAddr = mergedAddrs.Where(ad => ad.Address != null && ad.Usage != null && ad.Address.Id == addrId && ad.Usage.Id == usageId);
                            if (compAddr != null && compAddr.Count() != 1)
                            {
                                isEqual = false;
                                break;
                            }
                        }
                    }
                }
            }

            return isEqual;
        }
        /// <summary>
        /// Get the list of vendors based on keyword search.
        /// </summary>
        /// <param name="searchCriteria"> The search criteria containing keyword for vendor search.</param>
        /// <returns> The vendor search results</returns>      
        /// <accessComments>
        /// Requires at least one of the permissions VIEW.VENDOR, CREATE.UPDATE.REQUISITION and CREATE.UPDATE.PURCHASE.ORDER.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/vendors", 1, true, Name = "SearchVendors")]
        public async Task<ActionResult<IEnumerable<VendorSearchResult>>> QueryVendorsByPostAsync(VendorSearchCriteria searchCriteria)
        {
            if (searchCriteria == null)
            {
                string message = "Vendor search criteria must be specified.";
                _logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(searchCriteria.QueryKeyword))
            {
                string message = "query keyword is required to query.";
                _logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                var vendorSearchResults = await _vendorsService.QueryVendorsByPostAsync(searchCriteria);
                return Ok(vendorSearchResults);
            }
            catch (ArgumentNullException anex)
            {
                _logger.LogError(anex, "Invalid argument.");
                return CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (KeyNotFoundException knfex)
            {
                _logger.LogError(knfex, "Record not found.");
                return CreateHttpResponseException("Record not found.", HttpStatusCode.NotFound);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                _logger.LogDebug(csee, "Session expired - unable to search vendors.");
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to search vendors");
                return CreateHttpResponseException("Unable to search vendors", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get the list of vendors based on keyword search for Vouchers.
        /// </summary>
        /// <param name="searchCriteria"> The search criteria containing keyword for vendor search.</param>
        /// <returns> The vendor search results for Vouchers</returns>      
        /// <accessComments>
        /// Requires at least one of the permissions VIEW.VENDOR, CREATE.UPDATE.VOUCHER
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/vendors-voucher", 1, true, Name = "SearchVoucherVendors")]
        public async Task<ActionResult<IEnumerable<VendorsVoucherSearchResult>>> QueryVendorForVoucherAsync(VendorSearchCriteria searchCriteria)
        {
            if (searchCriteria == null)
            {
                string message = "Vendor search criteria must be specified.";
                _logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(searchCriteria.QueryKeyword))
            {
                string message = "query keyword is required to query.";
                _logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                var vendorSearchResults = await _vendorsService.QueryVendorForVoucherAsync(searchCriteria);
                return Ok(vendorSearchResults);
            }
            catch (ArgumentNullException anex)
            {
                _logger.LogError(anex, "Invalid argument.");
                return CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (KeyNotFoundException knfex)
            {
                _logger.LogError(knfex, "Record not found.");
                return CreateHttpResponseException("Record not found.", HttpStatusCode.NotFound);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                _logger.LogDebug(csee, "Session expired - unable to find vendors.");
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to find vendors.");
                return CreateHttpResponseException("Unable to find vendors.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves a vendor's tax form, box no and state to be defaulted in procurement document.
        /// </summary>
        /// <param name="vendorId">vendor id.</param>        
        /// <param name="apType">AP type.</param>
        /// <returns>VendorDefaultTaxFormInfo DTO.</returns>
        /// <accessComments>
        /// Requires at least one of the permissions VIEW.VENDOR, CREATE.UPDATE.VOUCHER, CREATE.UPDATE.REQUISITION,
        /// CREATE.UPDATE.PURCHASE.ORDER and CREATE.UPDATE.VOUCHER
        /// </accessComments>
        [HttpGet]
        [QueryStringConstraint(allowOtherKeys: true, "apType")]
        [HeaderVersionRoute("/vendors/{vendorId}/default-taxform-info", 1, true, Name = "GetVendorDefaultTaxFormInfoAsync")]
        public async Task<ActionResult<VendorDefaultTaxFormInfo>> GetVendorDefaultTaxFormInfoAsync(string vendorId, string apType)
        {
            if (string.IsNullOrEmpty(vendorId))
            {
                string message = "vendor id must be specified.";
                _logger.LogError(message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            try
            {
                var vendorDefaultTaxFormInfoDto = await _vendorsService.GetVendorDefaultTaxFormInfoAsync(vendorId, apType);
                return vendorDefaultTaxFormInfoDto;
            }
            catch (ArgumentNullException anex)
            {
                _logger.LogError(anex, "Invalid argument.");
                return CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (PermissionsException peex)
            {
                _logger.LogError(peex, "Insufficient permissions to get the vendor default tax form info.");
                return CreateHttpResponseException("Insufficient permissions to get the vendor default tax form info.", HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfex)
            {
                _logger.LogError(knfex, "Record not found.");
                return CreateHttpResponseException("Record not found.", HttpStatusCode.NotFound);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                _logger.LogDebug(csee, "Session expired - unable to populate vendor default tax form info.");
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to populate vendor default tax form info.");
                return CreateHttpResponseException("Unable to populate vendor default tax form info.", HttpStatusCode.BadRequest);
            }
        }
        #endregion
    }

}
