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
    /// Provides access to StudentFinancialAidNeedSummaries
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentFinancialAidNeedSummariesController : BaseCompressedApiController
    {
        private readonly IStudentFinancialAidNeedSummaryService StudentFinancialAidNeedSummaryService;
        //private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the StudentFinancialAidNeedSummariesController class.
        /// </summary>
        /// <param name="StudentFinancialAidNeedSummaryService">StudentFinancialAidNeedSummaryService</param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentFinancialAidNeedSummariesController(IStudentFinancialAidNeedSummaryService StudentFinancialAidNeedSummaryService,
            ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.StudentFinancialAidNeedSummaryService = StudentFinancialAidNeedSummaryService;
            this.logger = logger;
        }

        /// <summary>
        /// Return all StudentFinancialAidNeedSummaries
        /// </summary>
        /// <returns>List of StudentFinancialAidNeedSummaries</returns>
        [HttpGet, PermissionsFilter(StudentPermissionCodes.ViewStudentFinancialAidNeedSummaries)]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/student-financial-aid-need-summaries", 9, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentFinancialAidNeedSummaries", IsEedmSupported = true)]
        public async Task<IActionResult> GetStudentFinancialAidNeedSummariesAsync(Paging page)
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
            try
            {
                StudentFinancialAidNeedSummaryService.ValidatePermissions(GetPermissionsMetaData());
                var pageOfItems = await StudentFinancialAidNeedSummaryService.GetAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                  await StudentFinancialAidNeedSummaryService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await StudentFinancialAidNeedSummaryService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.StudentFinancialAidNeedSummary>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Read (GET) a StudentFinancialAidNeedSummaries using a GUID
        /// </summary>
        /// <param name="id">GUID to desired StudentFinancialAidNeedSummaries</param>
        /// <returns>A single StudentFinancialAidNeedSummaries object</returns>
        [HttpGet,  CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.ViewStudentFinancialAidNeedSummaries)]
        [HeaderVersionRoute("/student-financial-aid-need-summaries/{id}", 9, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetStudentFinancialAidNeedSummariesById", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentFinancialAidNeedSummary>> GetStudentFinancialAidNeedSummariesByGuidAsync(string id)
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
                StudentFinancialAidNeedSummaryService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                 await StudentFinancialAidNeedSummaryService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                 await StudentFinancialAidNeedSummaryService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                     new List<string>() { id }));
                return await StudentFinancialAidNeedSummaryService.GetByIdAsync(id);
            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create (POST) a new StudentFinancialAidNeedSummaries
        /// </summary>
        /// <param name="StudentFinancialAidNeedSummaries">DTO of the new StudentFinancialAidNeedSummaries</param>
        /// <returns>A single StudentFinancialAidNeedSummaries object</returns>
        [HttpPost]
        [HeaderVersionRoute("/student-financial-aid-need-summaries", 9, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentFinancialAidNeedSummariesV9")]
        public async Task<ActionResult<Dtos.StudentFinancialAidNeedSummary>> CreateAsync([FromBody] Dtos.StudentFinancialAidNeedSummary StudentFinancialAidNeedSummaries)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing StudentFinancialAidNeedSummaries
        /// </summary>
        /// <param name="id">GUID of the StudentFinancialAidNeedSummaries to update</param>
        /// <param name="StudentFinancialAidNeedSummaries">DTO of the updated StudentFinancialAidNeedSummarys</param>
        /// <returns>A single StudentFinancialAidNeedSummaries object</returns>
        [HttpPut]
        [HeaderVersionRoute("/student-financial-aid-need-summaries/{id}", 9, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentFinancialAidNeedSummariesV9")]
        public async Task<ActionResult<Dtos.StudentFinancialAidNeedSummary>> UpdateAsync([FromRoute] string id, [FromBody] Dtos.StudentFinancialAidNeedSummary StudentFinancialAidNeedSummaries)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a StudentFinancialAidNeedSummaries
        /// </summary>
        /// <param name="id">GUID to desired StudentFinancialAidNeedSummaries</param>
        [HttpDelete]
        [Route("/student-financial-aid-need-summaries/{id}", Name = "DefaultDeleteStudentFinancialAidNeedSummaries", Order = -10)]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}
