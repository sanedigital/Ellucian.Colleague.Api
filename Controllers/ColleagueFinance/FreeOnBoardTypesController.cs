// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Exceptions;
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


namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to FreeOnBoardTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class FreeOnBoardTypesController : BaseCompressedApiController
    {
        private readonly IFreeOnBoardTypesService _freeOnBoardTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the FreeOnBoardTypesController class.
        /// </summary>
        /// <param name="freeOnBoardTypesService">Service of type <see cref="IFreeOnBoardTypesService">IFreeOnBoardTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public FreeOnBoardTypesController(IFreeOnBoardTypesService freeOnBoardTypesService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _freeOnBoardTypesService = freeOnBoardTypesService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all freeOnBoardTypes
        /// </summary>
        /// <returns>List of FreeOnBoardTypes <see cref="Dtos.FreeOnBoardTypes"/> objects representing matching freeOnBoardTypes</returns>
        [HttpGet]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true }), ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/free-on-board-types", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetFreeOnBoardTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.FreeOnBoardTypes>>> GetFreeOnBoardTypesAsync()
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
                var items = await _freeOnBoardTypesService.GetFreeOnBoardTypesAsync(bypassCache);

                if (items != null && items.Any())
                {
                    AddEthosContextProperties(await _freeOnBoardTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                      await _freeOnBoardTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      items.Select(a => a.Id).ToList()));
                }

                return Ok(items);
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
        /// Read (GET) a freeOnBoardTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired freeOnBoardTypes</param>
        /// <returns>A freeOnBoardTypes object <see cref="Dtos.FreeOnBoardTypes"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/free-on-board-types/{guid}", "10", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetFreeOnBoardTypesByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.FreeOnBoardTypes>> GetFreeOnBoardTypesByGuidAsync(string guid)
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
                return await _freeOnBoardTypesService.GetFreeOnBoardTypesByGuidAsync(guid);
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
        /// Create (POST) a new freeOnBoardTypes
        /// </summary>
        /// <param name="freeOnBoardTypes">DTO of the new freeOnBoardTypes</param>
        /// <returns>A freeOnBoardTypes object <see cref="Dtos.FreeOnBoardTypes"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/free-on-board-types", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostFreeOnBoardTypesV10")]
        public async Task<ActionResult<Dtos.FreeOnBoardTypes>> PostFreeOnBoardTypesAsync([FromBody] Dtos.FreeOnBoardTypes freeOnBoardTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing freeOnBoardTypes
        /// </summary>
        /// <param name="guid">GUID of the freeOnBoardTypes to update</param>
        /// <param name="freeOnBoardTypes">DTO of the updated freeOnBoardTypes</param>
        /// <returns>A freeOnBoardTypes object <see cref="Dtos.FreeOnBoardTypes"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/free-on-board-types/{guid}", "10", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutFreeOnBoardTypesV10")]
        public async Task<ActionResult<Dtos.FreeOnBoardTypes>> PutFreeOnBoardTypesAsync([FromRoute] string guid, [FromBody] Dtos.FreeOnBoardTypes freeOnBoardTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a freeOnBoardTypes
        /// </summary>
        /// <param name="guid">GUID to desired freeOnBoardTypes</param>
        [HttpDelete]
        [Route("/free-on-board-types/{guid}", Name = "DefaultDeleteFreeOnBoardTypes", Order = -10)]
        public async Task<IActionResult> DeleteFreeOnBoardTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}
