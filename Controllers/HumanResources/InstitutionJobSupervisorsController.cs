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
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using Newtonsoft.Json;
using Ellucian.Colleague.Domain.Base.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to InstitutionJobSupervisors
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class InstitutionJobSupervisorsController : BaseCompressedApiController
    {
        private readonly IInstitutionJobSupervisorsService _institutionJobSupervisorsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the InstitutionJobSupervisorsController class.
        /// </summary>
        /// <param name="institutionJobSupervisorsService">Service of type <see cref="IInstitutionJobSupervisorsService">IInstitutionJobSupervisorsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public InstitutionJobSupervisorsController(IInstitutionJobSupervisorsService institutionJobSupervisorsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _institutionJobSupervisorsService = institutionJobSupervisorsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all InstitutionJobSupervisors
        /// </summary>
        /// <returns>List of InstitutionJobSupervisors <see cref="Dtos.InstitutionJobSupervisors"/> objects representing matching institutionJobs</returns>
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 })]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/institution-job-supervisors", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetInstitutionJobSupervisorsV10", IsEedmSupported = true)]
        public async Task<IActionResult> GetInstitutionJobSupervisorsAsync(Paging page)
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
                    page = new Paging(100, 0);
                }

                var pageOfItems = await _institutionJobSupervisorsService.GetInstitutionJobSupervisorsAsync(page.Offset, page.Limit, bypassCache);


                AddEthosContextProperties(
                    await _institutionJobSupervisorsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _institutionJobSupervisorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobSupervisors>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            }
            catch (JsonSerializationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch
                (KeyNotFoundException e)
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
        /// Read (GET) an InstitutionJobSupervisors using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired institutionJobs</param>
        /// <returns>An InstitutionJobSupervisors DTO object <see cref="Dtos.InstitutionJobSupervisors"/> in EEDM format</returns>
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/institution-job-supervisors/{guid}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetInstitutionJobSupervisorsByGuidV10", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.InstitutionJobSupervisors>> GetInstitutionJobSupervisorsByGuidAsync(string guid)
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
                AddEthosContextProperties(
                   await _institutionJobSupervisorsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _institutionJobSupervisorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));

                return await _institutionJobSupervisorsService.GetInstitutionJobSupervisorsByGuidAsync(guid);
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
        /// Return all InstitutionJobSupervisors
        /// </summary>
        /// <returns>List of InstitutionJobSupervisors <see cref="Dtos.InstitutionJobSupervisors"/> objects representing matching institutionJobs</returns>
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 })]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/institution-job-supervisors", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetInstitutionJobSupervisors", IsEedmSupported = true)]
        public async Task<IActionResult> GetInstitutionJobSupervisors2Async(Paging page)
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
                    page = new Paging(100, 0);
                }

                var pageOfItems = await _institutionJobSupervisorsService.GetInstitutionJobSupervisors2Async(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                    await _institutionJobSupervisorsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _institutionJobSupervisorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobSupervisors>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            }
            catch (JsonSerializationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch
                (KeyNotFoundException e)
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
        /// Read (GET) an InstitutionJobSupervisors using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired institutionJobs</param>
        /// <returns>An InstitutionJobSupervisors DTO object <see cref="Dtos.InstitutionJobSupervisors"/> in EEDM format</returns>
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/institution-job-supervisors/{guid}", "11", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetInstitutionJobSupervisorsByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.InstitutionJobSupervisors>> GetInstitutionJobSupervisorsByGuid2Async(string guid)
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
                AddEthosContextProperties(
                    await _institutionJobSupervisorsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _institutionJobSupervisorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));

                return await _institutionJobSupervisorsService.GetInstitutionJobSupervisorsByGuid2Async(guid);
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
        /// Create (POST) a new institutionJobs
        /// </summary>
        /// <param name="institutionJobs">DTO of the new institutionJobs</param>
        /// <returns>An InstitutionJobSupervisors DTO object <see cref="Dtos.InstitutionJobSupervisors"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/institution-job-supervisors", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostInstitutionJobSupervisorsV11")]
        [HeaderVersionRoute("/institution-job-supervisors", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostInstitutionJobSupervisorsV10")]
        public async Task<ActionResult<Dtos.InstitutionJobSupervisors>> PostInstitutionJobSupervisorsAsync([FromBody] Dtos.InstitutionJobSupervisors institutionJobs)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing institutionJobs
        /// </summary>
        /// <param name="guid">GUID of the institutionJobs to update</param>
        /// <param name="institutionJobs">DTO of the updated institutionJobs</param>
        /// <returns>An InstitutionJobSupervisors DTO object <see cref="Dtos.InstitutionJobSupervisors"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/institution-job-supervisors/{guid}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutInstitutionJobSupervisorsV11")]
        [HeaderVersionRoute("/institution-job-supervisors/{guid}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutInstitutionJobSupervisorsV10")]
        public async Task<ActionResult<Dtos.InstitutionJobSupervisors>> PutInstitutionJobSupervisorsAsync([FromRoute] string guid, [FromBody] Dtos.InstitutionJobSupervisors institutionJobs)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a institutionJobs
        /// </summary>
        /// <param name="guid">GUID to desired institutionJobs</param>
        [HttpDelete]
        [Route("/institution-job-supervisors/{guid}", Name = "DefaultDeleteInstitutionJobSupervisors")]
        public async Task<IActionResult> DeleteInstitutionJobSupervisorsAsync(string guid)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
    
}
