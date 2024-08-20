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
    /// Provides access to Noncourse status data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class NoncourseStatusesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the NoncourseStatusesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="studentReferenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public NoncourseStatusesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository studentReferenceDataRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves all Noncourse Statuses.
        /// </summary>
        /// <returns>All <see cref="Ellucian.Colleague.Domain.Student.Entities.NoncourseStatus">Noncourse Statuses</see></returns>
        /// [CacheControlFilter(Public = true, MaxAgeHours = 1, Revalidate = true)]
        /// <note>NoncourseStatus is cached for 24 hours.</note>
        [HttpDelete]

        //"NonpersonRelationships", action = "GetNonpersonRelationshipsByGuidAsync", isEedmSupported = true }

        //"NonpersonRelationships", action = "GetNonpersonRelationshipsByGuidAsync", isEedmSupported = true, RequestedContentType = (RouteConstants.HedtechIntegrationMediaTypeFormat, 13) }

        //"NonpersonRelationships", action = "GetNonpersonRelationshipsAsync", isEedmSupported = true }
        //"NonpersonRelationships", action = "GetNonpersonRelationshipsAsync", isEedmSupported = true, RequestedContentType = (RouteConstants.HedtechIntegrationMediaTypeFormat, 13) }
        //"NonpersonRelationships", action = "PutNonpersonRelationshipsAsync" }
        //"NonpersonRelationships", action = "PostNonpersonRelationshipsAsync" }
        //"NonpersonRelationships", action = "DeleteNonpersonRelationshipsAsync" }
        //"NonpersonRelationships", action = "GetNonpersonRelationshipsByGuidAsync", isEedmSupported = true }
        //"NonpersonRelationships", action = "GetNonpersonRelationshipsByGuidAsync", isEedmSupported = true, RequestedContentType = (RouteConstants.HedtechIntegrationMediaTypeFormat, 13) }
        //"NonpersonRelationships", action = "GetNonpersonRelationshipsAsync", isEedmSupported = true }
        //"NonpersonRelationships", action = "GetNonpersonRelationshipsAsync", isEedmSupported = true, RequestedContentType = (RouteConstants.HedtechIntegrationMediaTypeFormat, 13) }
        //"NonpersonRelationships", action = "PutNonpersonRelationshipsAsync" }
        //"NonpersonRelationships", action = "PostNonpersonRelationshipsAsync" }
        //"NonpersonRelationships", action = "DeleteNonpersonRelationshipsAsync" }
        [HttpGet]
        [HeaderVersionRoute("/noncourse-statuses", 1, true, Name = "GetNoncourseStatuses")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Student.NoncourseStatus>>> GetAsync()
        {
            var noncourseStatusDtoCollection = new List<Ellucian.Colleague.Dtos.Student.NoncourseStatus>();
            try
            {
                var noncourseStatusCollection = await _studentReferenceDataRepository.GetNoncourseStatusesAsync();
                // Get the right adapter for the type mapping
                var noncourseStatusDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.NoncourseStatus, Ellucian.Colleague.Dtos.Student.NoncourseStatus>();
                // Map the NoncourseStatus entity to the NoncourseStatus DTO
                foreach (var status in noncourseStatusCollection)
                {
                    noncourseStatusDtoCollection.Add(noncourseStatusDtoAdapter.MapToType(status));
                }
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while fetching non-course statuses";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(e.Message);
            }

            return noncourseStatusDtoCollection;

        }
    }
}
