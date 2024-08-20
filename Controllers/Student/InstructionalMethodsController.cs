// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Domain.Student.Repositories;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Coordination.Student.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Filters;
using Ellucian.Data.Colleague.Exceptions;
using System.Net;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Instructional Method data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class InstructionalMethodsController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly ICurriculumService _curriculumService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the InstructionalMethodsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="curriculumService">Service of type <see cref="ICurriculumService">ICurriculumService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public InstructionalMethodsController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, ICurriculumService curriculumService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _curriculumService = curriculumService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves all Instructional Methods.
        /// </summary>
        /// <returns>All <see cref="InstructionalMethod">Instructional Method</see> codes and descriptions.</returns>
        /// <note>InstructionalMethod is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/instructional-methods", 1, false, Name = "GetInstructionalMethods")]
        public async Task<ActionResult<IEnumerable<InstructionalMethod>>> GetAsync()
        {
            try
            {
                var instructionalMethodCollection = await _referenceDataRepository.GetInstructionalMethodsAsync();

                // Get the right adapter for the type mapping
                var instructionalMethodDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.InstructionalMethod, InstructionalMethod>();

                // Map the instructional method entity to the instructional method DTO
                var instructionalMethodDtoCollection = new List<InstructionalMethod>();
                foreach (var instrMethod in instructionalMethodCollection)
                {
                    instructionalMethodDtoCollection.Add(instructionalMethodDtoAdapter.MapToType(instrMethod));
                }

                return instructionalMethodDtoCollection;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving instructional methods";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                string message = "Exception occurred while retrieving instructional methods";
                _logger.LogError(ex, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM version 4</remarks>
        /// <summary>
        /// Retrieves all instructional methods.
        /// </summary>
        /// <returns>All <see cref="Dtos.InstructionalMethod2">InstructionalMethods.</see></returns>
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true }), ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/instructional-methods", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetCdmInstructionalMethods", IsEedmSupported = true)]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.InstructionalMethod2>>> GetInstructionalMethods2Async()
        {
            bool bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                var items = await _curriculumService.GetInstructionalMethods2Async(bypassCache);

                AddEthosContextProperties(
                    await _curriculumService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _curriculumService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        items.Select(i => i.Id).ToList()));

                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM version 4</remarks>
        /// <summary>
        /// Retrieves an instructional method by ID.
        /// </summary>
        /// <returns>A <see cref="Dtos.InstructionalMethod2">InstructionalMethod.</see></returns>
        [ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/instructional-methods/{id}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetInstructionalMethodByGuid", IsEedmSupported = true)]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.InstructionalMethod2>> GetInstructionalMethodById2Async(string id)
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
                var instructionalMethod = await _curriculumService.GetInstructionalMethodById2Async(id);

                if (instructionalMethod != null)
                {

                    AddEthosContextProperties(await _curriculumService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _curriculumService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { instructionalMethod.Id }));
                }

                return instructionalMethod;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Creates a Instructional Method.
        /// </summary>
        /// <param name="instructionalMethod"><see cref="Dtos.InstructionalMethod2">InstructionalMethod</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.InstructionalMethod2">InstructionalMethod</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/instructional-methods", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostInstructionalMethodsV6")]
        public async Task<ActionResult<Dtos.InstructionalMethod2>> PostInstructionalMethodsAsync([FromBody] Dtos.InstructionalMethod2 instructionalMethod)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Updates a Instructional Method.
        /// </summary>
        /// <param name="id">Id of the Instructional Method to update</param>
        /// <param name="instructionalMethod"><see cref="Dtos.InstructionalMethod2">InstructionalMethod</see> to create</param>
        /// <returns>Updated <see cref="Dtos.InstructionalMethod2">InstructionalMethod</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/instructional-methods/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutInstructionalMethodsV6")]
        public async Task<ActionResult<Dtos.InstructionalMethod2>> PutInstructionalMethodsAsync([FromRoute] string id, [FromBody] Dtos.InstructionalMethod2 instructionalMethod)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing Instructional Method
        /// </summary>
        /// <param name="id">Id of the Instructional Method to delete</param>
        [HttpDelete]
        [Route("/instructional-methods/{id}", Name = "DeleteInstructionalMethods", Order = -10)]
        public async Task<IActionResult> DeleteInstructionalMethodsAsync([FromRoute] string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

