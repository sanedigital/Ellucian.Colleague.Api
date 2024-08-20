// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;


namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Expose Human Resources Earnings Types data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class EarningTypesController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly IEarningsTypeRepository earningsTypeRepository;
        private readonly IEarningTypesService _earningTypesService;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
        private const string unexpectedGenericErrorMessage = "Unexpected error occurred while processing the request.";

        /// <summary>
        /// EarningsTypesController constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="adapterRegistry"></param>
        /// <param name="earningsTypeRepository"></param>
        /// <param name="earningTypesService">Service of type <see cref="IEarningTypesService">IEarningTypesService</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public EarningTypesController(ILogger logger, IAdapterRegistry adapterRegistry, IEarningsTypeRepository earningsTypeRepository, IEarningTypesService earningTypesService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.logger = logger;
            this.adapterRegistry = adapterRegistry;
            this.earningsTypeRepository = earningsTypeRepository;
            _earningTypesService = earningTypesService;
        }

        /// <summary>
        /// Gets a list of earnings types. An earnings type is an identifier for wages or leave associated with an employment position.   
        /// The returned list should contain all active and inactive earn types available for an institution
        /// </summary>
        /// <returns>A List of earnings type objects</returns>
        /// <note>EarningsTypes are cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/earnings-types", 1, false, Name = "GetEarningsTypes")]
        public async Task<ActionResult<IEnumerable<EarningsType>>> GetEarningsTypesAsync()
        {
            logger.LogDebug("********* Start - Process to get List of Earning Types - Start *********");
            try
            {
                var earningsTypeEntities = await earningsTypeRepository.GetEarningsTypesAsync();
                var entityToDtoAdapter = adapterRegistry.GetAdapter<Domain.HumanResources.Entities.EarningsType, EarningsType>();
                logger.LogDebug("********* End - Process to get List of Earning Types - End *********");
                return Ok(earningsTypeEntities.Select(earningsType => entityToDtoAdapter.MapToType(earningsType)));
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Return all earningTypes
        /// </summary>
        /// <returns>List of EarningTypes <see cref="Dtos.EarningTypes"/> objects representing matching earningTypes</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/earning-types", "12", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetEarningTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.EarningTypes>>> GetEarningTypesAsync()
        {
            logger.LogDebug("********* Start - Process to get List of Earning Types - Start*********");
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
                logger.LogDebug(string.Format("Calling GetEarningTypesAsync service method with BypassCache {0}", bypassCache));
                var items = await _earningTypesService.GetEarningTypesAsync(bypassCache);

                AddEthosContextProperties(await _earningTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _earningTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              items.Select(a => a.Id).ToList()));

                logger.LogDebug("********* End - Process to get List of Earning Types - End*********");
                return Ok(items);
            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
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
        /// Read (GET) a earningTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired earningTypes</param>
        /// <returns>A earningTypes object <see cref="Dtos.EarningTypes"/> in EEDM format</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/earning-types/{guid}", "12", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetEarningTypesByGuid")]
        public async Task<ActionResult<Dtos.EarningTypes>> GetEarningTypesByGuidAsync(string guid)
        {
            logger.LogDebug("********* Start - Process to Get earning types by guid - Start *********");
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
                logger.LogDebug("GUID cannot be null or empty");
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                logger.LogDebug(string.Format("Calling GetEarningTypesByGuidAsync service method with guid {0}", guid));
                var earningType = await _earningTypesService.GetEarningTypesByGuidAsync(guid);

                if (earningType != null)
                {

                    AddEthosContextProperties(await _earningTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _earningTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { earningType.Id }));
                }

                logger.LogDebug("********* End - Process to Get earning types by guid - End *********");
                return earningType;
            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
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
        /// Create (POST) a new earningTypes
        /// </summary>
        /// <param name="earningTypes">DTO of the new earningTypes</param>
        /// <returns>A earningTypes object <see cref="Dtos.EarningTypes"/> in EEDM format</returns>
        [HttpPost]
        [HeaderVersionRoute("/earning-types", "12", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostEarningTypesV12")]
        public async Task<ActionResult<Dtos.EarningTypes>> PostEarningTypesAsync([FromBody] Dtos.EarningTypes earningTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing earningTypes
        /// </summary>
        /// <param name="guid">GUID of the earningTypes to update</param>
        /// <param name="earningTypes">DTO of the updated earningTypes</param>
        /// <returns>A earningTypes object <see cref="Dtos.EarningTypes"/> in EEDM format</returns>
        [HttpPut]
        [HeaderVersionRoute("/earning-types/{guid}", "12", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutEarningTypesV12")]
        public async Task<ActionResult<Dtos.EarningTypes>> PutEarningTypesAsync([FromRoute] string guid, [FromBody] Dtos.EarningTypes earningTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a earningTypes
        /// </summary>
        /// <param name="guid">GUID to desired earningTypes</param>
        [HttpDelete]
        [Route("/earning-types/{guid}", Name = "DefaultDeleteEarningTypes")]
        public async Task<IActionResult> DeleteEarningTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}
