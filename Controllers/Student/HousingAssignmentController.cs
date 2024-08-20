// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student;
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
using System.Linq;
using System.Net;
using System.Threading.Tasks;



namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to HousingAssignments
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class HousingAssignmentController : BaseCompressedApiController
    {
        private readonly IHousingAssignmentService _housingAssignmentService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the HousingAssignmentsController class.
        /// </summary>
        /// <param name="housingAssignmentService">Service of type <see cref="IHousingAssignmentService">IHousingAssignmentsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public HousingAssignmentController(IHousingAssignmentService housingAssignmentService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _housingAssignmentService = housingAssignmentService;
            this._logger = logger;
        }


        #region 16.0.0

        /// <summary>
        /// Return all housingAssignments
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria">mealplan  search criteria in JSON format</param>
        /// <returns>List of HousingAssignments <see cref="Dtos.HousingAssignment2"/> objects representing matching housingAssignments</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, PermissionsFilter(new string[] { StudentPermissionCodes.ViewHousingAssignment, StudentPermissionCodes.CreateUpdateHousingAssignment })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [QueryStringFilterFilter("criteria", typeof(Dtos.HousingAssignment2))]
        [TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/housing-assignments", "16.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHousingAssignments", IsEedmSupported = true)]
        public async Task<IActionResult> GetHousingAssignments2Async(Paging page, QueryStringFilter criteria)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            var criteriaFilter = GetFilterObject<Dtos.HousingAssignment2>(_logger, "criteria");

            if (CheckForEmptyFilterParameters())
                return new PagedActionResult<IEnumerable<Dtos.HousingAssignment2>>(new List<Dtos.HousingAssignment2>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            try
            {
                _housingAssignmentService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var pageOfItems = await _housingAssignmentService.GetHousingAssignments2Async(page.Offset, page.Limit, criteriaFilter, bypassCache);

                AddEthosContextProperties(await _housingAssignmentService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _housingAssignmentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.HousingAssignment2>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

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
        /// Read (GET) a housingAssignment using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired housingAssignment</param>
        /// <returns>A housingAssignment object <see cref="Dtos.HousingAssignment2"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { StudentPermissionCodes.ViewHousingAssignment, StudentPermissionCodes.CreateUpdateHousingAssignment })]
        [HttpGet]
        [HeaderVersionRoute("/housing-assignments/{guid}", "16.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetHousingAssignmentByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.HousingAssignment2>> GetHousingAssignmentByGuid2Async(string guid)
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
                _housingAssignmentService.ValidatePermissions(GetPermissionsMetaData());
                var housingAssignment = await _housingAssignmentService.GetHousingAssignmentByGuid2Async(guid);

                if (housingAssignment != null)
                {

                    AddEthosContextProperties(await _housingAssignmentService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _housingAssignmentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { housingAssignment.Id }));
                }

                return housingAssignment;

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
        /// Update (PUT) an existing housingAssignment
        /// </summary>
        /// <param name="guid">GUID of the housingAssignments to update</param>
        /// <param name="housingAssignment">DTO of the updated housingAssignments</param>
        /// <returns>A housingAssignments object <see cref="Dtos.HousingAssignment"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.CreateUpdateHousingAssignment)]
        [HttpPut]
        [HeaderVersionRoute("/housing-assignments/{guid}", "16.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutHousingAssignmentV1600", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.HousingAssignment2>> PutHousingAssignment2Async([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.HousingAssignment2 housingAssignment)
        {
            //make sure id was specified on the URL
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The id must be specified in the request URL.")));
            }

            if (housingAssignment == null)
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
            if (!string.Equals(guid, housingAssignment.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                return CreateHttpResponseException(new IntegrationApiException("ID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("Id not the same as in request body.")));
            }

            if (string.IsNullOrEmpty(housingAssignment.Id))
            {
                housingAssignment.Id = guid.ToLowerInvariant();
            }

            try
            {
                _housingAssignmentService.ValidatePermissions(GetPermissionsMetaData());
                //get Data Privacy List
                var dpList = await _housingAssignmentService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _housingAssignmentService.ImportExtendedEthosData(await ExtractExtendedData(await _housingAssignmentService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic
                var housingAssignmentReturn = await _housingAssignmentService.UpdateHousingAssignment2Async(guid,
                    await PerformPartialPayloadMerge(housingAssignment, async () => await _housingAssignmentService.GetHousingAssignmentByGuid2Async(guid),
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _housingAssignmentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return housingAssignmentReturn;

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
        /// Create (POST) a new housingAssignment
        /// </summary>
        /// <param name="housingAssignment">DTO of the new housingAssignments</param>
        /// <returns>A housingAssignments object <see cref="Dtos.HousingAssignment"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.CreateUpdateHousingAssignment)]
        [HttpPost]
        [HeaderVersionRoute("/housing-assignments", "16.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostHousingAssignmentV1600", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.HousingAssignment2>> PostHousingAssignment2Async([ModelBinder(typeof(EedmModelBinder))] Dtos.HousingAssignment2 housingAssignment)
        {
            if (housingAssignment == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null housingRequest argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }

            //make sure the housingRequest object has an Id as it is required
            if (string.IsNullOrEmpty(housingAssignment.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The id must be specified in the request body.")));
            }

            var validationResult = ValidateHousingAssignment2(housingAssignment);
            if (validationResult != null)
            {
                return validationResult;
            }

            try
            {
                if (housingAssignment.Id != Guid.Empty.ToString())
                {
                    throw new InvalidOperationException("On a post you can not define a GUID.");
                }

                _housingAssignmentService.ValidatePermissions(GetPermissionsMetaData());
                //call import extend method that needs the extracted extension data and the config
                await _housingAssignmentService.ImportExtendedEthosData(await ExtractExtendedData(await _housingAssignmentService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the housing assignment
                var housingAssignmentReturn = await _housingAssignmentService.CreateHousingAssignment2Async(housingAssignment);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _housingAssignmentService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _housingAssignmentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { housingAssignmentReturn.Id }));

                return housingAssignmentReturn;

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

        private ActionResult<Dtos.HousingAssignment2> ValidateHousingAssignment2(Dtos.HousingAssignment2 housingAssignment)
        {
            if (housingAssignment.Person == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null Person property",
                    IntegrationApiUtility.GetDefaultApiError("The person property is a required property.")));
            }

            if (housingAssignment.Person != null && string.IsNullOrEmpty(housingAssignment.Person.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null Person property",
                    IntegrationApiUtility.GetDefaultApiError("The person id property is a required property.")));
            }

            if (housingAssignment.Room == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null room property",
                    IntegrationApiUtility.GetDefaultApiError("The room property is required.")));
            }

            if (housingAssignment.Room != null && string.IsNullOrEmpty(housingAssignment.Room.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null room id property",
                    IntegrationApiUtility.GetDefaultApiError("The room id property is required.")));
            }

            if (!housingAssignment.StartOn.HasValue)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null startOn property",
                        IntegrationApiUtility.GetDefaultApiError("The startOn property is required.")));
            }

            if (!housingAssignment.EndOn.HasValue)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null endOn property",
                        IntegrationApiUtility.GetDefaultApiError("The endOn property is required.")));
            }

            if (housingAssignment.StartOn.HasValue && housingAssignment.EndOn.HasValue && housingAssignment.StartOn.Value > housingAssignment.EndOn.Value)
            {
                return CreateHttpResponseException(new IntegrationApiException("StartOn property",
                        IntegrationApiUtility.GetDefaultApiError("The end date cannot be earlier start date.")));
            }

            if (housingAssignment.Status == Dtos.EnumProperties.HousingAssignmentsStatus.NotSet)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null status property",
                            IntegrationApiUtility.GetDefaultApiError("The status property is required.")));
            }

            if (!housingAssignment.StatusDate.HasValue)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null statusDate property",
                            IntegrationApiUtility.GetDefaultApiError("The statusDate property is required.")));
            }
            return null;
        }
        #endregion

        /// <summary>
        /// Return all housingAssignments
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria">mealplan  search criteria in JSON format</param>
        /// <returns>List of HousingAssignments <see cref="Dtos.HousingAssignment"/> objects representing matching housingAssignments</returns>
        [HttpGet, PermissionsFilter(new string[] { StudentPermissionCodes.ViewHousingAssignment, StudentPermissionCodes.CreateUpdateHousingAssignment })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [QueryStringFilterFilter("criteria", typeof(Dtos.HousingAssignment))]
        [TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/housing-assignments", "10.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHousingAssignmentV1010", IsEedmSupported = true)]
        public async Task<IActionResult> GetHousingAssignmentsAsync(Paging page, QueryStringFilter criteria)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            var criteriaFilter = GetFilterObject<Dtos.HousingAssignment>(_logger, "criteria");

            if (CheckForEmptyFilterParameters())
                return new PagedActionResult<IEnumerable<Dtos.HousingAssignment>>(new List<Dtos.HousingAssignment>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            try
            {
                _housingAssignmentService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var pageOfItems = await _housingAssignmentService.GetHousingAssignmentsAsync(page.Offset, page.Limit, criteriaFilter, bypassCache);

                AddEthosContextProperties(await _housingAssignmentService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _housingAssignmentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.HousingAssignment>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

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
        /// Read (GET) a housingAssignment using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired housingAssignment</param>
        /// <returns>A housingAssignment object <see cref="Dtos.HousingAssignment"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { StudentPermissionCodes.ViewHousingAssignment, StudentPermissionCodes.CreateUpdateHousingAssignment })]
        [HeaderVersionRoute("/housing-assignments/{guid}", "10.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHousingAssignmentByGuidV1010", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.HousingAssignment>> GetHousingAssignmentByGuidAsync(string guid)
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
                _housingAssignmentService.ValidatePermissions(GetPermissionsMetaData());
                var housingAssignment = await _housingAssignmentService.GetHousingAssignmentByGuidAsync(guid);

                if (housingAssignment != null)
                {

                    AddEthosContextProperties(await _housingAssignmentService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _housingAssignmentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { housingAssignment.Id }));
                }


                return housingAssignment;

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
        /// Create (POST) a new housingAssignment
        /// </summary>
        /// <param name="housingAssignment">DTO of the new housingAssignments</param>
        /// <returns>A housingAssignments object <see cref="Dtos.HousingAssignment"/> in EEDM format</returns>
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.CreateUpdateHousingAssignment)]
        [HeaderVersionRoute("/housing-assignments", "10.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostHousingAssignmentV1010", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.HousingAssignment>> PostHousingAssignmentAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.HousingAssignment housingAssignment)
        {
            if (housingAssignment == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null housingRequest argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }

            //make sure the housingRequest object has an Id as it is required
            if (string.IsNullOrEmpty(housingAssignment.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The id must be specified in the request body.")));
            }

            if (housingAssignment.Id != Guid.Empty.ToString())
            {
                throw new ArgumentNullException("housingAssignmentsDto", "On a post you can not define a GUID.");
            }

            var validationResult = ValidateHousingAssignment(housingAssignment);
            if (validationResult != null)
            {
                return validationResult;
            }

            try
            {
                _housingAssignmentService.ValidatePermissions(GetPermissionsMetaData());
                //call import extend method that needs the extracted extension data and the config
                await _housingAssignmentService.ImportExtendedEthosData(await ExtractExtendedData(await _housingAssignmentService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the housing assignment
                var housingAssignmentReturn = await _housingAssignmentService.CreateHousingAssignmentAsync(housingAssignment);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _housingAssignmentService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _housingAssignmentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { housingAssignmentReturn.Id }));

                return housingAssignmentReturn;

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
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing housingAssignment
        /// </summary>
        /// <param name="guid">GUID of the housingAssignments to update</param>
        /// <param name="housingAssignment">DTO of the updated housingAssignments</param>
        /// <returns>A housingAssignments object <see cref="Dtos.HousingAssignment"/> in EEDM format</returns>
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.CreateUpdateHousingAssignment)]
        [HeaderVersionRoute("/housing-assignments/{guid}", "10.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutHousingAssignmentV1010", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.HousingAssignment>> PutHousingAssignmentAsync([FromRoute] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.HousingAssignment housingAssignment)
        {
            //make sure id was specified on the URL
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The id must be specified in the request URL.")));
            }

            if (housingAssignment == null)
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
            if (!string.Equals(guid, housingAssignment.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                return CreateHttpResponseException(new IntegrationApiException("ID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("Id not the same as in request body.")));
            }

            if (string.IsNullOrEmpty(housingAssignment.Id))
            {
                housingAssignment.Id = guid.ToLowerInvariant();
            }

            try
            {
                _housingAssignmentService.ValidatePermissions(GetPermissionsMetaData());
                //get Data Privacy List
                var dpList = await _housingAssignmentService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _housingAssignmentService.ImportExtendedEthosData(await ExtractExtendedData(await _housingAssignmentService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic
                var housingAssignmentReturn = await _housingAssignmentService.UpdateHousingAssignmentAsync(guid,
                    await PerformPartialPayloadMerge(housingAssignment, async () => await _housingAssignmentService.GetHousingAssignmentByGuidAsync(guid),
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _housingAssignmentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return housingAssignmentReturn;

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
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        private ActionResult<Dtos.HousingAssignment> ValidateHousingAssignment(Dtos.HousingAssignment housingAssignment)
        {
            if (housingAssignment.Person == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null Person property",
                    IntegrationApiUtility.GetDefaultApiError("The person property is a required property.")));
            }

            if (housingAssignment.Person != null && string.IsNullOrEmpty(housingAssignment.Person.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null Person property",
                    IntegrationApiUtility.GetDefaultApiError("The person id property is a required property.")));
            }

            if (housingAssignment.Room == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null room property",
                    IntegrationApiUtility.GetDefaultApiError("The room property is required.")));
            }

            if (housingAssignment.Room != null && string.IsNullOrEmpty(housingAssignment.Room.Id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null room id property",
                    IntegrationApiUtility.GetDefaultApiError("The room id property is required.")));
            }

            if (!housingAssignment.StartOn.HasValue)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null startOn property",
                        IntegrationApiUtility.GetDefaultApiError("The startOn property is required.")));
            }

            if (!housingAssignment.EndOn.HasValue)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null endOn property",
                        IntegrationApiUtility.GetDefaultApiError("The endOn property is required.")));
            }

            if (housingAssignment.StartOn.HasValue && housingAssignment.EndOn.HasValue && housingAssignment.StartOn.Value > housingAssignment.EndOn.Value)
            {
                return CreateHttpResponseException(new IntegrationApiException("StartOn property",
                        IntegrationApiUtility.GetDefaultApiError("The end date cannot be earlier start date.")));
            }

            if (housingAssignment.Status == Dtos.EnumProperties.HousingAssignmentsStatus.NotSet)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null status property",
                            IntegrationApiUtility.GetDefaultApiError("The status property is required.")));
            }

            if (!housingAssignment.StatusDate.HasValue)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null statusDate property",
                            IntegrationApiUtility.GetDefaultApiError("The statusDate property is required.")));
            }
            return null;
        }

        /// <summary>
        /// Delete (DELETE) a housingAssignment
        /// </summary>
        /// <param name="guid">GUID to desired housingAssignments</param>
        [HttpDelete]
        [Route("/housing-assignments/{guid}", Name = "DefaultDeleteHousingAssignment", Order = -10)]
        public async Task<IActionResult> DeleteHousingAssignmentAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}
