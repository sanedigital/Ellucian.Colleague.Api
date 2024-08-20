// Copyright 2019 -2023 Ellucian Company L.P. and its affiliates.
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
using System.Linq;
using System.Threading.Tasks;

using System.Net;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to the education goal data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class EducationGoalsController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository referenceDataRepository;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initializes a new instance of the EducationGoalsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">Logger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public EducationGoalsController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.referenceDataRepository = referenceDataRepository;
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
        }

        /// <summary>
        /// Returns all education goals
        /// </summary>
        /// <returns>Collection of <see cref="EducationGoal">education goals</see></returns>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <note>Education goal information is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/education-goals", 1, true, Name = "GetEducationGoalsAsync")]
        public async Task<ActionResult<IEnumerable<EducationGoal>>> GetEducationGoalsAsync()
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                var educationGoalDtoCollection = new List<Ellucian.Colleague.Dtos.Student.EducationGoal>();
                var educationGoalCollection = await referenceDataRepository.GetAllEducationGoalsAsync(bypassCache);
                var educationGoalDtoAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.EducationGoal, EducationGoal>();
                if (educationGoalCollection != null && educationGoalCollection.Count() > 0)
                {
                    foreach (var educationGoal in educationGoalCollection)
                    {
                        try
                        {
                            educationGoalDtoCollection.Add(educationGoalDtoAdapter.MapToType(educationGoal));
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Error encountered converting education goal entity to DTO.");
                        }
                    }
                }
                return educationGoalDtoCollection;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                string message = "Session has expired while retrieving educational goals data";
                logger.LogError(csse, message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unable to retrieve education goals.");
            }
        }
    }
}
