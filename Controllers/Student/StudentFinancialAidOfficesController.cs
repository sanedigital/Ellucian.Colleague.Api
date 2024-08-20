// Copyright 2018-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
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
    /// Exposes FinancialAidOffice and FinancialAidConfiguration Data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentFinancialAidOfficesController : BaseCompressedApiController
    {
        private readonly IStudentFinancialAidOfficeService financialAidOfficeService;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Constructor for FinancialAidOfficesController
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="financialAidOfficeService">FinancialAidOfficeService</param>
        /// <param name="logger">Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentFinancialAidOfficesController(IAdapterRegistry adapterRegistry, IStudentFinancialAidOfficeService financialAidOfficeService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.financialAidOfficeService = financialAidOfficeService;
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
        }        

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all financial aid offices.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All FinancialAidOffice objects.</returns>
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true }), ServiceFilter(typeof(EedmResponseFilter)), HttpGet]
        [HttpGet]
        [HeaderVersionRoute("/financial-aid-offices", 9, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetFinancialAidOffices", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Dtos.FinancialAidOffice>>> GetEedmFinancialAidOfficesAsync()
        {
            bool bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                var items = await financialAidOfficeService.GetFinancialAidOfficesAsync(bypassCache);

                AddEthosContextProperties(await financialAidOfficeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await financialAidOfficeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              items.Select(a => a.Id).ToList()));

                return Ok(items);
            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException e)
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
        /// Retrieves an Financial Aid Offices by ID.
        /// </summary>
        /// <returns>An <see cref="Dtos.FinancialAidOffice">FinancialAidOffice</see>object.</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/financial-aid-offices/{guid}", 9, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetFinancialAidOfficeByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.FinancialAidOffice>> GetFinancialAidOfficeByGuidAsync(string guid)
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
                var faOffice = await financialAidOfficeService.GetFinancialAidOfficeByGuidAsync(guid, bypassCache);

                if (faOffice != null)
                {

                    AddEthosContextProperties(await financialAidOfficeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await financialAidOfficeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { faOffice.Id }));
                }

                return faOffice;
            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException e)
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
        /// Creates a Financial Aid Office.
        /// </summary>
        /// <param name="financialAidOffice"><see cref="Dtos.FinancialAidOffice">FinancialAidOffice</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.FinancialAidOffice">FinancialAidOffice</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/financial-aid-offices", 9, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostFinancialAidOfficesV9")]
        public async Task<ActionResult<Dtos.FinancialAidOffice>> PostFinancialAidOfficeAsync([FromBody] Dtos.FinancialAidOffice financialAidOffice)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Updates a Financial Aid Office.
        /// </summary>
        /// <param name="guid">Id of the Financial Aid Office to update</param>
        /// <param name="financialAidOffice"><see cref="Dtos.FinancialAidOffice">FinancialAidOffice</see> to create</param>
        /// <returns>Updated <see cref="Dtos.FinancialAidOffice">FinancialAidOffice</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/financial-aid-offices/{guid}", 9, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutFinancialAidOfficesV9")]
        public async Task<ActionResult<Dtos.FinancialAidOffice>> PutFinancialAidOfficeAsync([FromRoute] string guid, [FromBody] Dtos.FinancialAidOffice financialAidOffice)
        {

            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Deletes a Financial Aid Office.
        /// </summary>
        /// <param name="guid">ID of the Financial Aid Office to be deleted</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("/financial-aid-offices/{guid}", Name = "DeleteFinancialAidOffices", Order = -10)]
        public async Task<IActionResult> DeleteFinancialAidOfficeAsync(string guid)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}
