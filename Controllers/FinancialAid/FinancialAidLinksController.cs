// Copyright 2014-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Links Controller is used to get links for the Financial Aid Homepage
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class FinancialAidLinksController : BaseCompressedApiController
    {
        private readonly IFinancialAidReferenceDataRepository FinancialAidReferenceDataRepository;
        private readonly IAdapterRegistry AdapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Links Controller constructor
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="financialAidReferenceDataRepository">FinancialAidReferenceDataRepository</param>
        /// <param name="logger">Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public FinancialAidLinksController(IAdapterRegistry adapterRegistry, IFinancialAidReferenceDataRepository financialAidReferenceDataRepository, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            AdapterRegistry = adapterRegistry;
            FinancialAidReferenceDataRepository = financialAidReferenceDataRepository;
            this.logger = logger;
        }

        /// <summary>
        /// Get a list of all Financial Aid Links from Colleague
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>A collection of Links</returns>
        /// <note>Financial-Aid-Links are cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/financial-aid-links", 1, true, Name = "FinancialAidLinks")]
        public ActionResult<IEnumerable<Link>> GetLinks()
        {
            try
            {
                var LinksCollection = FinancialAidReferenceDataRepository.Links;

                //Get the adapter for the type mapping
                var linkDtoAdapter = AdapterRegistry.GetAdapter<Domain.FinancialAid.Entities.Link, Link>();

                //Map the Link entity to the Link dto
                var LinkDtoCollection = new List<Link>();

                foreach (var link in LinksCollection)
                {
                    LinkDtoCollection.Add(linkDtoAdapter.MapToType(link));
                }

                return Ok(LinkDtoCollection);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting Links resource");
            }
        }
    }
}
