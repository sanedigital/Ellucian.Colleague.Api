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
    /// Provides access to the registration reason data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class RegistrationReasonsController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository referenceDataRepository;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initializes a new instance of the RegistrationReasonsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">Logger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public RegistrationReasonsController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.referenceDataRepository = referenceDataRepository;
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
        }

        /// <summary>
        /// Returns all registration reasons
        /// </summary>
        /// <returns>Collection of <see cref="RegistrationReason">registration reasons</see></returns>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <note>Registration reason data is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/registration-reasons", 1, true, Name = "GetRegistrationReasonsAsync")]
        public async Task<ActionResult<IEnumerable<RegistrationReason>>> GetRegistrationReasonsAsync()
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
                var registrationReasonDtoCollection = new List<Ellucian.Colleague.Dtos.Student.RegistrationReason>();
                var registrationReasonCollection = await referenceDataRepository.GetRegistrationReasonsAsync(bypassCache);
                var registrationReasonDtoAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.RegistrationReason, RegistrationReason>();
                if (registrationReasonCollection != null && registrationReasonCollection.Count() > 0)
                {
                    foreach (var country in registrationReasonCollection)
                    {
                        try
                        {
                            registrationReasonDtoCollection.Add(registrationReasonDtoAdapter.MapToType(country));
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Error encountered converting registration reason entity to DTO.");
                        }
                    }
                }
                return registrationReasonDtoCollection;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                string message = "Session has expired while retrieving registration reasons data";
                logger.LogError(csse, message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unable to retrieve registration reasons.");
            }
        }
    }
}
