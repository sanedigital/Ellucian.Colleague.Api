// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to Instructors
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class InstructorsController : BaseCompressedApiController
    {
        private readonly IInstructorsService _instructorsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the InstructorsController class.
        /// </summary>
        /// <param name="instructorsService">Service of type <see cref="IInstructorsService">IInstructorsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor">Interface to action context</param>
        /// <param name="apiSettings"></param>
        public InstructorsController(IInstructorsService instructorsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _instructorsService = instructorsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all instructors
        /// </summary>
        /// <returns>List of Instructors <see cref="Dtos.Instructor"/> objects representing matching instructors</returns>
        [HttpGet]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 200 }), ServiceFilter(typeof(EedmResponseFilter))]
        [QueryStringFilterFilter("criteria", typeof(Dtos.Filters.InstructorFilter2)), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [ValidateQueryStringFilter]
        [HeaderVersionRoute("/instructors", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetInstructorsV8", IsEedmSupported = true)]
        public async Task<IActionResult> GetInstructorsAsync(Paging page, QueryStringFilter criteria = null)
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
                if (page == null)
                {
                    page = new Paging(200, 0);
                }
                
                var criteriaObj= GetFilterObject<Dtos.Filters.InstructorFilter2>(_logger, "criteria");

                if (CheckForEmptyFilterParameters()) return new PagedActionResult<IEnumerable<Dtos.Instructor>>(new List<Dtos.Instructor>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                string primaryLocationGuid = "";
                if (criteriaObj.PrimaryLocation != null)
                {
                    primaryLocationGuid = !string.IsNullOrEmpty(criteriaObj.PrimaryLocation.Id) ? criteriaObj.PrimaryLocation.Id : "";
                }

                string instructorGuid = "";
                if (criteriaObj.Instructor != null)
                {
                     instructorGuid = !string.IsNullOrEmpty(criteriaObj.Instructor.Id) ? criteriaObj.Instructor.Id : "";
                }
                
                var pageOfItems = await _instructorsService.GetInstructorsAsync(page.Offset, page.Limit, instructorGuid, primaryLocationGuid, bypassCache);

                AddEthosContextProperties(
                                    await _instructorsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                    await _instructorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                    pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.Instructor>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
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
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }


        /// <summary>
        /// Return all instructors
        /// </summary>
        /// <returns>List of Instructors <see cref="Dtos.Instructor2"/> objects representing matching instructors</returns>
        [HttpGet]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 200 }), ServiceFilter(typeof(EedmResponseFilter))]
        [QueryStringFilterFilter("criteria", typeof(Dtos.Filters.InstructorFilter2)), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [ValidateQueryStringFilter]
        [HeaderVersionRoute("/instructors", 9, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetInstructors", IsEedmSupported = true)]
        public async Task<IActionResult> GetInstructors2Async(Paging page, QueryStringFilter criteria = null)
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
                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                var criteriaObj = GetFilterObject<Dtos.Filters.InstructorFilter2>(_logger, "criteria");

                if (CheckForEmptyFilterParameters()) return new PagedActionResult<IEnumerable<Dtos.Instructor2>>(new List<Dtos.Instructor2>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                string primaryLocationGuid = "";
                if (criteriaObj.PrimaryLocation != null)
                {
                    primaryLocationGuid = !string.IsNullOrEmpty(criteriaObj.PrimaryLocation.Id) ? criteriaObj.PrimaryLocation.Id : "";
                }

                string instructorGuid = "";
                if (criteriaObj.Instructor != null)
                {
                    instructorGuid = !string.IsNullOrEmpty(criteriaObj.Instructor.Id) ? criteriaObj.Instructor.Id : "";
                }
                                
                var pageOfItems = await _instructorsService.GetInstructors2Async(page.Offset, page.Limit, instructorGuid, primaryLocationGuid, bypassCache);

                AddEthosContextProperties(
                  await _instructorsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _instructorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.Instructor2>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
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
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }
        /// <summary>
        /// Read (GET) a instructors using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired instructors</param>
        /// <returns>A instructors object <see cref="Dtos.Instructor"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/instructors/{guid}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetInstructorsByGuidV8", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Instructor>> GetInstructorsByGuidAsync(string guid)
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
                var instructor = await _instructorsService.GetInstructorByGuidAsync(guid);

                AddEthosContextProperties(
                    await _instructorsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _instructorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return instructor;
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentNullException e)
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
        /// Read (GET) a instructors using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired instructors</param>
        /// <returns>A instructors object <see cref="Dtos.Instructor2"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/instructors/{guid}", 9, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetInstructorsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Instructor2>> GetInstructorsByGuid2Async(string guid)
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
                
                var instructor = await _instructorsService.GetInstructorByGuid2Async(guid);

                AddEthosContextProperties(
                    await _instructorsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _instructorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return instructor;
                
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentNullException e)
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
        /// Create (POST) a new instructors
        /// </summary>
        /// <param name="instructor">DTO of the new instructors</param>
        /// <returns>A instructors object <see cref="Dtos.Instructor"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/instructors", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostInstructorsV8")]
        [HeaderVersionRoute("/instructors", 9, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostInstructorsV9")]
        public async Task<ActionResult<Dtos.Instructor>> PostInstructorsAsync([FromBody] Dtos.Instructor instructor)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing instructors
        /// </summary>
        /// <param name="guid">GUID of the instructors to update</param>
        /// <param name="instructor">DTO of the updated instructors</param>
        /// <returns>A instructors object <see cref="Dtos.Instructors"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/instructors/{guid}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutInstructorsV8")]
        [HeaderVersionRoute("/instructors/{guid}", 9, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutInstructorsV9")]
        public async Task<ActionResult<Dtos.Instructor>> PutInstructorsAsync([FromRoute] string guid, [FromBody] Dtos.Instructor instructor)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a instructors
        /// </summary>
        /// <param name="guid">GUID to desired instructors</param>
        [HttpDelete]
        [Route("/instructors/{guid}", Name = "DefaultDeleteInstructors", Order = -10)]
        public async Task<IActionResult> DeleteInstructorsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}
