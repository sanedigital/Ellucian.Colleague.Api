// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Institution data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class InstitutionsController : BaseCompressedApiController
    {
        private readonly IInstitutionRepository _institutionRepository;
        private readonly IAdapterRegistry _adapterRegistry;

        /// <summary>
        /// InstitutionsController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="institutionRepository">Institution repository of type <see cref="IInstitutionRepository">IInstitutionRepository</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public InstitutionsController(IAdapterRegistry adapterRegistry, IInstitutionRepository institutionRepository, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _institutionRepository = institutionRepository;
        }

        //[CacheControlFilter(MaxAgeHours = 1, Public = true, Revalidate = true)]
        /// <summary>
        /// Gets all institutions.
        /// </summary>
        /// <returns>List of <see cref="Institution">Institutions</see></returns>
        /// <note>Institution is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/institutions", 1, true, Name = "GetInstitutions")]
        public ActionResult<IEnumerable<Institution>> Get()
        {
            var institutionDtoCollection = new List<Institution>();

            try
            {
                var institutionCollection = _institutionRepository.Get();

                // Get the right adapter for the type mapping
                var institutionDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Institution, Institution>();

                // Map the institution entity to the institution DTO

                foreach (var bldg in institutionCollection)
                {
                    institutionDtoCollection.Add(institutionDtoAdapter.MapToType(bldg));
                }
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving all institutions.";
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                string message = "Exception occured while retrieving all institutions.";
                return CreateHttpResponseException(message);
            }
            return Ok(institutionDtoCollection.OrderBy(s => s.InstitutionType));
        }
    }
}
