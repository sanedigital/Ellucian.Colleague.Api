// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.ComponentModel;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Dtos.Student.Requirements;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Ellucian.Data.Colleague.Exceptions;
using System.Net;
using System;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Requirements data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class RequirementsController : BaseCompressedApiController
    {
        private readonly IRequirementRepository _RequirementRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the RequirementsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="requirementRepository">Repository of type <see cref="IRequirementRepository">IRequirementRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public RequirementsController(IAdapterRegistry adapterRegistry, IRequirementRepository requirementRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _RequirementRepository = requirementRepository;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves the requirement details for a specific requirement code.
        /// </summary>
        /// <param name="id">Requirement Code</param>
        /// <returns>The requested <see cref="Requirement">Requirement</see></returns>
        /// <note>This parameter has support for :ref:`urlcharactersubstitution`</note>
        [ParameterSubstitutionFilter]
        [HttpGet]
        [HeaderVersionRoute("/requirements/{id}", 1, true, Name = "GetRequirement")]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.Student.Requirements.Requirement>> GetAsync(string id)
        {
            try
            {
                var requirementEntity = await _RequirementRepository.GetAsync(id);

                var requirementDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Requirements.Requirement, Requirement>();

                Requirement requirementDto = requirementDtoAdapter.MapToType(requirementEntity);
                return requirementDto;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving requirement details";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                string message = "Exception occurred while retrieving requirement details";
                _logger.LogError(ex, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the requirement details for the provided requirement codes.
        /// </summary>
        /// <param name="criteria">Criteria, including Requirement Ids, to use to request Requirements</param>
        /// <returns>List of the requested <see cref="Requirement">Requirement</see> objects</returns>
        /// <note>Requirement is cached for 24 hours.</note>
        [HttpPost]
        [HeaderVersionRoute("/qapi/requirements", 1, true, Name = "QueryRequirementsByPost")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Student.Requirements.Requirement>>> QueryRequirementsByPostAsync([FromBody] RequirementQueryCriteria criteria )
        {
            try
            {
                var requirementDtos = new List<Dtos.Student.Requirements.Requirement>();

                var requirementEntities = await _RequirementRepository.GetAsync(criteria.RequirementIds);

                var requirementDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Requirements.Requirement, Requirement>();

                foreach (var requirementEntity in requirementEntities)
                {
                    requirementDtos.Add(requirementDtoAdapter.MapToType(requirementEntity));
                }
                return requirementDtos;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while querying requirements";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                string message = "Exception occurred while querying requirements";
                _logger.LogError(ex, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

        }

    }
}

