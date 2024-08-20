// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
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
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to InstructorCategories
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class InstructorCategoriesController : BaseCompressedApiController
    {
        private readonly IInstructorCategoriesService _instructorCategoriesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the InstructorCategoriesController class.
        /// </summary>
        /// <param name="instructorCategoriesService">Service of type <see cref="IInstructorCategoriesService">IInstructorCategoriesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public InstructorCategoriesController(IInstructorCategoriesService instructorCategoriesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _instructorCategoriesService = instructorCategoriesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all instructorCategories
        /// </summary>
        /// <returns>List of InstructorCategories <see cref="Dtos.InstructorCategories"/> objects representing matching instructorCategories</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/instructor-categories", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetInstructorCategoriesV8", IsEedmSupported = true)]
        [HeaderVersionRoute("/instructor-categories", 9, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetInstructorCategories", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.InstructorCategories>>> GetInstructorCategoriesAsync()
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
                var instructorCategories = await _instructorCategoriesService.GetInstructorCategoriesAsync(bypassCache);

                if (instructorCategories != null && instructorCategories.Any())
                {
                    AddEthosContextProperties(await _instructorCategoriesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _instructorCategoriesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              instructorCategories.Select(a => a.Id).ToList()));
                }
                return Ok(instructorCategories);
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
        /// Read (GET) a instructorCategories using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired instructorCategories</param>
        /// <returns>A instructorCategories object <see cref="Dtos.InstructorCategories"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/instructor-categories/{guid}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetInstructorCategoriesByGuidV8", IsEedmSupported = true)]
        [HeaderVersionRoute("/instructor-categories/{guid}", 9, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetInstructorCategoriesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.InstructorCategories>> GetInstructorCategoriesByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddEthosContextProperties(
                    await _instructorCategoriesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _instructorCategoriesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));
                return await _instructorCategoriesService.GetInstructorCategoriesByGuidAsync(guid);
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
        /// Create (POST) a new instructorCategories
        /// </summary>
        /// <param name="instructorCategories">DTO of the new instructorCategories</param>
        /// <returns>A instructorCategories object <see cref="Dtos.InstructorCategories"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/instructor-categories", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostInstructorCategoriesV8")]
        [HeaderVersionRoute("/instructor-categories", 9, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostInstructorCategoriesV9")]
        public async Task<ActionResult<Dtos.InstructorCategories>> PostInstructorCategoriesAsync([FromBody] Dtos.InstructorCategories instructorCategories)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing instructorCategories
        /// </summary>
        /// <param name="guid">GUID of the instructorCategories to update</param>
        /// <param name="instructorCategories">DTO of the updated instructorCategories</param>
        /// <returns>A instructorCategories object <see cref="Dtos.InstructorCategories"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/instructor-categories/{guid}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutInstructorCategoriesV8")]
        [HeaderVersionRoute("/instructor-categories/{guid}", 9, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutInstructorCategoriesV9")]
        public async Task<ActionResult<Dtos.InstructorCategories>> PutInstructorCategoriesAsync([FromRoute] string guid, [FromBody] Dtos.InstructorCategories instructorCategories)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a instructorCategories
        /// </summary>
        /// <param name="guid">GUID to desired instructorCategories</param>
        [HttpDelete]
        [Route("/instructor-categories/{guid}", Name = "DefaultDeleteInstructorCategories", Order = -10)]
        public async Task<IActionResult> DeleteInstructorCategoriesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}
