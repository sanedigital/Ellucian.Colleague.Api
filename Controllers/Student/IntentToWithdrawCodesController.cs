// Copyright 2021-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to intent to withdraw code data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class IntentToWithdrawCodesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository referenceDataRepository;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the IntentToWithdrawCodesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">Logger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public IntentToWithdrawCodesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.referenceDataRepository = referenceDataRepository;
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
        }

        /// <summary>
        /// Returns all intent to withdraw codes
        /// </summary>
        /// <returns>Collection of <see cref="IntentToWithdrawCode">intent to withdraw codes</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if there was a Colleage data or configuration error.</exception>
        /// <accessComments>Any authenticated user can retrieve intent to withdraw codes.</accessComments>
        /// <note>Intent to withdraw codes are cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/intent-to-withdraw-codes", 1, true, Name = "GetIntentToWithdrawCodesAsync")]
        public async Task<ActionResult<IEnumerable<IntentToWithdrawCode>>> GetIntentToWithdrawCodesAsync()
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
                var dtoCollection = new List<Ellucian.Colleague.Dtos.Student.IntentToWithdrawCode>();
                var entityCollection = await referenceDataRepository.GetIntentToWithdrawCodesAsync(bypassCache);
                var dtoAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.IntentToWithdrawCode, IntentToWithdrawCode>();
                if (entityCollection != null && entityCollection.Count() > 0)
                {
                    foreach (var entity in entityCollection)
                    {
                        try
                        {
                            dtoCollection.Add(dtoAdapter.MapToType(entity));
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Error encountered converting intent to withdraw code entity to DTO.");
                        }
                    }
                }
                return dtoCollection;
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unable to retrieve intent to withdraw codes.");
            }
        }

    }
}
