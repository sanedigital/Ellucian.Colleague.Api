// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Ellucian.Data.Colleague.Exceptions;
using System.Net;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to Petition Statuses
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class PetitionStatusesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// PetitionStatusesController constructor
        /// </summary>
        /// <param name="adapterRegistry">adapterRegistry</param>
        /// <param name="studentReferenceDataRepository">studentReferenceDataRepository</param>
        /// <param name="logger">logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PetitionStatusesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository studentReferenceDataRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            this._logger = logger;
        }

        /// <summary>
        /// Get a list of all Petition Statuses
        /// </summary>
        /// <returns>A list of <see cref="PetitionStatus">PetitionStatus</see> codes and descriptions</returns>
        /// <note>PetitionStatus is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/petition-statuses", 1, true, Name = "GetPetitionStatuses")]
        public async Task<ActionResult<IEnumerable<PetitionStatus>>> GetAsync()
        {
            try
            {
                var petitionStatusDtos = new List<PetitionStatus>();

                var petitionStatuses =await _studentReferenceDataRepository.GetPetitionStatusesAsync();

                //Get the adapter and convert to dto
                var petitionStatusDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.PetitionStatus, PetitionStatus>();

                if (petitionStatuses != null && petitionStatuses.Count() > 0)
                {
                    foreach (var status in petitionStatuses)
                    {
                        petitionStatusDtos.Add(petitionStatusDtoAdapter.MapToType(status));
                    }
                }

                return petitionStatusDtos;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving petition statuses";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }

            catch (Exception e)
            {
                string message = "Unable to retrieve PetitionStatuses.";
                _logger.LogError(e, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }
    }
}

