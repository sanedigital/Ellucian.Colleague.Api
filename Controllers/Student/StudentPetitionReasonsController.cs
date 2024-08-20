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
    public class StudentPetitionReasonsController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// StudentPetitionReasonsController constructor
        /// </summary>
        /// <param name="adapterRegistry">adapterRegistry</param>
        /// <param name="studentReferenceDataRepository">studentReferenceDataRepository</param>
        /// <param name="logger">logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentPetitionReasonsController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository studentReferenceDataRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            this._logger = logger;
        }

        /// <summary>
        /// Get a list of all Student Petition Reasons
        /// </summary>
        /// <returns>A list of <see cref="StudentPetitionReason">StudentPetitionReason</see> codes and descriptions</returns>
        /// <note>StudentPetitionReason is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/student-petition-reasons", 1, true, Name = "GetStudentPetitionReasons")]
        public async Task<ActionResult<IEnumerable<StudentPetitionReason>>> GetAsync()
        {
            try
            {
                var studentPetitionReasonDtos = new List<StudentPetitionReason>();

                var studentPetitionReasons = await _studentReferenceDataRepository.GetStudentPetitionReasonsAsync();

                //Get the adapter and convert to dto
                var petitonReasonDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.StudentPetitionReason, StudentPetitionReason>();

                if (studentPetitionReasons != null && studentPetitionReasons.Count() > 0)
                {
                    foreach (var petitionReason in studentPetitionReasons)
                    {
                        studentPetitionReasonDtos.Add(petitonReasonDtoAdapter.MapToType(petitionReason));
                    }
                }

                return studentPetitionReasonDtos;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving petition reasons";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }

            catch (Exception e)
            {
                string message = "Unable to retrieve StudentPetitionReason.";
                _logger.LogError(e, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }
    }
}
