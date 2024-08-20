// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Controller for Other Honors
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class OtherHonorsController : BaseCompressedApiController
    {
       private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly IOtherHonorService _otherHonorService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the Other HonorController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="otherHonorService">Service of type <see cref="IOtherHonorService">IOtherHonorService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public OtherHonorsController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, IOtherHonorService otherHonorService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _otherHonorService = otherHonorService;
            this._logger = logger;
        }


        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves all Other Honors
        /// </summary>
        /// <returns>All <see cref="OtherHonors">OtherHonors.</see></returns>        
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionRoute("/academic-honors", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetHeDMOtherHonorTypes", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.OtherHonor>>> GetOtherHonorAsync()
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
                var items = await _otherHonorService.GetOtherHonorsAsync(bypassCache);

                AddEthosContextProperties(
                  await _otherHonorService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _otherHonorService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      items.Select(i => i.Id).Distinct().ToList()));

                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves an Other Honor by ID.
        /// </summary>
        /// <returns>A <see cref="OtherHonors">OtherHonor.</see></returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/academic-honors/{id}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetOtherHonorTypeById", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.OtherHonor>> GetOtherHonorByIdAsync(string id)
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
                AddEthosContextProperties(
                  await _otherHonorService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _otherHonorService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { id }));

                return await _otherHonorService.GetOtherHonorByGuidAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Updates a OtherHonor.
        /// </summary>
        /// <param name="otherHonor"><see cref="OtherHonor">OtherHonor</see> to update</param>
        /// <returns>Newly updated <see cref="OtherHonor">OtherHonor</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/academic-honors/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutOtherHonorTypeV6")]
        public ActionResult<Dtos.OtherHonor> PutOtherHonors([FromBody] Dtos.OtherHonor otherHonor)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Creates a OtherHonor.
        /// </summary>
        /// <param name="otherHonor"><see cref="OtherHonor">OtherHonor</see> to create</param>
        /// <returns>Newly created <see cref="OtherHonor">OtherHonor</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/academic-honors", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostOtherHonorTypeV6")]
        public ActionResult<Dtos.OtherHonor> PostOtherHonors([FromBody] Dtos.OtherHonor otherHonor)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing OtherHonor
        /// </summary>
        /// <param name="id">Id of the OtherHonor to delete</param>
        [HttpDelete]
        [Route("/academic-honors/{id}", Name = "DeleteOtherHonorType", Order = -10)]
        public ActionResult<Dtos.OtherHonor> DeleteOtherHonors(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
