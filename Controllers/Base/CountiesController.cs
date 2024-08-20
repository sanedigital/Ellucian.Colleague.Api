// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Controller for county operations
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class CountiesController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initializes a new instance of the <see cref="CountiesController"/> class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public CountiesController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this._adapterRegistry = adapterRegistry;
            this._referenceDataRepository = referenceDataRepository;
            this._logger = logger;
        }

        /// <summary>
        /// Gets information for all counties
        /// </summary>
        /// <returns>Collection of of counties</returns>
        /// <accessComments>
        /// Any authenticated user can retrieve county information
        /// </accessComments>
        /// <note>County information is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/counties", 1, true, Name = "GetCountiesAsync")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Base.County>>> GetAsync()
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
                var countyDtoCollection = new List<Ellucian.Colleague.Dtos.Base.County>();
                var countyCollection = await _referenceDataRepository.GetCountiesAsync(bypassCache);
                var countyDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.County, Ellucian.Colleague.Dtos.Base.County>();
                if (countyCollection != null && countyCollection.Count() > 0)
                {
                    foreach (var country in countyCollection)
                    {
                        countyDtoCollection.Add(countyDtoAdapter.MapToType(country));
                    }
                }
                return countyDtoCollection;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                string message = "Session has expired while retrieving county data";
                _logger.LogError(csse, message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unable to retrieve counties.");
            }
        }

    }
}
