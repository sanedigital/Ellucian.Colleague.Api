// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student.Requirements;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Constraints;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;

namespace Ellucian.Colleague.Api.Controllers
{
	/// <summary>
	/// Provides access to Academic Program data.
	/// </summary>
	[Authorize]
	[LicenseProvider(typeof(EllucianLicenseProvider))]
	[EllucianLicenseModule(ModuleConstants.Student)]
	[Route("/[controller]/[action]")]
	public class ProgramsController : BaseCompressedApiController
	{
        private readonly ILogger _logger;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly IProgramRepository _ProgramRepository;
		private readonly IProgramRequirementsRepository _ProgramRequirementsRepository;
		private readonly IProgramsService _programsService;

        /// <summary>
        /// Initializes a new instance of the ProgramsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="programRepository">Repository of type <see cref="IProgramRepository">IProgramRepository</see></param>
        /// <param name="programRequirementsRepository">Repository of type <see cref="IProgramRequirementsRepository">IProgramRequirementsRepository</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        /// <param name="programsService"></param>
        public ProgramsController(IAdapterRegistry adapterRegistry, IProgramRepository programRepository, IProgramRequirementsRepository programRequirementsRepository, 
			ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings, IProgramsService programsService) 
			: base(actionContextAccessor, apiSettings)
		{
			_adapterRegistry = adapterRegistry;
			_ProgramRepository = programRepository;
			_ProgramRequirementsRepository = programRequirementsRepository;
			_programsService = programsService;
			_logger = logger;
		}

		/// <summary>
		/// Retrieves all Programs
		/// </summary>
		/// <returns>All <see cref="Dtos.Student.Requirements.Program">Programs</see></returns>
		/// <note>Programs are cached for 24 hours.</note>
		[HttpGet]
		[HeaderVersionRoute("/programs", 1, true, Name = "GetPrograms")]
		public async Task<ActionResult<IEnumerable<Dtos.Student.Requirements.Program>>> GetAsync()
		{
			try
			{
				var ProgramCollection = await _ProgramRepository.GetAsync();

				// Get the right adapter for the type mapping
				var programDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Requirements.Program, Dtos.Student.Requirements.Program>();

				// Map the program entity to the program DTO
				var programDtoCollection = new List<Dtos.Student.Requirements.Program>();
				foreach (var program in ProgramCollection)
				{
					programDtoCollection.Add(programDtoAdapter.MapToType(program));
				}
				return programDtoCollection;
			}
			catch (ColleagueSessionExpiredException tex)
			{
				string message = "Session has expired while retrieving programs";
				_logger.LogError(tex, message);
				return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
			}
			catch (Exception ex)
			{
				string message = "An exception occurred while retrieving programs";
				_logger.LogError(ex, message);
				return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
			}
		}

		/// <summary>
		/// Retrieves all active Programs.
		/// </summary>
		/// <returns>All active <see cref="Dtos.Student.Requirements.Program">Programs</see></returns>
		/// <note>Program is cached for 24 hours.</note>
		[Obsolete("Obsolete as of API version 1.2, use version 2 of this API")]
		[HttpGet]
		[HeaderVersionRoute("/programs/active", 1, false, Name = "GetActivePrograms")]
		public async Task<ActionResult<IEnumerable<Dtos.Student.Requirements.Program>>> GetActiveProgramsAsync()
		{
			var ProgramCollection = (await _ProgramRepository.GetAsync()).Where(p => p.IsActive == true);

			// Get the right adapter for the type mapping
			var programDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Requirements.Program, Dtos.Student.Requirements.Program>();

			// Map the program entity to the program DTO
			var programDtoCollection = new List<Dtos.Student.Requirements.Program>();
			foreach (var program in ProgramCollection)
			{
				programDtoCollection.Add(programDtoAdapter.MapToType(program));
			}
			return programDtoCollection;
		}

