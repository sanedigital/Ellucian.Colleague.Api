// Copyright 2014-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Exposes BudgetComponents data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class FinancialAidBudgetComponentsController : BaseCompressedApiController
    {
        private readonly IFinancialAidReferenceDataRepository _FinancialAidReferenceDataRepository;
        private readonly IAdapterRegistry _AdapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// BudgetComponents constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="financialAidReferenceDataRepository">Financial Aid Reference Data Repository of type <see cref="IFinancialAidReferenceDataRepository">IFinancialAidReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public FinancialAidBudgetComponentsController(IAdapterRegistry adapterRegistry, IFinancialAidReferenceDataRepository financialAidReferenceDataRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _AdapterRegistry = adapterRegistry;
            _FinancialAidReferenceDataRepository = financialAidReferenceDataRepository;
            this._logger = logger;
        }

        /// <summary>
        /// Get all BudgetComponent objects for all Financial Aid award years
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>A List of all Budget Components</returns>
        /// <note>BudgetComponent is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/financial-aid-budget-components", 1, true, Name = "GetFinancialAidBudgetComponents")]
        public ActionResult<IEnumerable<BudgetComponent>> GetBudgetComponents()
        {
            try
            {
                var budgetComponents = _FinancialAidReferenceDataRepository.BudgetComponents;

                var budgetComponentDtoAdapter = _AdapterRegistry.GetAdapter<Colleague.Domain.FinancialAid.Entities.BudgetComponent, Colleague.Dtos.FinancialAid.BudgetComponent>();

                return Ok( budgetComponents.Select(budget =>
                    budgetComponentDtoAdapter.MapToType(budget)));
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting BudgetComponents resource. See log for details");
            }
        }
    }
}
