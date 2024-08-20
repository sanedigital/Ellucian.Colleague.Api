// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using System.Net;
using System.Net.Http.Headers;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
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
    /// Provides access to TopicCodes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class TopicCodesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;


        /// <summary>
        /// TopicCodesController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        ///<param name="logger">Repository of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>

        /// <param name="apiSettings"></param>
        public TopicCodesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            this._logger = logger;

        }

        /// <summary>
        /// Retrieves all Topic Codes.
        /// </summary>
        /// <returns>All <see cref="TopicCode">Topic Code</see> codes and descriptions</returns>
        /// <note>TopicCode is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/topic-codes", 1, true, Name = "GetTopicCodes")]
        public async Task<ActionResult<IEnumerable<TopicCode>>> GetAsync()
        {
            try
            {
                var topicCodeCollection = await _referenceDataRepository.GetTopicCodesAsync(false);

                // Get the right adapter for the type mapping
                var topicCodeDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.TopicCode, TopicCode>();

                // Map the academiclevel entity to the program DTO
                var topicCodeDtoCollection = new List<TopicCode>();
                foreach (var topicCode in topicCodeCollection)
                {
                    topicCodeDtoCollection.Add(topicCodeDtoAdapter.MapToType(topicCode));
                }

                return topicCodeDtoCollection;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving topic codes";
                _logger.LogError(tex, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                string message = "Exception occurred while retrieving topic codes";
                _logger.LogError(ex, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }
    }
}