		/// <summary>
		/// Retrieves all active Programs.
		/// </summary>
		/// <returns>All active <see cref="Dtos.Student.Requirements.Program">Programs</see></returns>
		/// <note>Programs are cached for 24 hours.</note>
		[HttpGet]
		[HeaderVersionRoute("/programs/active", 2, true, Name = "GetActivePrograms2")]
		public async Task<ActionResult<IEnumerable<Dtos.Student.Requirements.Program>>> GetActivePrograms2Async(bool IncludeEndedPrograms = true)
		{
			try
			{
				var ProgramCollection = (IncludeEndedPrograms) ?
										(await _ProgramRepository.GetAsync()).Where(p => p != null && p.IsActive == true &&
										p.IsSelectable == true)
										.ToList() :
										(await _ProgramRepository.GetAsync()).Where(p => p != null && p.IsActive == true &&
										p.IsSelectable == true && (p.ProgramEndDate == null ||
										(p.ProgramEndDate.HasValue && p.ProgramEndDate >= DateTime.Now)))
										.ToList();

				// Get the right adapter for the type mapping
				var programDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Requirements.Program, Dtos.Student.Requirements.Program>();

				// Map the program entity to the program DTO
				var programDtoCollection = new List<Dtos.Student.Requirements.Program>();
				foreach (var program in ProgramCollection)
				{
					programDtoCollection.Add(programDtoAdapter.MapToType(program));
				}
				return programDtoCollection;
			}
			catch (ColleagueSessionExpiredException e)
			{
				string message = "Session has expired while retrieving active programs";
				_logger.LogError(e, message);
				return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
			}
			catch (Exception e)
			{
				string message = "Exception occurred while retrieving active programs";
				_logger.LogError(e, message);
				return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
			}
		}

		/// <summary>
		/// Retrieves a single Program by ID.
		/// </summary>
		/// <param name="id">Id of program to retrieve</param>
		/// <returns>The requested <see cref="Dtos.Student.Requirements.Program">Program</see></returns>
		[HttpGet]
		[ParameterSubstitutionFilter]
		[QueryStringConstraint(allowOtherKeys: true, "id")]
		[HeaderVersionRoute("/programs", 1, true, Name = "GetProgramsById", Order = -15)]
		public async Task<ActionResult<Dtos.Student.Requirements.Program>> GetProgramsById(string id)
		{
			try
			{
				var program = await _ProgramRepository.GetAsync(id);

				// Get the right adapter for the type mapping
				var programDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Requirements.Program, Dtos.Student.Requirements.Program>();

				// Map the program entity to the program DTO
				var programDto = programDtoAdapter.MapToType(program);

				return programDto;
			}
			catch (ColleagueSessionExpiredException e)
			{
				string message = "Session has expired while retrieving program details";
				_logger.LogError(e, message);
				return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
			}
			catch (Exception e)
			{
				string message = "Exception occurred while retrieving programs details";
				_logger.LogError(e, message);
				return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
			}
		}

		/// <summary>
		/// Retrieves the programs for a list of programs.
		/// </summary>
		/// <param name="programCodes">The list of programs to retrieve</param>
		/// <returns>A collection of <see cref="Dtos.Student.Requirements.Program"> Programs</see></returns>
		/// <accessComments>
		/// Any authenticated user
		/// </accessComments>
		[HttpPost]
		[HeaderVersionRoute("/qapi/programs", 1, true, Name = "GetProgramsByIds")]
		public async Task<ActionResult<IEnumerable<Dtos.Student.Requirements.Program>>> GetProgramsByIdsAsync([FromBody] List<string> programCodes)
		{
			if (programCodes == null || programCodes.Count == 0)
			{
                throw new ArgumentNullException("programCodes", "At least one program code is required");
            }

            try
            {
				var programs = await _programsService.GetProgramsByIdsAsync(programCodes);
                return Ok(programs);
			}
			catch (ColleagueSessionExpiredException tex)
			{
				string message = "Session has expired while retrieving programs";
				_logger.LogError(tex, message);
				return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
			}
			catch (Exception ex)
			{
				string message = "An exception occurred while retrieving programs";
				_logger.LogError(ex, message);
				return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
			}
		}

        /// <summary>
        /// Retrieves program requirements.
        /// </summary>
        /// <param name="id">Id of the program</param>
        /// <param name="catalog">Catalog code</param>
        /// <returns>The <see cref="ProgramRequirements">Program Requirements</see> for the program catalog combination.</returns>
        /// <note>This parameter has support for :ref:`urlcharactersubstitution`</note>
        [ParameterSubstitutionFilter]
		[HttpGet]
		[HeaderVersionRoute("/programs/{id}/{catalog}", 1, true, Name = "GetRequirements")]
		public async Task<ActionResult<ProgramRequirements>> GetRequirementsAsync(string id, string catalog)
		{
			try
			{
				var pr = await _ProgramRequirementsRepository.GetAsync(id, catalog);

				// Get the right adapter for the type mapping
				var programRequirementsDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Requirements.ProgramRequirements, ProgramRequirements>();

				// Map the program requirements entity to the program requirements DTO
				var programRequirementsDto = programRequirementsDtoAdapter.MapToType(pr);

				return programRequirementsDto;
			}
			catch (ColleagueSessionExpiredException e)
			{
				string message = "Session has expired while retrieving program requirements";
				_logger.LogError(e, message);
				return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
			}
			catch (Exception e)
			{
				string message = "Exception occurred while retrieving program requirements";
				_logger.LogError(e, message);
				return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
			}
		}
	}
}
