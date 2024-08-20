// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Filters;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Microsoft.Extensions.Logging;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using System.Threading.Tasks;
using Ellucian.Data.Colleague.Exceptions;
using System.Net;
using System;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Non Courses (Tests) data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class TestsController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _testRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the TestsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="testRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public TestsController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository testRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _testRepository = testRepository;
            this._logger = logger;
        }

        /// <summary>
        /// Gets information for all Tests
        /// </summary>
        /// <returns>All <see cref="Ellucian.Colleague.Domain.Student.Entities.Test">Tests</see></returns>
        /// [CacheControlFilter(Public = true, MaxAgeHours = 1, Revalidate = true)]
        /// <note>Test is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/tests", 1, true, Name = "GetTests")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Student.Test>>> GetAsync()
        {
            var testDtoCollection = new List<Ellucian.Colleague.Dtos.Student.Test>();
            try
            {
                var testCollection = await _testRepository.GetTestsAsync();
                // Get the right adapter for the type mapping
                var testDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Test, Ellucian.Colleague.Dtos.Student.Test>();
                // Map the Test entity to the grade DTO
                foreach (var test in testCollection)
                {
                    testDtoCollection.Add(testDtoAdapter.MapToType(test));
                }
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while fetching tests";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(e.Message);
            }
            return testDtoCollection;

        }
    }
}
