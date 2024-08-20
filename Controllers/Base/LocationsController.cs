// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Location data.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class LocationsController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
        private const string unexpectedGenericErrorMessage = "Unexpected error occurred while processing the request.";

        /// <summary>
        /// LocationsController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public LocationsController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves all Locations.
        /// </summary>
        /// <returns>All <see cref="Location">Locations</see></returns>
        /// <note>This request supports anonymous access. The LOCATIONS entity in Colleague must have public access enabled for this endpoint to function anonymously. See :ref:`anonymousapis` for additional information.</note>
        [HttpGet]
        [HeaderVersionRoute("/locations", 1, true, Name = "GetLocations")]
        public ActionResult<IEnumerable<Location>> GetLocations()
        {
            try
            {
                var LocationCollection = _referenceDataRepository.Locations;

                // Get the right adapter for the type mapping
                var LocationDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Location, Location>();

                // Map the Location entity to the program DTO
                var LocationDtoCollection = new List<Location>();
                foreach (var Location in LocationCollection)
                {
                    LocationDtoCollection.Add(LocationDtoAdapter.MapToType(Location));
                }

                return Ok(LocationDtoCollection);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving locations";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }

            catch (Exception e)
            {
                _logger.LogError(e, "Unknown error occurred");
                return CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }
    }
}
