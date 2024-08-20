// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using System.Collections.Generic;
using System.ComponentModel;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Web.Security;
using System.Net;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Api.Utility;
using System.Net.Http;
using System.Text.RegularExpressions;
using Ellucian.Data.Colleague.Exceptions;
using Microsoft.AspNetCore.Hosting;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    ///  BenefitsEnrollment controller
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class BenefitsEnrollmentController : BaseCompressedApiController
    {
        private readonly IBenefitsEnrollmentService benefitsEnrollmentService;
        private readonly ILogger logger;
        private readonly IWebHostEnvironment hostEnvironment;
        private const string noEmployeeIDErrorMessage = "EmployeeId is required.";
        private const string noBenefitEnrollmentPoolDTOErrorMessage = "Benefit Enrollment Pool DTO is required in body of request";
        private const string organizationOrLastNameRequiredMessage = "OrganizationName or LastName is required.";
        private const string benefitElectionsActionFailureMessage = "Unable to submit/reopen the benefit elections.";
        private const string noBenefitEnrollmentCompletionCriteriaErrorMessage = "BenefitEnrollmentCompletionCriteria DTO is required in the body of the request";
        private const string invalidBenefitEnrollmentCompletionCriteriaErrorMessage = "Required parameters of BenefitEnrollmentCompletionCriteria DTO contain invalid values.";
        private const string beneficiaryCategoryFailureMessage = "Unable to get beneficiary categories";
        private const string forbiddenSubmitOrReopenErrorMessage = "User does not have the permission to submit/re-open the elected benefits of others";
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
        private const string invalidPermissionsErrorMessage = "The current user does not have the permissions to perform the requested operation.";
        private const string unexpectedGenericErrorMessage = "Unexpected error occurred while processing the request.";

        /// <summary>
        /// Initializes a new instance of the BenefitsEnrollmentController class
        /// </summary>
        /// <param name="benefitsEnrollmentService">Service of type <see cref="IBenefitsEnrollmentService">IBenefitsEnrollmentService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor">Interface to the action context accessor</param>
        /// <param name="hostEnvironment"></param>
        /// <param name="apiSettings"></param>
        public BenefitsEnrollmentController(IBenefitsEnrollmentService benefitsEnrollmentService, ILogger logger, IActionContextAccessor actionContextAccessor, IWebHostEnvironment hostEnvironment, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.benefitsEnrollmentService = benefitsEnrollmentService;
            this.logger = logger;
            this.hostEnvironment = hostEnvironment;
        }

        /// <summary>
        /// Returns benefits enrollment eligibility for an employee
        /// </summary>
        /// <param name="employeeId">Id of employee to request benefits enrollment eligibility</param>
        /// <returns>EmployeeBenefitsEnrollmentEligibility dto containing the enrollment period if eligible or a reson for ineligibility.<see cref="Dtos.HumanResources.EmployeeBenefitsEnrollmentEligibility"></see> </returns>
        /// <accessComments>
        /// An authenticated user can view their own benefits enrollment eligibility.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/employees/{employeeId}/benefits-enrollment-eligibility", 1, true, Name = "GetEmployeeBenefitsEnrollmentEligibilityAsync")]
        public async Task<ActionResult<EmployeeBenefitsEnrollmentEligibility>> GetEmployeeBenefitsEnrollmentEligibilityAsync(string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                throw new ArgumentNullException("employeeId");
            }
            try
            {
                logger.LogDebug("*******Start - Process to get benefits enrollment eligibility for an employee - Start***********");
                var eligibility = await benefitsEnrollmentService.GetEmployeeBenefitsEnrollmentEligibilityAsync(employeeId);
                logger.LogDebug("*******End - Process to get benefits enrollment eligibility for an employee is successful - End***********");
                return eligibility;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, "User does not have permission to access EmployeeBenefitsEnrollmentEligibility for " + employeeId);
                return CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(unexpectedGenericErrorMessage);
            }
        }

        /// <summary>
        /// Returns benefits enrollment pool items (dependent and beneficiary information) for an employee
        /// </summary>
        /// <param name="employeeId">Id of employee to request benefits enrollment pool items</param>
        /// <returns>List of EmployeeBenefitsEnrollmentPoolItem dtos containing the dependent and beneficiary information for employee <see cref="Dtos.HumanResources.EmployeeBenefitsEnrollmentPoolItem"></see></returns>
        /// <accessComments>
        /// An authenticated user can view their own benefits enrollment pool items
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/employees/{employeeId}/benefits-enrollment-pool", 1, true, Name = "GetEmployeeBenefitsEnrollmentPoolAsync")]
        public async Task<ActionResult<IEnumerable<EmployeeBenefitsEnrollmentPoolItem>>> GetEmployeeBenefitsEnrollmentPoolAsync(string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                throw new ArgumentNullException("employeeId");
            }
            try
            {
                logger.LogDebug("*******Start - Process to get benefits enrollment pool items (dependent and beneficiary information) for an employee - Start***********");
                var poolItems = await benefitsEnrollmentService.GetEmployeeBenefitsEnrollmentPoolAsync(employeeId);
                logger.LogDebug("*******End - Process to get benefits enrollment pool items (dependent and beneficiary information) for an employee is successful - End***********");
                return Ok(poolItems);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, "User does not have permission to access EmployeeBenefitsEnrollmentPool for " + employeeId);
                return CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(unexpectedGenericErrorMessage);
            }
        }

        /// <summary>
        /// Gets EmployeeBenefitsEnrollmentPackage object for the specified employee id
        /// </summary>
        /// <param name="employeeId">employee id for whom to get benefits enrollment package</param>
        /// <param name="enrollmentPeriodId">(optional) enrollment perod id</param>
        /// <accessComments>
        /// An authenticated user can view their own benefits enrollment package
        /// </accessComments>
        /// <returns>EmployeeBenefitsEnrollmentPackage DTO</returns>
        [HttpGet]
        [HeaderVersionRoute("/employees/{employeeId}/benefits-enrollment-package", 1, true, Name = "GetEmployeeBenefitsEnrollmentPackageAsync")]
        public async Task<ActionResult<EmployeeBenefitsEnrollmentPackage>> GetEmployeeBenefitsEnrollmentPackageAsync(string employeeId, string enrollmentPeriodId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(employeeId))
                {
                    throw new ArgumentNullException("employeeId");
                }
                logger.LogDebug("*******Start - Process to get EmployeeBenefitsEnrollmentPackage object for the specified employee - Start***********");
                var package = await benefitsEnrollmentService.GetEmployeeBenefitsEnrollmentPackageAsync(employeeId);
                logger.LogDebug("*******End - Process to get EmployeeBenefitsEnrollmentPackage object for the specified employee is successful - End***********");
                return package;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, "User does not have permission to access EmployeeBenefitsEnrollmentPackage for " + employeeId);
                return CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(unexpectedGenericErrorMessage);
            }
        }

        /// <summary>
        /// This endpoint will adds new benefits enrollment pool information to an employee
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can add benefits enrollment pool information.     
        /// The endpoint will reject the add of benefits enrollment pool information if the employee does not have a valid permission.
        /// </accessComments>       
        /// <param name="employeeId">Required parameter to add benefits enrollment pool information to an employee</param>
        /// <param name="employeeBenefitsEnrollmentPoolItem"><see cref="EmployeeBenefitsEnrollmentPoolItem">EmployeeBenefitsEnrollmentPoolItem DTO</see></param>
        /// <returns><see cref="EmployeeBenefitsEnrollmentPoolItem">Newly added EmployeeBenefitsEnrollmentPoolItem DTO object</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the employeeId or employeeBenefitsEnrollmentPoolItem are not present in the request or any unexpected error has occured.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the employeeBenefitsEnrollmentPoolItem is not present in the request or any unexpected error has occured.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user is not allowed to add benefits enrollment pool information.</exception>
        [HttpPost]
        [HeaderVersionRoute("/employees/{employeeId}/benefits-enrollment-pool", 1, true, Name = "AddEmployeeBenefitsEnrollmentPoolAsync")]
        public async Task<ActionResult<EmployeeBenefitsEnrollmentPoolItem>> AddEmployeeBenefitsEnrollmentPoolAsync([FromRoute] string employeeId, [FromBody] EmployeeBenefitsEnrollmentPoolItem employeeBenefitsEnrollmentPoolItem)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                return CreateHttpResponseException(noEmployeeIDErrorMessage);
            }

            if (employeeBenefitsEnrollmentPoolItem == null)
            {
                return CreateHttpResponseException(noBenefitEnrollmentPoolDTOErrorMessage);
            }

            if (string.IsNullOrWhiteSpace(employeeBenefitsEnrollmentPoolItem.OrganizationName) && string.IsNullOrWhiteSpace(employeeBenefitsEnrollmentPoolItem.LastName))
            {
                return CreateHttpResponseException(organizationOrLastNameRequiredMessage);
            }

            try
            {
                logger.LogDebug("*******Start - Process to add new benefits enrollment pool information to an employee - Start***********");
                var addBenefits = await benefitsEnrollmentService.AddEmployeeBenefitsEnrollmentPoolAsync(employeeId, employeeBenefitsEnrollmentPoolItem);
                logger.LogDebug("*******End - Process to add new benefits enrollment pool information to an employee is successful - End***********");
                return addBenefits;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// This endpoint will update benefits enrollment pool information of an employee
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can update their own benefits enrollment pool information .     
        /// The endpoint will reject the updated benefits enrollment pool information if the employee does not have a valid permission.
        /// </accessComments>       
        /// <param name="employeeId">Required parameter to update benefits enrollment pool information of an employee</param>
        /// <param name="employeeBenefitsEnrollmentPoolItem"><see cref="EmployeeBenefitsEnrollmentPoolItem">EmployeeBenefitsEnrollmentPoolItem DTO</see></param>
        /// <returns><see cref="EmployeeBenefitsEnrollmentPoolItem">Updated EmployeeBenefitsEnrollmentPoolItem DTO object</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the employeeId, employeeBenefitsEnrollmentPoolItem or employeeBenefitsEnrollmentPoolItem.Id are not present in the request or any unexpected error has occured.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user is not allowed to add benefits enrollment pool information.</exception>
        [HttpPut]
        [HeaderVersionRoute("/employees/{employeeId}/benefits-enrollment-pool", 1, true, Name = "UpdateEmployeeBenefitsEnrollmentPoolAsync")]
        public async Task<ActionResult<EmployeeBenefitsEnrollmentPoolItem>> UpdateEmployeeBenefitsEnrollmentPoolAsync([FromRoute] string employeeId, [FromBody] EmployeeBenefitsEnrollmentPoolItem employeeBenefitsEnrollmentPoolItem)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                return CreateHttpResponseException(noEmployeeIDErrorMessage);
            }

            if (employeeBenefitsEnrollmentPoolItem == null)
            {
                return CreateHttpResponseException(noBenefitEnrollmentPoolDTOErrorMessage);
            }

            if (string.IsNullOrEmpty(employeeBenefitsEnrollmentPoolItem.Id))
            {
                string message = "Benefit Enrollment Pool Id is required in body of request";
                return CreateHttpResponseException(message);
            }

            if (string.IsNullOrWhiteSpace(employeeBenefitsEnrollmentPoolItem.OrganizationName) && string.IsNullOrWhiteSpace(employeeBenefitsEnrollmentPoolItem.LastName))
            {
                return CreateHttpResponseException(organizationOrLastNameRequiredMessage);
            }

            try
            {
                logger.LogDebug("*******Start - Process to update new benefits enrollment pool information to an employee - Start***********");
                var updateBenefits = await benefitsEnrollmentService.UpdateEmployeeBenefitsEnrollmentPoolAsync(employeeId, employeeBenefitsEnrollmentPoolItem);
                logger.LogDebug("*******End - Process to update new benefits enrollment pool information to an employee is successful - End***********");
                return updateBenefits;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
            }

            catch (KeyNotFoundException knfe)
            {
                logger.LogError(knfe, knfe.Message);
                return CreateNotFoundException("BenefitsEnrollmentPool", employeeBenefitsEnrollmentPoolItem.Id);
            }

            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// This endpoint will update benefits enrollment information of an employee for the given benefit types specified
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can update their own benefits enrollment information
        /// The endpoint will reject the updated benefits enrollment information if the employee does not have a valid permission
        /// </accessComments>    
        /// <param name="employeeId">Required parameter to update benefits enrollment information</param>
        /// <param name="employeeBenefitsEnrollmentInfo"><see cref="EmployeeBenefitsEnrollmentInfo">EmployeeBenefitsEnrollmentInfo DTO</see></param>
        /// <returns><see cref="EmployeeBenefitsEnrollmentInfo">Updated EmployeeBenefitsEnrollmentInfo DTO object</see></returns>
        [HttpPut]
        [HeaderVersionRoute("/employees/{employeeId}/benefits-enrollment-info", 1, true, Name = "UpdateEmployeeBenefitsEnrollmentInfoAsync")]
        public async Task<ActionResult<IAsyncResult>> UpdateEmployeeBenefitsEnrollmentInfoAsync([FromRoute] string employeeId, [FromBody] EmployeeBenefitsEnrollmentInfo employeeBenefitsEnrollmentInfo)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                return CreateHttpResponseException(noEmployeeIDErrorMessage);
            }

            if (employeeBenefitsEnrollmentInfo == null)
            {
                string message = "EmployeeBenefitsEnrollmentInfo DTO is required in body of request";
                return CreateHttpResponseException(message);
            }

            if (string.IsNullOrEmpty(employeeBenefitsEnrollmentInfo.EmployeeId))
            {
                string message = "EmployeeId is required in body of request";
                return CreateHttpResponseException(message);
            }

            if (string.IsNullOrEmpty(employeeBenefitsEnrollmentInfo.EnrollmentPeriodId))
            {
                string message = "EnrollmentPeriodId is required in body of request";
                return CreateHttpResponseException(message);
            }

            if (string.IsNullOrEmpty(employeeBenefitsEnrollmentInfo.BenefitPackageId))
            {
                string message = "BenefitPackageId is required in body of request";
                return CreateHttpResponseException(message);
            }

            try
            {
                logger.LogDebug("*******Start - Process to update benefits enrollment information of an employee for the given benefit types specified - Start***********");
                var updateBenefitsEnrollmentInfo = await benefitsEnrollmentService.UpdateEmployeeBenefitsEnrollmentInfoAsync(employeeBenefitsEnrollmentInfo);
                logger.LogDebug("*******End - Process to update benefits enrollment information of an employee for the given benefit types specified is successful - End***********");
                return Ok(updateBenefitsEnrollmentInfo);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
            }

            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Queries enrollment period benefits based on specified criteria
        /// </summary>
        /// <param name="criteria"></param>
        /// <accessComments>Any authenticated user can get enrollment period benefits information
        /// </accessComments>
        /// <returns>Set of enrollment period benefits</returns>
        [HttpPost]
        [HeaderVersionRoute("/qapi/benefits-enrollment-period-benefits", 1, true, Name = "QueryEnrollmentPeriodBenefitsAsync")]
        public async Task<ActionResult<IEnumerable<EnrollmentPeriodBenefit>>> QueryEnrollmentPeriodBenefitsAsync(BenefitEnrollmentBenefitsQueryCriteria criteria)
        {
            try
            {
                if (criteria == null)
                {
                    throw new ArgumentNullException("criteria");
                }
                logger.LogDebug("*******Start - Process to query enrollment period benefits based on specified criteria - Start***********");
                var benefits = await benefitsEnrollmentService.QueryEnrollmentPeriodBenefitsAsync(criteria);
                logger.LogDebug("*******End - Process to query enrollment period benefits based on specified criteria is successful - End***********");
                return Ok(benefits);
            }
            catch (RepositoryException re)
            {
                logger.LogError(re, re.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(re));
            }
            catch (ArgumentNullException ane)
            {
                logger.LogError(ane, ane.Message);
                return CreateHttpResponseException("Unable to get enrollment period benefits", HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Unable to get enrollment period benefits", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Queries benefits enrollment information based on specified criteria; if no benefit type is provided, all of the employee's elected benefit information is returned
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can view their own benefits enrollment information
        /// </accessComments>    
        /// <param name="criteria">Required parameter used to query employee benefits enrollment information</param>
        /// <returns><see cref="EmployeeBenefitsEnrollmentInfo">EmployeeBenefitsEnrollmentInfo DTO object</see></returns>
        [HttpPost]
        [HeaderVersionRoute("/qapi/benefits-enrollment-info", 1, true, Name = "QueryEmployeeBenefitsEnrollmentInfoAsync")]
        public async Task<ActionResult<EmployeeBenefitsEnrollmentInfo>> QueryEmployeeBenefitsEnrollmentInfoAsync(EmployeeBenefitsEnrollmentInfoQueryCriteria criteria)
        {
            try
            {
                if (criteria == null)
                {
                    throw new ArgumentNullException("criteria");
                }
                logger.LogDebug("*******Start - Process to query benefits enrollment information based on specified criteria - Start***********");
                var benefits = await benefitsEnrollmentService.QueryEmployeeBenefitsEnrollmentInfoAsync(criteria);
                logger.LogDebug("*******End - Process to query benefits enrollment information based on specified criteria is successful - End***********");
                return benefits;
            }
            catch (RepositoryException re)
            {
                logger.LogError(re, re.Message);
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(re));
            }
            catch (ArgumentNullException ane)
            {
                logger.LogError(ane, ane.Message);
                return CreateHttpResponseException("Unable to get benefits enrollment information", HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// This end-point submits/re-opens the benefits elected by an employee.
        /// A boolean flag present in the input criteria object indicates whether to submit or re-open the benefit elections.
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can submit/re-open their own elected benefits.      
        /// </accessComments>   
        /// <param name="criteria">BenefitEnrollmentCompletionCriteria object</param>
        /// <returns><see cref="BenefitEnrollmentCompletionInfo">BenefitEnrollmentCompletionInfo DTO</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the required parameters in the input object has no value (or) in case of any unexpected error while processing the request.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if a user tries to submit/re-open elected benefits of others.</exception>
        [HttpPost]
        [HeaderVersionRoute("/employees/benefit-elections", 1, true, Name = "SubmitOrReOpenBenefitElections")]
        public async Task<ActionResult<BenefitEnrollmentCompletionInfo>> SubmitOrReOpenBenefitElectionsAsync([FromBody] BenefitEnrollmentCompletionCriteria criteria)
        {
            try
            {
                if (criteria == null)
                {
                    throw new ArgumentNullException("criteria", "BenefitEnrollmentCompletionCriteria is required");
                }

                if (string.IsNullOrWhiteSpace(criteria.EmployeeId))
                {
                    throw new ArgumentException("Employee Id is required");
                }
                if (string.IsNullOrWhiteSpace(criteria.EnrollmentPeriodId))
                {
                    throw new ArgumentException("Enrollment Period Id is required");
                }
                if (criteria.SubmitBenefitElections && string.IsNullOrWhiteSpace(criteria.BenefitsPackageId))
                {
                    throw new ArgumentException("BenefitsPackageId is required");
                }
                logger.LogDebug("*******Start - Process to submit/re-open the benefits elected by an employee - Start***********");
                var benefitsEnrollmentCompletionInfo = await benefitsEnrollmentService.SubmitOrReOpenBenefitElectionsAsync(criteria);
                logger.LogDebug("*******End - Process to submit/re-open the benefits elected by an employee is successful - End***********");
                return benefitsEnrollmentCompletionInfo;
            }
            catch (ArgumentNullException ane)
            {
                logger.LogError(ane, noBenefitEnrollmentCompletionCriteriaErrorMessage);
                return CreateHttpResponseException(noBenefitEnrollmentCompletionCriteriaErrorMessage, HttpStatusCode.BadRequest);
            }
            catch (ArgumentException ae)
            {
                logger.LogError(ae, invalidBenefitEnrollmentCompletionCriteriaErrorMessage);
                return CreateHttpResponseException(invalidBenefitEnrollmentCompletionCriteriaErrorMessage, HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException ex)
            {
                logger.LogError(ex, forbiddenSubmitOrReopenErrorMessage);
                return CreateHttpResponseException(forbiddenSubmitOrReopenErrorMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(benefitElectionsActionFailureMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets the beneficiary categories/types
        /// </summary>
        ///  <accessComments>
        /// Any authenticated user can retrieve beneficiary category information.
        /// </accessComments> 
        /// <returns>Returns a list of Beneficiary Category DTO objects</returns>
        /// <note>--Domain Entity-- is cached for 24 hours.</note>

        [HttpGet]
        [HeaderVersionRoute("/employees/beneficiary-category", 1, true, Name = "GetBeneficiaryCategoriesAsync")]
        public async Task<ActionResult<IEnumerable<BeneficiaryCategory>>> GetBeneficiaryCategoriesAsync()
        {
            try
            {
                logger.LogDebug("*******Start - Process to get the beneficiary categories/types - Start***********");
                var beneficiaryCategories = await benefitsEnrollmentService.GetBeneficiaryCategoriesAsync();
                logger.LogDebug("*******End - Process to get the beneficiary categories/types is successful - End***********");
                return Ok(beneficiaryCategories);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException(beneficiaryCategoryFailureMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get benefits enrollment acknowledgement report
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can download benefits acknowledgement report.      
        /// </accessComments>  
        /// <param name="employeeId"></param>
        /// <returns>The pdf report of enrolled benefits information</returns>
        [HttpGet]
        [HeaderVersionRoute("/employees/{employeeId}/benefits-enrollment-acknowledgement", 1, true, RouteConstants.EllucianPDFMediaTypeFormat, Name = "GetBenefitsEnrollmentAcknowledgementReport")]
        public async Task<IActionResult> GetBenefitsEnrollmentAcknowledgementReportAsync(string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                return CreateHttpResponseException("Employee id must be specified.");
            }

            try
            {
                logger.LogDebug("*******Start - Process to get the benefits enrollment acknowledgement report - Start***********");               
                
                var path = System.IO.Path.Join(hostEnvironment.ContentRootPath, "Reports/HumanResources/BenefitsEnrollmentAcknowledgement.frx");

                var resourceFilePath = System.IO.Path.Join(hostEnvironment.ContentRootPath, "Resources/HumanResources/BenefitsEnrollment.json");
                var renderedBytes = await benefitsEnrollmentService.GetBenefitsInformationForAcknowledgementReport(employeeId, path, resourceFilePath);
                logger.LogDebug(string.Format("*****Benefits information for acknowledgement report obtained successfully for {0}******", employeeId));
                var fileNameString = string.Format("Open Enrollment Benefits {0}", DateTime.Now.ToString("MMddyyyy HH:mm:ss"));

                var fileName = Regex.Replace(fileNameString, "[^a-zA-Z0-9_]", "_") + ".pdf";

                logger.LogDebug("*******End - Process to get the benefits enrollment acknowledgement report is successful - End***********");
                return File(renderedBytes, "application/pdf", fileName);
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Parameters are not valid. See log for details.", HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
            }
            catch (ApplicationException ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Benefits Enrollment Acknowledgement could not be generated. See log for details.");
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Invalid operation based on state of Benefits Enrollment resource. See log for details.", HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException(unexpectedGenericErrorMessage);
            }
        }
    }
}
