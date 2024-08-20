// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
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
    /// Provides access to FinancialAidYears data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class FinancialAidYearsController : BaseCompressedApiController
    {
        private readonly IFinancialAidYearService _financialAidYearService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the FinancialAidYearsController class.
        /// </summary>
        /// <param name="financialAidYearService">Repository of type <see cref="IFinancialAidYearService">IFinancialAidYearService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public FinancialAidYearsController(IFinancialAidYearService financialAidYearService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _financialAidYearService = financialAidYearService;
            this._logger = logger;
        }        

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all financial aid years.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All FinancialAidYear objects.</returns>
        [CustomMediaTypeAttributeFilter( ErrorContentType = IntegrationErrors2 )]
        [HttpGet, ValidateQueryStringFilter(NamedQueries = new string[] { "academicPeriod" }), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [QueryStringFilterFilter("academicPeriod", typeof(Dtos.Filters.AcademicPeriodNamedQueryFilter))]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/financial-aid-years", "7.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetFinancialAidYears", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidYear>>> GetFinancialAidYearsAsync(QueryStringFilter academicPeriod)
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
                string academicPeriodId = string.Empty;
                var academicPeriodFilter = GetFilterObject<Dtos.Filters.AcademicPeriodNamedQueryFilter>( _logger, "academicPeriod" );

                if( CheckForEmptyFilterParameters() )
                    return new List<Dtos.FinancialAidYear>();

                if( academicPeriodFilter != null )
                {
                    if( academicPeriodFilter.AcademicPeriod != null && !string.IsNullOrEmpty( academicPeriodFilter.AcademicPeriod.Id ) )
                        academicPeriodId = academicPeriodFilter.AcademicPeriod.Id;
                }

                var items = await _financialAidYearService.GetFinancialAidYearsAsync( academicPeriodId, bypassCache );

                AddEthosContextProperties(
                    await _financialAidYearService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _financialAidYearService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        items.Select(i => i.Id).ToList()));

                return Ok(items);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden );
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException e)
            {
                _logger.LogError( e.ToString() );
                return CreateHttpResponseException( IntegrationApiUtility.ConvertToIntegrationApiException( e ) );
            }
            catch( ArgumentException e )
            {
                _logger.LogError( e.ToString() );
                return CreateHttpResponseException( IntegrationApiUtility.ConvertToIntegrationApiException( e ) );
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
        /// Retrieves an Financial Aid Years by ID.
        /// </summary>
        /// <returns>An <see cref="Dtos.FinancialAidYear">FinancialAidYear</see>object.</returns>
        [CustomMediaTypeAttributeFilter( ErrorContentType = IntegrationErrors2 )]
        [HttpGet]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/financial-aid-years/{id}", "7.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetFinancialAidYearById", IsEedmSupported = true)]
        public async Task<ActionResult<FinancialAidYear>> GetFinancialAidYearByIdAsync(string id)
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
                AddEthosContextProperties(
                    await _financialAidYearService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _financialAidYearService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { id }));

                return await _financialAidYearService.GetFinancialAidYearByGuidAsync(id);
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
        /// Creates a Financial Aid Year.
        /// </summary>
        /// <param name="financialAidYear"><see cref="Dtos.FinancialAidYear">FinancialAidYear</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.FinancialAidYear">FinancialAidYear</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/financial-aid-years", "7.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostFinancialAidYearsV710")]
        public async Task<ActionResult<Dtos.FinancialAidYear>> PostFinancialAidYearAsync([FromBody] Dtos.FinancialAidYear financialAidYear)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Updates a Financial Aid Year.
        /// </summary>
        /// <param name="id">Id of the Financial Aid Year to update</param>
        /// <param name="financialAidYear"><see cref="Dtos.FinancialAidYear">FinancialAidYear</see> to create</param>
        /// <returns>Updated <see cref="Dtos.FinancialAidYear">FinancialAidYear</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/financial-aid-years/{id}", "7.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutFinancialAidYearsV710")]
        public async Task<ActionResult<Dtos.FinancialAidYear>> PutFinancialAidYearAsync([FromRoute] string id, [FromBody] Dtos.FinancialAidYear financialAidYear)
        {

            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Deletes a Financial Aid Year.
        /// </summary>
        /// <param name="id">ID of the Financial Aid Year to be deleted</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("/financial-aid-years/{id}", Name = "DeleteFinancialAidYears", Order = -10)]
        public async Task<IActionResult> DeleteFinancialAidYearAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}
