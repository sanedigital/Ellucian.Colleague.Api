// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.ComponentModel;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ellucian.Data.Colleague.Exceptions;
using System;
using System.Net;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Access to Department data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class DepartmentsController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
        private const string unexpectedGenericErrorMessage = "Unexpected error occurred while processing the request.";

        /// <summary>
        /// DepartmentsController Constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Reference data repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="logger"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public DepartmentsController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves all Departments
        /// </summary>
        /// <returns>All <see cref="Department">Department codes and descriptions.</see></returns>
        /// <note>Department is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/departments", 1, true, Name = "GetDepartments")]
        public async Task<ActionResult<IEnumerable<Department>>> GetDepartmentsAsync()
        {
            try
            {
                var departmentCollection = await _referenceDataRepository.DepartmentsAsync();

                // Get the right adapter for the type mapping
                var departmentDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Department, Department>();

                // Map the degree plan entity to the degree plan DTO
                var departmentDtoCollection = new List<Department>();
                foreach (var department in departmentCollection)
                {
                    departmentDtoCollection.Add(departmentDtoAdapter.MapToType(department));
                }

                return departmentDtoCollection;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }

            catch (Exception e)
            {
                _logger.LogError(e, "Unknown error occurred");
                return CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves all active Departments.
        /// </summary>
        /// <returns>All <see cref="Department">active Department codes and descriptions.</see></returns>
        /// <note>Departent is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/departments/active", 1, true, Name = "GetActiveDepartments")]
        public async Task<ActionResult<IEnumerable<Department>>> GetActiveDepartmentsAsync()
        {
            try
            {
                var departmentCollection = await _referenceDataRepository.DepartmentsAsync();

                // Get the right adapter for the type mapping
                var departmentDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Department, Department>();

                // Map the degree plan entity to the degree plan DTO
                var departmentDtoCollection = new List<Department>();
                foreach (var department in departmentCollection)
                {
                    if (department.IsActive)
                    {
                        departmentDtoCollection.Add(departmentDtoAdapter.MapToType(department));
                    }
                }

                return departmentDtoCollection;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
        }
    }
}
