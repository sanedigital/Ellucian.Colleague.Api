// Copyright 2021-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// 
    /// </summary>
    /// 
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]

    public class FacultyContractsController : BaseCompressedApiController
    {
        private readonly IFacultyContractService _facultyContractService;
        private readonly ILogger _logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initializes a new instance of the FacultyContractcontroller class.
        /// </summary>
        /// <param name="facultyContractService">Service of type<see cref="IFacultyContractService">IFacultyContractService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public FacultyContractsController(IFacultyContractService facultyContractService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _facultyContractService = facultyContractService;
            this._logger = logger;
        }

        /// <summary>
        /// Query Faculty Contracts
        /// </summary>
        /// <param name="facultyId">Id of the faculty member</param>
        /// <returns></returns>
        /// <accessComments>
        /// Only the current user can get their own faculty contracts. 
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/faculty/{facultyId}/contracts", 1, true, Name = "GetFacultyContractsAsync")]
        public async Task<ActionResult<IEnumerable<FacultyContract>>> GetFacultyContractsAsync(string facultyId)
        {
            if (string.IsNullOrEmpty(facultyId))
            {
                return CreateHttpResponseException("Faculty id required to retrieve faculty contracts");
            }
            
            try
            {
                var facultyContracts = await _facultyContractService.GetFacultyContractsByFacultyIdAsync(facultyId);
                return Ok(facultyContracts);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e, "User does not have permission to retrieve faculty ID " + facultyId);
                return CreateHttpResponseException("You are not authorized to retrieve this contract", HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to retrieve faculty contracts");
                return CreateHttpResponseException("Failed to retrieve faculty contracts");
            }

            
        }
    }
}
