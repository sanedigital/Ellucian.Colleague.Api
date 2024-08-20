// Copyright 2013-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to health condition data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class HealthConditionsController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        /// <summary>
        /// HealthConditionsController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Reference data repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public HealthConditionsController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
        }
        /// <summary>
        /// Get the health conditions
        /// </summary>
        /// <returns>All <see cref="HealthConditions"/></returns>
        /// <note>HealthConditions is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/health-conditions", 1, true, Name = "GetHealthConditions")]
        public IEnumerable<HealthConditions> GetHealthConditions()
        {
            var healthConditions = _referenceDataRepository.HealthConditions;
            var healthConditionsDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.HealthConditions, HealthConditions>();
            var healthConditionsDtoCollection = new List<Ellucian.Colleague.Dtos.Base.HealthConditions>();
            foreach (var healthCondition in healthConditions)
            {
                healthConditionsDtoCollection.Add(healthConditionsDtoAdapter.MapToType(healthCondition));
            }
            return healthConditionsDtoCollection.OrderBy(s => s.Code);
        }
    }
}
