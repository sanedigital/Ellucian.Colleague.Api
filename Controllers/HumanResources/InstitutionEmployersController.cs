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
    /// Provides access to InstitutionEmployers
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class InstitutionEmployersController : BaseCompressedApiController
    {
        private readonly IInstitutionEmployersService _institutionEmployersService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the InstitutionEmployersController class.
        /// </summary>
        /// <param name="institutionEmployersService">Service of type <see cref="IInstitutionEmployersService">IInstitutionEmployersService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor">Interface to the action context accessor</param>
        /// <param name="apiSettings"></param>
        public InstitutionEmployersController(IInstitutionEmployersService institutionEmployersService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _institutionEmployersService = institutionEmployersService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all InstitutionEmployers
        /// </summary>
        /// <returns>Returns list of InstitutionEmployers <see cref="Dtos.InstitutionEmployers"/> objects representing matching institutionEmployers</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.InstitutionEmployers))]
        [HttpGet]
        [HeaderVersionRoute("/institution-employers", "11.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetInstitutionEmployers", IsEedmSupported = true)]
        [HeaderVersionRoute("/institution-employers", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetInstitutionEmployersV11", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.InstitutionEmployers>>> GetInstitutionEmployersAsync(QueryStringFilter criteria)
        {
            var bypassCache = false;
            string code = string.Empty;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                var criteriaObj = GetFilterObject<Dtos.EmploymentDepartments>(_logger, "criteria");
                if (CheckForEmptyFilterParameters())
                    return new List<Dtos.InstitutionEmployers>(new List<Dtos.InstitutionEmployers>());
                var items = await _institutionEmployersService.GetInstitutionEmployersAsync(bypassCache);
                if (criteriaObj != null && !string.IsNullOrEmpty(criteriaObj.Code) && items != null && items.Any())
                {
                    code = criteriaObj.Code;
                    items = items.Where(c => c.Code == code);
                }
                AddEthosContextProperties(await _institutionEmployersService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                  await _institutionEmployersService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                  items.Select(a => a.Id).ToList()));
                return Ok(items);
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
        /// Read (GET) an InstitutionEmployers using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired institutionEmployers</param>
        /// <returns>An InstitutionEmployers DTO object <see cref="Dtos.InstitutionEmployers"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/institution-employers/{guid}", "11.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetInstitutionEmployersByGuid", IsEedmSupported = true)]
        [HeaderVersionRoute("/institution-employers/{guid}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetInstitutionEmployersByGuidV11", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.InstitutionEmployers>> GetInstitutionEmployersByGuidAsync(string guid)
        {
            try
            {
                AddDataPrivacyContextProperty((await _institutionEmployersService.GetDataPrivacyListByApi(GetRouteResourceName())).ToList());
                return await _institutionEmployersService.GetInstitutionEmployersByGuidAsync(guid);
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
        /// Create (POST) a new institutionEmployers
        /// </summary>
        /// <param name="institutionEmployers">DTO of the new institutionEmployers</param>
        /// <returns>An InstitutionEmployers DTO object <see cref="Dtos.InstitutionEmployers"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/institution-employers", "11.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostInstitutionEmployersV1110")]
        [HeaderVersionRoute("/institution-employers", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostInstitutionEmployersV11")]
        public async Task<ActionResult<Dtos.InstitutionEmployers>> PostInstitutionEmployersAsync([FromBody] Dtos.InstitutionEmployers institutionEmployers)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update (PUT) an existing institutionEmployers
        /// </summary>
        /// <param name="guid">GUID of the institutionEmployers to update</param>
        /// <param name="institutionEmployers">DTO of the updated institutionEmployers</param>
        /// <returns>An InstitutionEmployers DTO object <see cref="Dtos.InstitutionEmployers"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/institution-employers/{guid}", "11.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutInstitutionEmployersV1110")]
        [HeaderVersionRoute("/institution-employers/{guid}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutInstitutionEmployersV11")]
        public async Task<ActionResult<Dtos.InstitutionEmployers>> PutInstitutionEmployersAsync([FromRoute] string guid, [FromBody] Dtos.InstitutionEmployers institutionEmployers)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        
        /// <summary>
        /// Delete (DELETE) a institutionEmployers
        /// </summary>
        /// <param name="guid">GUID to desired institutionEmployers</param>
        [HttpDelete]
        [Route("/institution-employers/{guid}", Name = "DefaultDeleteInstitutionEmployers")]
        public async Task<IActionResult> DeleteInstitutionEmployersAsync(string guid)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
 }
