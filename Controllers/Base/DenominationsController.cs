// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.ComponentModel;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Denomination data.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    [Authorize]
    public class DenominationsController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// DenominationsController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public DenominationsController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves all Denominations.
        /// </summary>
        /// <returns>All <see cref="Denomination">Denomination codes and descriptions.</see></returns>
        /// <note>Denomination is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/denominations", 1, true, Name = "GetDenominations")]
        public IEnumerable<Denomination> GetDenominations()
        {
            var DenominationCollection = _referenceDataRepository.Denominations;

            // Get the right adapter for the type mapping
            var DenominationDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Denomination, Denomination>();

            // Map the Denomination entity to the program DTO
            var DenominationDtoCollection = new List<Denomination>();
            foreach (var Denomination in DenominationCollection)
            {
                DenominationDtoCollection.Add(DenominationDtoAdapter.MapToType(Denomination));
            }

            return DenominationDtoCollection;
        }
    }
}
