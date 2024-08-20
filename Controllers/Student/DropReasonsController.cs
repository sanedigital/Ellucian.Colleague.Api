// Copyright 2018-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using Ellucian.Colleague.Dtos.Student;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to the cap size data for graduation.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class DropReasonsController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository referenceDataRepository;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the CapSizesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">Logger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public DropReasonsController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.referenceDataRepository = referenceDataRepository;
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves all Drop Reasons codes and corresponding descriptions.
        /// </summary>
        /// <returns>All <see cref="DropReason">Drop Reason</see> codes and descriptions.</returns>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <note>Drop reason data is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/drop-reasons", 1, true, Name = "GetDropReasons")]
        public async Task<ActionResult<IEnumerable<DropReason>>> GetAsync()
        {
            try
            {
                var dropReasons = await referenceDataRepository.GetDropReasonsAsync();

                // Get the right adapter for the type mapping
                var dropReasonDtoAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.DropReason, DropReason>();

                // Map the DropReason entity to the  DTO
                var dropReasonDtoCollection = new List<DropReason>();
                if (dropReasons != null && dropReasons.Any())
                {
                    foreach (var dropReason in dropReasons)
                    {
                        
                        dropReasonDtoCollection.Add(dropReasonDtoAdapter.MapToType(dropReason));
                    }
                }
                return dropReasonDtoCollection;
            }
            catch (System.Exception e)
            {
                this.logger.LogError(e, "Unable to retrieve the Drop Reasons information");
                return CreateHttpResponseException("Unable to retrieve data");
            }
        }
    }
}
