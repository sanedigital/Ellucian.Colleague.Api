// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to nonacademic attendance event type data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class NonAcademicAttendanceEventTypesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initializes a new instance of the NonAcademicAttendanceEventTypesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="studentReferenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public NonAcademicAttendanceEventTypesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository studentReferenceDataRepository,
            ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves all nonacademic attendanc event types.
        /// </summary>
        /// <returns>All <see cref="NonAcademicAttendanceEventType">nonacademic attendance event type</see> codes and descriptions.</returns>
        /// <note>Nonacademic attendance event type data is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/nonacademic-attendance-event-types", 1, true, Name = "GetNonAcademicAttendanceEventTypes")]
        public async Task<ActionResult<IEnumerable<NonAcademicAttendanceEventType>>> GetAsync()
        {
            try
            {
                var nonAcademicAttendanceEventTypeCollection = await _studentReferenceDataRepository.GetNonAcademicAttendanceEventTypesAsync();

                // Get the right adapter for the type mapping
                var nonAcademicAttendanceEventTypeDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.NonAcademicAttendanceEventType, NonAcademicAttendanceEventType>();

                // Map the StudentLoad entity to the program DTO
                var nonAcademicAttendanceEventTypeDtoCollection = new List<NonAcademicAttendanceEventType>();
                foreach (var nonAcademicAttendanceEventType in nonAcademicAttendanceEventTypeCollection)
                {
                    nonAcademicAttendanceEventTypeDtoCollection.Add(nonAcademicAttendanceEventTypeDtoAdapter.MapToType(nonAcademicAttendanceEventType));
                }

                return nonAcademicAttendanceEventTypeDtoCollection;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (ColleagueDataReaderException cdre)
            {
                string message = "An error occurred while trying to read nonacademic attendance event type data from the database.";
                _logger.LogError(message, cdre.ToString());
                return CreateHttpResponseException(message);
            }
            catch (Exception ex)
            {
                string message = "An error occurred while trying to retrieve nonacademic attendance event type information.";
                _logger.LogError(message, ex.ToString());
                return CreateHttpResponseException(message);
            }
        }
    }
}
