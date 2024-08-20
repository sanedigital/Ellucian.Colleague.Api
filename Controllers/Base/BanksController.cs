// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using Ellucian.Colleague.Domain.Base.Repositories;
using System.Collections.Generic;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Exposes Bank Routing Information functionality
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class BanksController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IBankRepository bankRepository;
        private readonly IAdapterRegistry adapterRegistry;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Instantiate a new BankRoutingInformationController
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="bankRepository"></param>
        /// <param name="adapterRegistry"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public BanksController(ILogger logger, IBankRepository bankRepository, IAdapterRegistry adapterRegistry, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.logger = logger;
            this.bankRepository = bankRepository;
            this.adapterRegistry = adapterRegistry;
        }

        /// <summary>
        /// Get a bank resource by its id. This endpoint looks for banks known to Colleague's payroll system as well as the
        /// universe of US banks that participate in electronic bank transfers. Canadian banks are only included if they are entered in Colleague's payroll system.
        /// For US Banks, the id is the bank's routing id. For Canadian banks, the id is the combination of the bank's institution id and the branch number, separated by a hyphen
        /// in the following format: {institutionId}-{branchNumber}
        /// </summary>
        /// <param name="id">Routing id of a US bank or institutionId-branchNumber of a Canadian bank</param>
        /// <returns>A Bank object.</returns>
        [HttpGet]
        [HeaderVersionRoute("/banks/{id}", 1, true, Name = "Banks")]
        public async Task<ActionResult<Bank>> GetBankAsync([FromRoute] string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new KeyNotFoundException("Id is required");
            }

            try
            {
                var domainBank = await bankRepository.GetBankAsync(id);
                var domainToDtoBankAdapter = adapterRegistry.GetAdapter<Domain.Base.Entities.Bank, Dtos.Base.Bank>();
                var dtoBank = domainToDtoBankAdapter.MapToType(domainBank);
                return Ok(dtoBank);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, "Unable to find bank");
                return CreateHttpResponseException(knfe.Message, HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown exception occurred");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }

        }
    }
}



