// Copyright 2023 Ellucian Company L.P. and its affiliates.
using System.Net;
using System.ComponentModel;
using System.Collections.Generic;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Gender data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class GenderController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the GenderController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor">Interface to action context accessor</param>
        /// <param name="apiSettings"></param>
        public GenderController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, ILogger logger,
            IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a collection of all the gender codes.
        /// </summary>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <returns>All <see cref="Gender">Gender codes and descriptions.</see></returns>
        /// <note>Genders is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/gender", 1, true, Name = "GetGenders")]
        public async Task<ActionResult<IEnumerable<Gender>>> GetGenders()
        {
            try
            {
                var genderDtos = new List<Gender>();
                var genderEntities = await _referenceDataRepository.GetGenders();

                if (genderEntities != null)
                {
                    // Get the right adapter for the type mapping
                    var genderDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.Gender, Gender>();
                    // Map the gender entity to the gender DTO
                    foreach (var gender in genderEntities)
                    {
                        genderDtos.Add(genderDtoAdapter.MapToType(gender));
                    }
                }

                return Ok(genderDtos);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving gender codes";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unable to retrieve gender codes", HttpStatusCode.BadRequest);
            }
        }
    }
}