// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Web.Security;
using System.Net;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    ///  Provides access to Employee Compensation API(s)
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class EmployeeCompensationController : BaseCompressedApiController
    {

        private readonly IEmployeeCompensationService employeeCompensationService;
        private readonly ILogger logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initializes a new instance of the EmployeeCompensationController class.
        /// </summary>
        /// <param name="employeeCompensationService">Service of type <see cref="IEmployeeCompensationService">IEmployeeCompensationService</see></param>
        /// <param name="logger">IEmployeeCompensationService</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public EmployeeCompensationController(IEmployeeCompensationService employeeCompensationService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.employeeCompensationService = employeeCompensationService;
            this.logger = logger;
        }

        /// <summary>
        /// Returns Employee Compensation Details 
        /// </summary>
        /// <param name="effectivePersonId">EmployeeId of a user used for retrieving compensation details </param>
        /// <param name="salaryAmount">Estimated Annual Salary amount
        /// If this value is provided,it will be used in computing compensation details in Total Compensation Colleague Transaction.
        /// When not provided, the salary amount will be computed in Total Compensation Colleague Transaction
        /// </param>
        /// <returns>Employee Compensation DTO containing Compensation Details(Benefit-Deductions,Taxes and Stipends).<see cref="Dtos.HumanResources.EmployeeCompensation"></see> </returns>
        /// <accessComments>
        /// Any authenticated user can
        /// 1) view their own compensation information; 
        /// 2) view other employee's compensation information upon having admin access (i.e. VIEW.ALL.TOTAL.COMPENSATION permission)
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/employee-compensation", 1, true, Name = "GetEmployeeCompensation")]
        public async Task<ActionResult<EmployeeCompensation>> GetEmployeeCompensationAsync(string effectivePersonId = null, decimal? salaryAmount = null)
        {
            
            try
            {
                return await employeeCompensationService.GetEmployeeCompensationAsync(effectivePersonId, salaryAmount);
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("User doesn't have the permission to query the Employee Compensation Information.", HttpStatusCode.Forbidden);
            }
            catch (RepositoryException re)
            {
                var message = re.Message;
                logger.LogError(re, message);
                return CreateHttpResponseException("Database Error occurred while querying the Employee Compensation Information.");
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                var message = "Something unexpected occured.Unable to fetch Employee Compensation Information";
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(message);
            }
        }
    }
}
