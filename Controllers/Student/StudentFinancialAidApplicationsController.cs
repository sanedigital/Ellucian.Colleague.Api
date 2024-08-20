// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Web.Adapters;
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
    /// Provides access to FinancialAidApplications
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentFinancialAidApplicationsController : BaseCompressedApiController
    {
        private readonly IStudentFinancialAidApplicationService studentFinancialAidApplicationService;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the FinancialAidApplicationsController class.
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>   
        /// <param name="studentFinancialAidApplicationService">StudentFinancialAidApplicationService</param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentFinancialAidApplicationsController(IAdapterRegistry adapterRegistry,
            IStudentFinancialAidApplicationService studentFinancialAidApplicationService,
            ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.adapterRegistry = adapterRegistry;
            this.studentFinancialAidApplicationService = studentFinancialAidApplicationService;
            this.logger = logger;
        }

        /// <summary>
        /// Return all financialAidApplications
        /// </summary>
        /// <returns>List of FinancialAidApplications <see cref="Dtos.FinancialAidApplication"/> objects representing matching financialAidApplications</returns>
        [CustomMediaTypeAttributeFilter( ErrorContentType = IntegrationErrors2 )]
        [HttpGet]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.ViewFinancialAidApplications)]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.FinancialAidApplication)), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/financial-aid-applications", "9.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetFinancialAidApplications", IsEedmSupported = true)]
        [HeaderVersionRoute("/financial-aid-applications", 9, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetFinancialAidApplicationsV9", IsEedmSupported = true)]
        public async Task<IActionResult> GetAsync(Paging page, QueryStringFilter criteria)
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
                studentFinancialAidApplicationService.ValidatePermissions(GetPermissionsMetaData());
                var criteriaObject = GetFilterObject<Dtos.FinancialAidApplication>(logger, "criteria");
                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.FinancialAidApplication>>(new List<Dtos.FinancialAidApplication>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                var pageOfItems = await studentFinancialAidApplicationService.GetAsync(page.Offset, page.Limit, criteriaObject, bypassCache);
                AddEthosContextProperties(
                    await studentFinancialAidApplicationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await studentFinancialAidApplicationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.FinancialAidApplication>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
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
        /// Read (GET) a financialAidApplications using a GUID
        /// </summary>
        /// <param name="id">GUID to desired financialAidApplications</param>
        /// <returns>A financialAidApplications object <see cref="Dtos.FinancialAidApplication"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter( ErrorContentType = IntegrationErrors2 )]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(StudentPermissionCodes.ViewFinancialAidApplications)]
        [HttpGet]
        [HeaderVersionRoute("/financial-aid-applications/{id}", "9.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetFinancialAidApplicationsById", IsEedmSupported = true)]
        [HeaderVersionRoute("/financial-aid-applications/{id}", 9, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetFinancialAidApplicationsByIdV9", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.FinancialAidApplication>> GetByIdAsync(string id)
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
                studentFinancialAidApplicationService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                    await studentFinancialAidApplicationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await studentFinancialAidApplicationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));

                return await studentFinancialAidApplicationService.GetByIdAsync(id);
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
        /// Create (POST) a new financialAidApplications
        /// </summary>
        /// <param name="financialAidApplications">DTO of the new financialAidApplications</param>
        /// <returns>A financialAidApplications object <see cref="Dtos.FinancialAidApplication"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/financial-aid-applications", "9.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostFinancialAidApplicationsV910")]
        [HeaderVersionRoute("/financial-aid-applications", 9, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostFinancialAidApplicationsV9")]
        public async Task<ActionResult<Dtos.FinancialAidApplication>> CreateAsync([FromBody] Dtos.FinancialAidApplication financialAidApplications)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing financialAidApplications
        /// </summary>
        /// <param name="id">GUID of the financialAidApplications to update</param>
        /// <param name="financialAidApplications">DTO of the updated financialAidApplications</param>
        /// <returns>A financialAidApplications object <see cref="Dtos.FinancialAidApplication"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/financial-aid-applications/{id}", "9.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutFinancialAidApplicationsV910")]
        [HeaderVersionRoute("/financial-aid-applications/{id}", 9, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutFinancialAidApplicationsV9")]
        public async Task<ActionResult<Dtos.FinancialAidApplication>> UpdateAsync([FromRoute] string id, [FromBody] Dtos.FinancialAidApplication financialAidApplications)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a financialAidApplications
        /// </summary>
        /// <param name="id">GUID to desired financialAidApplications</param>
        [HttpDelete]
        [Route("/financial-aid-applications/{id}", Name = "DefaultDeleteFinancialAidApplications", Order = -10)]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}
