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

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to application status category data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class ApplicationStatusCategoriesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;

        /// <summary>
        /// Initializes a new instance of the ApplicationStatusCategoryController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public ApplicationStatusCategoriesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <summary>
        /// Retrieves all Application Status Categories.
        /// </summary>
        /// <returns>All <see cref="ApplicationStatusCategory">Application Status Category</see> codes and descriptions.</returns>
        /// <note>ApplicationStatusCategory is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/application-status-categories", 1, true, Name = "GetApplicationStatusCategories")]
        public async Task<ActionResult<IEnumerable<ApplicationStatusCategory>>> GetAsync()
        {
            var applicationStatusCategoryCollection = await _referenceDataRepository.GetApplicationStatusCategoriesAsync();

            // Get the right adapter for the type mapping
            var applicationStatusCategoryDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.ApplicationStatusCategory, ApplicationStatusCategory>();

            // Map the ApplicationStatusCategory entity to the program DTO
            var applicationStatusCategoryDtoCollection = new List<ApplicationStatusCategory>();
            foreach (var applicationStatusCategory in applicationStatusCategoryCollection)
            {
                applicationStatusCategoryDtoCollection.Add(applicationStatusCategoryDtoAdapter.MapToType(applicationStatusCategory));
            }

            return applicationStatusCategoryDtoCollection;
        }
    }
}
