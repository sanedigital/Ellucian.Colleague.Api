// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
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
using System.Linq;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;


using Ellucian.Colleague.Domain.Student;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to HousingRequests
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class HousingRequestsController : BaseCompressedApiController
    {
        private readonly IHousingRequestService _housingRequestService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the HousingRequestsController class.
        /// </summary>
        /// <param name="housingRequestService">Service of type <see cref="IHousingRequestService">IHousingRequestsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public HousingRequestsController(IHousingRequestService housingRequestService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _housingRequestService = housingRequestService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all housingRequests
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <returns>List of HousingRequests <see cref="Dtos.HousingRequest"/> objects representing matching housingRequests</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, PermissionsFilter(new string[]{StudentPermissionCodes.ViewHousingRequest, StudentPermissionCodes.CreateHousingRequest})]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/housing-requests", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHousingRequests", IsEedmSupported = true)]
        public async Task<IActionResult> GetHousingRequestsAsync(Paging page)
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
                _housingRequestService.ValidatePermissions(GetPermissionsMetaData());

                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var pageOfItems = await _housingRequestService.GetHousingRequestsAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(await _housingRequestService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _housingRequestService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.HousingRequest>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
            catch (ArgumentNullException e)
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
        /// Read (GET) a housingRequests using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired housingRequests</param>
        /// <returns>A housingRequests object <see cref="Dtos.HousingRequest"/> in EEDM format</returns>       
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { StudentPermissionCodes.ViewHousingRequest, StudentPermissionCodes.CreateHousingRequest })]
        [HttpGet]
        [HeaderVersionRoute("/housing-requests/{guid}", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHousingRequestsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.HousingRequest>> GetHousingRequestByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                _housingRequestService.ValidatePermissions(GetPermissionsMetaData());

                var bypassCache = false;
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }

                var housingRequest = await _housingRequestService.GetHousingRequestByGuidAsync(guid, bypassCache);

                if (housingRequest != null)
                {

                    AddEthosContextProperties(await _housingRequestService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _housingRequestService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { housingRequest.Id }));
                }


                return housingRequest;
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
            catch (ArgumentNullException e)
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
        /// Create (POST) a new housingRequests
        /// </summary>
        /// <param name="housingRequest">DTO of the new housingRequests</param>
        /// <returns>A housingRequests object <see cref="Dtos.HousingRequest"/> in EEDM format</returns>
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[]{StudentPermissionCodes.CreateHousingRequest})]
        [HttpPost]
        [HeaderVersionRoute("/housing-requests", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostHousingRequestsV10", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.HousingRequest>> PostHousingRequestAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.HousingRequest housingRequest)
        {            
            if (housingRequest == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null housingRequest argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }

            ////make sure the housingRequest object has an Id as it is required
            if (string.IsNullOrEmpty(housingRequest.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The id must be specified in the request body.")));
            }

            var validationResult = ValidateHousingRequest(housingRequest);
            if (validationResult != null)
            {
                return validationResult;
            }

            try
            {
                if (housingRequest.Id != Guid.Empty.ToString())
                {
                    throw new ArgumentNullException("housingRequestsDto", "On a post you can not define a GUID.");
                }

                _housingRequestService.ValidatePermissions(GetPermissionsMetaData());

                //call import extend method that needs the extracted extension data and the config
                await _housingRequestService.ImportExtendedEthosData(await ExtractExtendedData(await _housingRequestService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the housing request
                var housingRequestReturn = await _housingRequestService.CreateHousingRequestAsync(housingRequest);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _housingRequestService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _housingRequestService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { housingRequestReturn.Id }));

                return housingRequestReturn;
            }
            catch (ArgumentNullException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
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
        /// Update (PUT) an existing housingRequests
        /// </summary>
        /// <param name="guid">GUID of the housingRequests to update</param>
        /// <param name="housingRequest">DTO of the updated housingRequests</param>
        /// <returns>A housingRequests object <see cref="Dtos.HousingRequest"/> in EEDM format</returns>
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[]{StudentPermissionCodes.CreateHousingRequest})]
        [HttpPut]
        [HeaderVersionRoute("/housing-requests/{guid}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutHousingRequestsV10", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.HousingRequest>> PutHousingRequestAsync([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.HousingRequest housingRequest)
        {
            //make sure id was specified on the URL
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The id must be specified in the request URL.")));
            }

            if (housingRequest == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null housingRequest argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            
            //make sure the id on the url is not a nil one
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return CreateHttpResponseException(new IntegrationApiException("Invalid id value",
                    IntegrationApiUtility.GetDefaultApiError("Nil GUID cannot be used in PUT operation.")));
            }

            //make sure the id in the body and on the url match
            if (!string.Equals(guid, housingRequest.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                return CreateHttpResponseException(new IntegrationApiException("ID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("Id not the same as in request body.")));
            }
            
            if (string.IsNullOrEmpty(housingRequest.Id))
            {
                housingRequest.Id = guid.ToLowerInvariant();
            }

            try
            {
                _housingRequestService.ValidatePermissions(GetPermissionsMetaData());

                //get Data Privacy List
                var dpList = await _housingRequestService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _housingRequestService.ImportExtendedEthosData(await ExtractExtendedData(await _housingRequestService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic
                var housingRequestReturn = await _housingRequestService.UpdateHousingRequestAsync(guid,
                    await PerformPartialPayloadMerge(housingRequest, async () => await _housingRequestService.GetHousingRequestByGuidAsync(guid, true),
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _housingRequestService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return housingRequestReturn;               
            }            
            catch (ArgumentNullException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
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
        /// Delete (DELETE) a housingRequests
        /// </summary>
        /// <param name="guid">GUID to desired housingRequest</param>
        [HttpDelete]
        [Route("/housing-requests/{guid}", Name = "DefaultDeleteHousingRequests", Order = -10)]
        public async Task<IActionResult> DeleteHousingRequestsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Validates housing request json body
        /// </summary>
        /// <param name="housingRequest"></param>
        private ActionResult<Dtos.HousingRequest> ValidateHousingRequest(Dtos.HousingRequest housingRequest)
        {
            if (housingRequest.Person == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null Person property",
                    IntegrationApiUtility.GetDefaultApiError("The person property is a required property.")));
            }

            if ((housingRequest.Person != null && string.IsNullOrEmpty(housingRequest.Person.Id)))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null Person Id property",
                    IntegrationApiUtility.GetDefaultApiError("The person id property is a required property.")));
            }

            if (housingRequest.StartOn == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null StartOn property",
                    IntegrationApiUtility.GetDefaultApiError("The startOn property is a required property.")));
            }

            if (housingRequest.Status == Dtos.EnumProperties.HousingRequestsStatus.NotSet)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null Status property",
                    IntegrationApiUtility.GetDefaultApiError("The status property is a required property.")));
            }

            if (housingRequest.StartOn != null && housingRequest.EndOn != null && housingRequest.StartOn > housingRequest.EndOn)
            {
                return CreateHttpResponseException(new IntegrationApiException("Start date is after end date",
                   IntegrationApiUtility.GetDefaultApiError("The start date cannot be after end date.")));
            }

            //Status
            if (housingRequest.Status == Dtos.EnumProperties.HousingRequestsStatus.Approved)
            {
                return CreateHttpResponseException(new IntegrationApiException("Approved status property",
                           IntegrationApiUtility.GetDefaultApiError("The approved status is not allowed in PUT/POST.")));
            }

            //Room characteristics
            if (housingRequest.RoomCharacteristics != null && housingRequest.RoomCharacteristics.Any(i => i.Preferred != null && string.IsNullOrEmpty(i.Preferred.Id)))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null roomCharacteristic.prefered.id property",
                           IntegrationApiUtility.GetDefaultApiError("The roomCharacteristic prefered property id is required if prefered included.")));
            }
            if (housingRequest.RoomCharacteristics != null && housingRequest.RoomCharacteristics.Any(i => i.Required != null && i.Required == Dtos.EnumProperties.RequiredPreference.NotSet))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null roomCharacteristic.required property",
                           IntegrationApiUtility.GetDefaultApiError("The roomCharacteristic required property is required if included.")));
            }

            //Floor characteristics
            if (housingRequest.FloorCharacteristics != null && housingRequest.FloorCharacteristics.Preferred == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null floorCharacteristics.preferred property",
                    IntegrationApiUtility.GetDefaultApiError("The floor characteristics preferred is required if floor characteristics included.")));
            }

            if (housingRequest.FloorCharacteristics != null && housingRequest.FloorCharacteristics.Preferred != null && string.IsNullOrEmpty(housingRequest.FloorCharacteristics.Preferred.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null floorCharacteristics.preferred.id property",
                    IntegrationApiUtility.GetDefaultApiError("The floor characteristics preferred id is required if characteristics preferred included.")));
            }

            if (housingRequest.FloorCharacteristics != null && housingRequest.FloorCharacteristics.Required == Dtos.EnumProperties.RequiredPreference.NotSet)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null floorCharacteristics.required property",
                    IntegrationApiUtility.GetDefaultApiError("The floor characteristics required property is required if characteristics required included.")));
            }

            //Roommate Preferences
            if (housingRequest.RoommatePreferences != null && housingRequest.RoommatePreferences.Any(i => i.Roommate != null && i.Roommate.Preferred != null && string.IsNullOrEmpty(i.Roommate.Preferred.Id)))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null RoommatePreferences.preferred.id property",
                   IntegrationApiUtility.GetDefaultApiError("The roommate preferred id is required if preferred included.")));
            }
            if (housingRequest.RoommatePreferences != null && housingRequest.RoommatePreferences.Any(i => i.Roommate != null && i.Roommate.Required != null && i.Roommate.Required == Dtos.EnumProperties.RequiredPreference.NotSet))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null RoommatePreferences.required property",
                   IntegrationApiUtility.GetDefaultApiError("The roommate required property is required if required included.")));
            }
            return null;
        }
    }
}
