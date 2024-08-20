/* Copyright 2023 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;


namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Controller exposes actions to interact with EarningsTypeGroups
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class EarningsTypeGroupsController : BaseCompressedApiController
    {
        private ILogger logger;
        private IAdapterRegistry adapterRegistry;
        private IHumanResourcesReferenceDataRepository referenceDataRepository;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
        private const string unexpectedGenericErrorMessage = "Unexpected error occurred while processing the request.";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="adapterRegistry"></param>
        /// <param name="referenceDataRepository"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public EarningsTypeGroupsController(ILogger logger, IAdapterRegistry adapterRegistry, IHumanResourcesReferenceDataRepository referenceDataRepository, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.logger = logger;
            this.adapterRegistry = adapterRegistry;
            this.referenceDataRepository = referenceDataRepository;
        }

        /// <summary>
        /// Get all EarningsTypeGroups. This endpoint is used in SelfService
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can view EarningsTypeGroups
        /// </accessComments>
        /// <returns>A list of all EarningsTypeGroups</returns>
        /// <note>EarningsTypeGroups are cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/earnings-type-groups", 1, true, Name = "GetEarningsTypeGroups")]
        public async Task<ActionResult<IEnumerable<EarningsTypeGroup>>> GetEarningsTypeGroupsAsync()
        {
            logger.LogDebug("********* Start - Process to get Earnings Type Groups- Start*********");
            try
            {
                var earningsTypeGroupDictionary = await referenceDataRepository.GetEarningsTypesGroupsAsync();
                if (earningsTypeGroupDictionary == null || !earningsTypeGroupDictionary.Any())
                {
                    return new List<EarningsTypeGroup>();
                }

                var adapter = adapterRegistry.GetAdapter<Domain.HumanResources.Entities.EarningsTypeGroup, EarningsTypeGroup>();
                var dtos = earningsTypeGroupDictionary.Values.Select(etg => adapter.MapToType(etg));
                logger.LogDebug("********* End - Process to get Earnings Type Groups- End*********");
                return Ok(dtos);
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

    }
}
