// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Data.Colleague.Exceptions;
using Microsoft.AspNetCore.Hosting;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// This is the controller for the type of Tax Form Statements.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class HumanResourcesTaxFormStatementsController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;
        private readonly IHumanResourcesTaxFormStatementService taxFormStatementService;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initialize the Tax Form Statement controller.
        /// </summary>
        public HumanResourcesTaxFormStatementsController(IAdapterRegistry adapterRegistry, ILogger logger, IHumanResourcesTaxFormStatementService taxFormStatementService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
            this.taxFormStatementService = taxFormStatementService;
        }

        /// <summary>
        /// Returns W-2 tax form statements for the specified person.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <returns>Set of W-2 tax form statements</returns>
        /// <accessComments>
        /// In order to access W-2 statement data, the user must meet one of the following conditions:
        /// 1. Have the admin permission, ViewEmployeeW2
        /// 2. Have the ViewW2 permission, and be requesting their own data
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/tax-form-statements/{personId}/FormW2", 2, true, Name = "GetW2TaxFormStatements2")]
        public async Task<ActionResult<IEnumerable<TaxFormStatement3>>> GetW22Async(string personId)
        {
            if (string.IsNullOrEmpty(personId))
                return CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

            try
            {
                return Ok(await taxFormStatementService.Get2Async(personId, Domain.Base.TaxFormTypes.FormW2));
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex, peex.Message);
                return CreateHttpResponseException("Insufficient permissions to access W-2 tax form statements.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentOutOfRangeException arex)
            {
                logger.LogError(arex, arex.Message);
                return CreateHttpResponseException("Invalid data.", HttpStatusCode.BadRequest);
            }
            catch (ArgumentNullException anex)
            {
                logger.LogError(anex, anex.Message);
                return CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            // Application and Null Reference exceptions will be caught below.
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Unable to get the W-2 statements", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns 1095-C tax form statements for the specified person.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <returns>Set of 1095-C tax form statements</returns>
        /// <accessComments>
        /// In order to access 1095-C statement data, the user must meet one of the following conditions:
        /// 1. Have the admin permission, ViewEmployee1095C
        /// 2. Have the View1095C permission, and be requesting their own data
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/tax-form-statements/{personId}/Form1095C", 2, true, Name = "Get1095CTaxFormStatements2")]
        public async Task<ActionResult<IEnumerable<TaxFormStatement3>>> Get1095c2Async(string personId)
        {
            if (string.IsNullOrEmpty(personId))
                return CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

            try
            {
                return Ok(await taxFormStatementService.Get2Async(personId, Domain.Base.TaxFormTypes.Form1095C));
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex, peex.Message);
                return CreateHttpResponseException("Insufficient permissions to access 1095C tax form statements.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentOutOfRangeException arex)
            {
                logger.LogError(arex, arex.Message);
                return CreateHttpResponseException("Invalid data.", HttpStatusCode.BadRequest);
            }
            catch (ArgumentNullException anex)
            {
                logger.LogError(anex, anex.Message);
                return CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Unable to get the 1095-C statements", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns T4 tax form statements for the specified person.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <returns>Set of T4 tax form statements</returns>
        /// <accessComments>
        /// In order to access T4 statement data, the user must meet one of the following conditions:
        /// 1. Have the admin permission, ViewEmployeeT4
        /// 2. Have the ViewT4 permission, and be requesting their own data
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/tax-form-statements/{personId}/FormT4", 2, true, Name = "GetT4TaxFormStatements2")]
        public async Task<ActionResult<IEnumerable<TaxFormStatement3>>> GetT42Async(string personId)
        {
            if (string.IsNullOrEmpty(personId))
                return CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);


            try
            {
                return Ok(await taxFormStatementService.Get2Async(personId, Domain.Base.TaxFormTypes.FormT4));
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex, peex.Message);
                return CreateHttpResponseException("Insufficient permissions to access T4 tax form statements.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentOutOfRangeException arex)
            {
                logger.LogError(arex, arex.Message);
                return CreateHttpResponseException("Invalid data.", HttpStatusCode.BadRequest);
            }
            catch (ArgumentNullException anex)
            {
                logger.LogError(anex, anex.Message);
                return CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Unable to get the T4 statements", HttpStatusCode.BadRequest);
            }
        }


        #region OBSOLETE METHODS

        /// <summary>
        /// Returns W-2 tax form statements for the specified person.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <returns>Set of W-2 tax form statements</returns>
        /// <accessComments>
        /// In order to access W-2 statement data, the user must meet one of the following conditions:
        /// 1. Have the admin permission, ViewEmployeeW2
        /// 2. Have the ViewW2 permission, and be requesting their own data
        /// </accessComments>
        [Obsolete("Obsolete as of API 1.29.1. Use GetW22Async instead.")]
        [HttpGet]
        [HeaderVersionRoute("/tax-form-statements/{personId}/FormW2", 1, false, Name = "GetW2TaxFormStatements")]
        public async Task<ActionResult<IEnumerable<TaxFormStatement2>>> GetW2Async(string personId)
        {
            if (string.IsNullOrEmpty(personId))
                return CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

            try
            {
                return Ok(await taxFormStatementService.GetAsync(personId, TaxForms.FormW2));
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex, peex.Message);
                return CreateHttpResponseException("Insufficient permissions to access W-2 tax form statements.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentOutOfRangeException arex)
            {
                logger.LogError(arex, arex.Message);
                return CreateHttpResponseException("Invalid data.", HttpStatusCode.BadRequest);
            }
            catch (ArgumentNullException anex)
            {
                logger.LogError(anex, anex.Message);
                return CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            // Application and Null Reference exceptions will be caught below.
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Unable to get the W-2 statements", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns 1095-C tax form statements for the specified person.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <returns>Set of 1095-C tax form statements</returns>
        /// <accessComments>
        /// In order to access 1095-C statement data, the user must meet one of the following conditions:
        /// 1. Have the admin permission, ViewEmployee1095C
        /// 2. Have the View1095C permission, and be requesting their own data
        /// </accessComments>
        [Obsolete("Obsolete as of API 1.29.1. Use Get1095c2Async instead.")]
        [HttpGet]
        [HeaderVersionRoute("/tax-form-statements/{personId}/Form1095C", 1, false, Name = "Get1095CTaxFormStatements")]
        public async Task<ActionResult<IEnumerable<TaxFormStatement2>>> Get1095cAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
                return CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

            try
            {
                return Ok(await taxFormStatementService.GetAsync(personId, TaxForms.Form1095C));
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex, peex.Message);
                return CreateHttpResponseException("Insufficient permissions to access 1095C tax form statements.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentOutOfRangeException arex)
            {
                logger.LogError(arex, arex.Message);
                return CreateHttpResponseException("Invalid data.", HttpStatusCode.BadRequest);
            }
            catch (ArgumentNullException anex)
            {
                logger.LogError(anex, anex.Message);
                return CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Unable to get the 1095-C statements", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns T4 tax form statements for the specified person.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <returns>Set of T4 tax form statements</returns>
        /// <accessComments>
        /// In order to access T4 statement data, the user must meet one of the following conditions:
        /// 1. Have the admin permission, ViewEmployeeT4
        /// 2. Have the ViewT4 permission, and be requesting their own data
        /// </accessComments>
        [Obsolete("Obsolete as of API 1.29.1. Use GetT42Async instead.")]
        [HttpGet]
        [HeaderVersionRoute("/tax-form-statements/{personId}/FormT4", 1, false, Name = "GetT4TaxFormStatements")]
        public async Task<ActionResult<IEnumerable<TaxFormStatement2>>> GetT4Async(string personId)
        {
            if (string.IsNullOrEmpty(personId))
                return CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);


            try
            {
                return Ok(await taxFormStatementService.GetAsync(personId, TaxForms.FormT4));
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex, peex.Message);
                return CreateHttpResponseException("Insufficient permissions to access T4 tax form statements.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentOutOfRangeException arex)
            {
                logger.LogError(arex, arex.Message);
                return CreateHttpResponseException("Invalid data.", HttpStatusCode.BadRequest);
            }
            catch (ArgumentNullException anex)
            {
                logger.LogError(anex, anex.Message);
                return CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Unable to get the T4 statements", HttpStatusCode.BadRequest);
            }
        }

        #endregion
    }
}
