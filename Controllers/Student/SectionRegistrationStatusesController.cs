// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using System.Net;
using System.Net.Http.Headers;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Coordination.Student.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Filters;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Security;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to SectionRegistrationStatuses
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class SectionRegistrationStatusesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly ICurriculumService _curriculumService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// SectionRegistrationStatusesController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="studentReferenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="curriculumService">Service of type <see cref="ICurriculumService">ICurriculumService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public SectionRegistrationStatusesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository studentReferenceDataRepository, ICurriculumService curriculumService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _curriculumService = curriculumService;
            this._logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves all SectionRegistrationStatuses.
        /// </summary>
        /// <returns>All <see cref="Ellucian.Colleague.Dtos.SectionRegistrationStatusItem2">SectionRegistrationStatus.</see></returns>
        /// 
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/section-registration-statuses", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetSectionRegistrationStatuses2V6", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.SectionRegistrationStatusItem2>>> GetSectionRegistrationStatuses2Async()
        {
            try
            {
                bool bypassCache = false;
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }
                var sectionRegistrationStatuses = await _curriculumService.GetSectionRegistrationStatuses2Async(bypassCache);

                if (sectionRegistrationStatuses != null && sectionRegistrationStatuses.Any())
                {
                    AddEthosContextProperties(await _curriculumService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _curriculumService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              sectionRegistrationStatuses.Select(a => a.Id).ToList()));
                }

                return Ok(sectionRegistrationStatuses);                
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves a SectionRegistrationStatus by ID.
        /// </summary>
        /// <param name="id">ID to desired SectionRegistrationStatus</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.SectionRegistrationStatusItem2">SectionRegistrationStatus.</see></returns>
        /// 
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/section-registration-statuses/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetSectionRegistrationStatusById2V6", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.SectionRegistrationStatusItem2>> GetSectionRegistrationStatusById2Async(string id)
        {
            try
            {
                AddEthosContextProperties(
                    await _curriculumService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _curriculumService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));
                return await _curriculumService.GetSectionRegistrationStatusById2Async(id);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH EEDM</remarks>
        /// <summary>
        /// Retrieves all SectionRegistrationStatuses.
        /// </summary>
        /// <returns>All <see cref="Ellucian.Colleague.Dtos.SectionRegistrationStatusItem3">SectionRegistrationStatus.</see></returns>
        /// 
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/section-registration-statuses", 8, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetSectionRegistrationStatuses3V8", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.SectionRegistrationStatusItem3>>> GetSectionRegistrationStatuses3Async()
        {
            try
            {
                bool bypassCache = false;
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }
                var sectionRegistrationStatuses2 = await _curriculumService.GetSectionRegistrationStatuses3Async(bypassCache);

                if (sectionRegistrationStatuses2 != null && sectionRegistrationStatuses2.Any())
                {
                    AddEthosContextProperties(await _curriculumService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _curriculumService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              sectionRegistrationStatuses2.Select(a => a.Id).ToList()));
                }

                return Ok(sectionRegistrationStatuses2);                
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

        /// <remarks>FOR USE WITH EEDM</remarks>
        /// <summary>
        /// Retrieves a SectionRegistrationStatus by ID.
        /// </summary>
        /// <param name="id">ID to desired SectionRegistrationStatus</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.SectionRegistrationStatusItem3">SectionRegistrationStatus.</see></returns>
        /// 
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet]
        [HeaderVersionRoute("/section-registration-statuses/{id}", 8, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetSectionRegistrationStatusById", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.SectionRegistrationStatusItem3>> GetSectionRegistrationStatusById3Async(string id)
        {
            try
            {
                AddEthosContextProperties(
                    await _curriculumService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _curriculumService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));
                return await _curriculumService.GetSectionRegistrationStatusById3Async(id);
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
        /// Updates a SectionRegistrationStatus.
        /// </summary>
        /// <param name="sectionRegistrationStatus"><see cref="SectionRegistrationStatusItem2">SectionRegistrationStatus</see> to update</param>
        /// <returns>Newly updated <see cref="SectionRegistrationStatusItem2">SectionRegistrationStatus</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/section-registration-statuses/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutSectionRegistrationStatusesV6")]
        [HeaderVersionRoute("/section-registration-statuses/{id}", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutSectionRegistrationStatusesV8")]
        public async Task<ActionResult<Dtos.SectionRegistrationStatusItem2>> PutSectionRegistrationStatusesAsync([FromBody] Dtos.SectionRegistrationStatusItem2 sectionRegistrationStatus)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a SectionRegistrationStatus.
        /// </summary>
        /// <param name="sectionRegistrationStatus"><see cref="SectionRegistrationStatusItem2">SectionRegistrationStatus</see> to create</param>
        /// <returns>Newly created <see cref="SectionRegistrationStatusItem2">SectionRegistrationStatus</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/section-registration-statuses", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostSectionRegistrationStatusesV6")]
        [HeaderVersionRoute("/section-registration-statuses", 8, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostSectionRegistrationStatusesV8")]
        public async Task<ActionResult<Dtos.SectionRegistrationStatusItem2>> PostSectionRegistrationStatusesAsync([FromBody] Dtos.SectionRegistrationStatusItem2 sectionRegistrationStatus)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing SectionRegistrationStatus.
        /// </summary>
        /// <param name="id">Id of the SectioinRegistrationStatus to delete</param>
        [HttpDelete]
        [Route("/section-registration-statuses/{id}", Name = "DeleteSectionRegistrationStatuses", Order = -10)]
        public async Task<IActionResult> DeleteSectionRegistrationStatusesAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
