// Copyright 2014-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Exposes FinancialAidOffice and FinancialAidConfiguration Data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class FinancialAidOfficesController : BaseCompressedApiController
    {
        private readonly IFinancialAidOfficeService financialAidOfficeService;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Constructor for FinancialAidOfficesController
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="financialAidOfficeService">FinancialAidOfficeService</param>
        /// <param name="logger">Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public FinancialAidOfficesController(IAdapterRegistry adapterRegistry, IFinancialAidOfficeService financialAidOfficeService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.financialAidOfficeService = financialAidOfficeService;
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
        }

        /// <summary>
        /// Get a list of Financial Aid Offices and their year-based configurations
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>A list of FinancialAidOffice3 objects</returns>
        /// <note>FinancialAidOffice3 is cached for 24 hours.</note>
        [HttpGet]
        [HeaderVersionRoute("/financial-aid-offices", 3, false, Name = "GetFinancialAidOffices3")]
        public async Task<ActionResult<IEnumerable<FinancialAidOffice3>>> GetFinancialAidOffices3Async()
        {
            try
            {
                return Ok(await financialAidOfficeService.GetFinancialAidOffices3Async());
            }
            catch (ColleagueSessionExpiredException csee)
            {
                return CreateHttpResponseException(csee.Message, System.Net.HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateNotFoundException("FinancialAidOffices", string.Empty);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting FinancialAidOffices resource. See log for details." + Environment.NewLine + e.Message);
            }
        }


        /// <summary>
        /// Get a list of Financial Aid Offices and their year-based configurations
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>A list of FinancialAidOffice2 objects</returns>
        [Obsolete("Obsolete as of Api version 1.15, use version 3 of this API")]
        public ActionResult<IEnumerable<FinancialAidOffice2>> GetFinancialAidOffices2()
        {
            try
            {
                return Ok(financialAidOfficeService.GetFinancialAidOffices2());
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateNotFoundException("FinancialAidOffices", string.Empty);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting FinancialAidOffices resource. See log for details." + Environment.NewLine + e.Message);
            }
        }

        /// <summary>
        /// Get a list of Financial Aid Offices and their year-based configurations
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>A list of FinancialAidOffice objects</returns>
        /// <note>FinancialAidOffices is cached for 24 hours</note>
        [Obsolete("Obsolete as of Api version 1.15, use version 3 of this API")]
        [HttpGet]
        [HeaderVersionRoute("/financial-aid-offices", 2, false, Name = "GetFinancialAidOffices2")]
        public async Task<ActionResult<IEnumerable<FinancialAidOffice2>>> GetFinancialAidOffices2Async()
        {
            try
            {
                return Ok(await financialAidOfficeService.GetFinancialAidOffices2Async());
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateNotFoundException("FinancialAidOffices", string.Empty);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting FinancialAidOffices resource. See log for details." + Environment.NewLine + e.Message);
            }
        }

        /// <summary>
        /// Get a list of Financial Aid Offices and their year-based configurations
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources.
        /// </accessComments>
        /// <returns>A list of FinancialAidOffice objects</returns>
        /// <note>FinancialAidOffices are cached for 24 hours</note>
        [Obsolete("Obsolete as of Api version 1.14, use version 2 of this API")]
        [HttpGet]
        [HeaderVersionRoute("/financial-aid-offices", 1, false, Name = "GetFinancialAidOffices")]
        public ActionResult<IEnumerable<FinancialAidOffice>> GetFinancialAidOffices()
        {
            try
            {
                return Ok(financialAidOfficeService.GetFinancialAidOffices());
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateNotFoundException("FinancialAidOffices", string.Empty);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting FinancialAidOffices resource. See log for details." + Environment.NewLine + e.Message);
            }
        }

        /// <summary>
        /// Get a list of Financial Aid Offices and their year-based configurations
        /// </summary>
        /// <returns>A list of FinancialAidOffice objects</returns>
        [Obsolete("Obsolete as of Api version 1.14, use version 2 of this API")]
        public async Task<ActionResult<IEnumerable<FinancialAidOffice>>> GetFinancialAidOfficesAsync()
        {
            try
            {
                return Ok(await financialAidOfficeService.GetFinancialAidOfficesAsync());
            }
            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateNotFoundException("FinancialAidOffices", string.Empty);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unknown error occurred getting FinancialAidOffices resource. See log for details." + Environment.NewLine + e.Message);
            }
        }
    }
}
