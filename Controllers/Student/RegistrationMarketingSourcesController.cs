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
using System.Net;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to the registration marketing source data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class RegistrationMarketingSourcesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository referenceDataRepository;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initializes a new instance of the RegistrationMarketingSourcesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">Logger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public RegistrationMarketingSourcesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.referenceDataRepository = referenceDataRepository;
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
        }

        /// <summary>
        /// Returns all registration marketing sources
        /// </summary>
        /// <returns>Collection of <see cref="RegistrationMarketingSource">registration marketing sources</see></returns>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <note>Registration marketing source data is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/registration-marketing-sources", 1, true, Name = "GetRegistrationMarketingSourcesAsync")]
        public async Task<ActionResult<IEnumerable<RegistrationMarketingSource>>> GetRegistrationMarketingSourcesAsync()
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
                var registrationMarketingSourceDtoCollection = new List<Ellucian.Colleague.Dtos.Student.RegistrationMarketingSource>();
                var registrationMarketingSourceCollection = await referenceDataRepository.GetRegistrationMarketingSourcesAsync(bypassCache);
                var registrationMarketingSourceDtoAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.RegistrationMarketingSource, RegistrationMarketingSource>();
                if (registrationMarketingSourceCollection != null && registrationMarketingSourceCollection.Count() > 0)
                {
                    foreach (var country in registrationMarketingSourceCollection)
                    {
                        try
                        {
                            registrationMarketingSourceDtoCollection.Add(registrationMarketingSourceDtoAdapter.MapToType(country));
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Error encountered converting registration marketing source entity to DTO.");
                        }
                    }
                }
                return registrationMarketingSourceDtoCollection;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                string message = "Session has expired while retrieving registration marketing sources data";
                logger.LogError(csse, message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unable to retrieve registration marketing sources.");
            }
        }
    }
}
