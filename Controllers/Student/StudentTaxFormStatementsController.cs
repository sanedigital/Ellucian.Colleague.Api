// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// This is the controller for the type of Student Tax Form Statements.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentTaxFormStatementsController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;
        private readonly IStudentTaxFormStatementService taxFormStatementService;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initialize the Student Tax Form Statement controller.
        /// </summary>
        public StudentTaxFormStatementsController(IAdapterRegistry adapterRegistry, ILogger logger, IStudentTaxFormStatementService taxFormStatementService, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
            this.taxFormStatementService = taxFormStatementService;
        }

        /// <summary>
        /// Returns a set of 1098 tax form statements for the specified person.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <returns>Set of 1098 tax form statements</returns>
        /// <accessComments>
        /// In order to access 1098 statement data, the user must meet one of the following conditions:
        /// 1. Have the admin permission, ViewStudent1098
        /// 2. Have the View1098 permission, and be requesting their own data
        /// 3. Be acting as a Person Proxy for the person whose data they are requesting
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/tax-form-statements/{personId}/Form1098", 2, true, Name = "Get1098TaxFormStatements2")]
        public async Task<ActionResult<IEnumerable<TaxFormStatement3>>> Get10982Async(string personId)
        {
            if (string.IsNullOrEmpty(personId))
                return CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

            try
            {
                return Ok(await taxFormStatementService.Get2Async(personId, Domain.Base.TaxFormTypes.Form1098));
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex, peex.Message);
                return CreateHttpResponseException("Insufficient permissions to view 1098 data.", HttpStatusCode.Forbidden);
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
                return CreateHttpResponseException("Unable to get the 1098 statements.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns a set of T2202A tax form statements for the specified person.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <returns>Set of T2202A tax form statements</returns>
        /// <accessComments>
        /// In order to access T2202A statement data, the user must meet one of the following conditions:
        /// 1. Have the admin permission, ViewStudentT2202A
        /// 2. Have the ViewT2202A permission, and be requesting their own data
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/tax-form-statements/{personId}/FormT2202A", 2, true, Name = "GetT2202ATaxFormStatements2")]
        public async Task<ActionResult<IEnumerable<TaxFormStatement3>>> GetT2202a2Async(string personId)
        {
            if (string.IsNullOrEmpty(personId))
                return CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

            try
            {
                return Ok(await taxFormStatementService.Get2Async(personId, Domain.Base.TaxFormTypes.FormT2202A));
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex, peex.Message);
                return CreateHttpResponseException("Insufficient permissions to view T2202 data.", HttpStatusCode.Forbidden);
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
                return CreateHttpResponseException("Unable to get the T2202 statements.", HttpStatusCode.BadRequest);
            }
        }


        #region OBSOLETE METHODS

        /// <summary>
        /// Returns a set of 1098 tax form statements for the specified person.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <returns>Set of 1098 tax form statements</returns>
        /// <accessComments>
        /// In order to access 1098 statement data, the user must meet one of the following conditions:
        /// 1. Have the admin permission, ViewStudent1098
        /// 2. Have the View1098 permission, and be requesting their own data
        /// 3. Be acting as a Person Proxy for the person whose data they are requesting
        /// </accessComments>
        [Obsolete("Obsolete as of API 1.29.1. Use Get10982Async instead.")]
        [HttpGet]
        [HeaderVersionRoute("/tax-form-statements/{personId}/Form1098", 1, false, Name = "Get1098TaxFormStatements")]
        public async Task<ActionResult<IEnumerable<TaxFormStatement2>>> Get1098Async(string personId)
        {
            if (string.IsNullOrEmpty(personId))
                return CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

            try
            {
                return Ok(await taxFormStatementService.GetAsync(personId, TaxForms.Form1098));
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex, peex.Message);
                return CreateHttpResponseException("Insufficient permissions to view 1098 data.", HttpStatusCode.Forbidden);
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
                return CreateHttpResponseException("Unable to get the 1098 statements.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns a set of T2202A tax form statements for the specified person.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <returns>Set of T2202A tax form statements</returns>
        /// <accessComments>
        /// In order to access T2202A statement data, the user must meet one of the following conditions:
        /// 1. Have the admin permission, ViewStudentT2202A
        /// 2. Have the ViewT2202A permission, and be requesting their own data
        /// </accessComments>
        [Obsolete("Obsolete as of API 1.29.1. Use GetT2202a2Async instead.")]
        [HttpGet]
        [HeaderVersionRoute("/tax-form-statements/{personId}/FormT2202A", 1, false, Name = "GetT2202ATaxFormStatements")]
        public async Task<ActionResult<IEnumerable<TaxFormStatement2>>> GetT2202aAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
                return CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

            try
            {
                return Ok(await taxFormStatementService.GetAsync(personId, TaxForms.FormT2202A));
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex, peex.Message);
                return CreateHttpResponseException("Insufficient permissions to view T2202 data.", HttpStatusCode.Forbidden);
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
                return CreateHttpResponseException("Unable to get the T2202 statements.", HttpStatusCode.BadRequest);
            }
        }

        #endregion
    }
}
