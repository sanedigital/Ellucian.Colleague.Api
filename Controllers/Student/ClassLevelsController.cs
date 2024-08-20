// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Domain.Student.Repositories;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using System.Threading.Tasks;
using Ellucian.Data.Colleague.Exceptions;
using Microsoft.Extensions.Logging;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Class Level data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class ClassLevelsController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initializes a new instance of the ClassLevelsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public ClassLevelsController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves all Class Levels.
        /// </summary>
        /// <returns>All <see cref="ClassLevel">Class Level</see> codes and descriptions.</returns>
        /// <note>ClassLevel is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/class-levels", 1, true, Name = "GetClassLevels")]
        public async Task<ActionResult<IEnumerable<ClassLevel>>> GetAsync()
        {
            try
            {
                var classLevelCollection = await _referenceDataRepository.GetClassLevelsAsync();

                // Get the right adapter for the type mapping
                var classLevelDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.ClassLevel, ClassLevel>();

                // Map the courselevel entity to the program DTO
                var classLevelDtoCollection = new List<ClassLevel>();
                foreach (var classLevel in classLevelCollection)
                {
                    classLevelDtoCollection.Add(classLevelDtoAdapter.MapToType(classLevel));
                }

                return classLevelDtoCollection;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, "Session has expired while retrieving class levels");
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                var message = "An error occurred while retrieving class levels.";
                logger.LogError(ex, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }
    }
}
