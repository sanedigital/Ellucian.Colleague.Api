// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using System.ComponentModel;
using System.Net;
using System.Net.Http;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Coordination.Finance;
using Ellucian.Colleague.Dtos.Finance.AccountActivity;
using Ellucian.Web.Http.Controllers;
using Microsoft.Extensions.Logging;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Http.ModelBinding;

namespace Ellucian.Colleague.Api.Controllers.Finance
{
    /// <summary>
    /// Provides access to get student financial account activity.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Finance)]
    [Metadata(ApiDescription = "Provides access to get student financial account activity", ApiDomain = "Finance")]

    public class AccountActivityController : BaseCompressedApiController
    {
        private readonly IAccountActivityService _service;
        private readonly ILogger _logger;

        /// <summary>
        /// AccountActivityController class constructor
        /// </summary>
        /// <param name="service">Service of type <see cref="IAccountActivityService">IAccountActivityService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor">Interface to action context accessor</param>
        /// <param name="apiSettings"></param>
        public AccountActivityController(IAccountActivityService service, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _service = service;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves the account period data for a student.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">Student ID</param>
        /// <returns>The student's <see cref="AccountActivityPeriods">account activity period</see> data</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        [HttpGet]
        [HeaderVersionRoute("/account-activity/admin/{studentId}", 1, true, Name = "GetAccountActivityPeriodsForStudent")]
        [HeaderVersionRoute("/account-activity/{studentId}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "EthosGetAccountActivityPeriodsForStudent", IsEthosEnabled = true, IsAdministrative = true, Order = -1000)]

        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        public ActionResult<AccountActivityPeriods> GetAccountActivityPeriodsForStudent(string studentId)
        {
            try
            {
                return _service.GetAccountActivityPeriodsForStudent(studentId);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                _logger.LogError(csee, csee.Message);
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                _logger.LogError(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Retrieves the terms for which a student has activity.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="guid">PERSON GUID</param>
        /// <returns>The student's <see cref="AccountActivityTerms">account activity terms</see> data</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        [HttpGet]
        [HeaderVersionRoute("/student-activity-terms/{guid}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "EthosGetAccountActivityTermsForStudent", IsEthosEnabled = true, IsAdministrative = true, Order = -1000)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        public async Task<ActionResult<AccountActivityTerms>> GetAccountActivityTermsForStudent(string guid)
        {
            try
            {
                return await _service.GetAccountActivityTermsForStudent(guid);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                _logger.LogError(csee, csee.Message);
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                _logger.LogError(peex, peex.Message);
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the account activity data for a student for a specified term.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="termId">Term ID</param>
        /// <param name="studentId">Student ID</param>
        /// <returns>The <see cref="DetailedAccountPeriod">detailed account period</see> data for the specified student and term.</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        /// <note>DetailedAccountPeriod is cached for 1 minute.</note>
        [Obsolete("Obsolete as of API version 1.8, use GetAccountActivityByTermForStudent2 instead")]      
        [HttpGet]
        [HeaderVersionRoute("/account-activity/term/admin/{studentId}", 1, false, Name = "GetAccountActivityByTermForStudent")]
        public ActionResult<DetailedAccountPeriod> GetAccountActivityByTermForStudent(string termId, string studentId)
        {
            try
            {
                return _service.GetAccountActivityByTermForStudent(termId, studentId);
            }
            catch (PermissionsException peex)
            {
                _logger.LogError(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Retrieves the account activity data for a student for a specified term.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="termId">Term ID</param>
        /// <param name="studentId">Student ID</param>
        /// <returns>The <see cref="DetailedAccountPeriod">detailed account period</see> for the specified student and term.</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        [HttpGet]
        [HeaderVersionRoute("/account-activity/term/admin/{studentId}", 2, true, Name = "GetAccountActivityByTermForStudent2")]
        [HeaderVersionRoute("/account-activity-term/{studentId}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "EthosGetAccountActivityByTermForStudent", IsEthosEnabled = true, IsAdministrative = true, Order = -1000)]

        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        public ActionResult<DetailedAccountPeriod> GetAccountActivityByTermForStudent2(string termId, string studentId)
        {
            try
            {
                return _service.GetAccountActivityByTermForStudent2(termId, studentId);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                _logger.LogError(csee, csee.Message);
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                _logger.LogError(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Retrieves the account activity data for a student for a specified period.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="arguments">The <see cref="AccountActivityPeriodArguments">AccountActivityPeriodArguments</see> for the desired period</param>
        /// <param name="studentId">Student ID</param>
        /// <returns>The <see cref="DetailedAccountPeriod">detailed account period</see> data for the specified student and period.</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        /// <note>DetailedAccountPeriod is cached for 1 minute.</note>
        [Obsolete("Obsolete as of API version 1.8, use PostAccountActivityByPeriodForStudent2 instead")]      
        [HttpPost]
        [HeaderVersionRoute("/account-activity/period/admin/{studentId}", 1, false, Name = "PostAccountActivityByPeriodForStudent")]
        public ActionResult<DetailedAccountPeriod> PostAccountActivityByPeriodForStudent(AccountActivityPeriodArguments arguments, [FromRoute]string studentId)
        {
            try
            {
                return _service.PostAccountActivityByPeriodForStudent(arguments.AssociatedPeriods, arguments.StartDate, arguments.EndDate, studentId);
            }
            catch (PermissionsException peex)
            {
                _logger.LogError(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Retrieves the account activity data for a student for a specified period.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="arguments">The <see cref="AccountActivityPeriodArguments">AccountActivityPeriodArguments</see> for the desired period</param>
        /// <param name="studentId">Student ID</param>
        /// <returns>The <see cref="DetailedAccountPeriod">Detailed Account Period</see> for the specified student and period.</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        [HttpPost]
        [HeaderVersionRoute("/account-activity/period/admin/{studentId}", 2, true, Name = "PostAccountActivityByPeriodForStudent2")]
        [HeaderVersionRoute("/account-activity-period/{studentId}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "EthosPostAccountActivityByPeriodForStudent", IsEthosEnabled = true, IsAdministrative = true, Order = -10000)]
        public ActionResult<DetailedAccountPeriod> PostAccountActivityByPeriodForStudent2([ModelBinder(typeof(EthosEnabledBinder))]AccountActivityPeriodArguments arguments, [FromRoute]string studentId)
        {
            try
            {
                return _service.PostAccountActivityByPeriodForStudent2(arguments.AssociatedPeriods, arguments.StartDate, arguments.EndDate, studentId);
            }
            catch (ColleagueSessionExpiredException csee)

            {
                _logger.LogError(csee, csee.Message);
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                _logger.LogError(peex.ToString());
                return CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Gets student award disbursement information for the specified award for the specified year
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">student id</param>
        /// <param name="awardYear">award year code</param>
        /// <param name="awardId">award id</param>
        /// <returns>StudentAwardDisbursementInfo DTO</returns>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/disbursements/{awardYear}/{awardId}", 1, false, "application/vnd.ellucian-student-finance-disbursements.v{0}+json", Name = "GetStudentAwardDisbursementInfoAsync")]
        [HeaderVersionRoute("/students/{studentId}/disbursements/{awardYear}/{awardId}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "EthosGetGetStudentAwardDisbursementInfoAsync", IsEthosEnabled =true, Order = -1000)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        public async Task<ActionResult<StudentAwardDisbursementInfo>> GetStudentAwardDisbursementInfoAsync([FromRoute]string studentId, [FromRoute] string awardYear, [FromRoute] string awardId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return CreateHttpResponseException("studentId is required");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                return CreateHttpResponseException("awardYearCode is required");
            }
            if (string.IsNullOrEmpty(awardId))
            {
                return CreateHttpResponseException("awardId is required");
            }
            try
            {
                return await _service.GetStudentAwardDisbursementInfoAsync(studentId, awardYear, awardId);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                _logger.LogError(csee, csee.Message);
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (ArgumentNullException ane)
            {
                _logger.LogError(ane, ane.Message);
                return CreateHttpResponseException("One of the provided arguments is invalid. See log for details");
            }
            catch(PermissionsException pe)
            {
                _logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Permission denied to retrieve disbursement data. See log for details", System.Net.HttpStatusCode.Forbidden);
            }
            catch(ApplicationException ae)
            {
                _logger.LogError(ae, ae.Message);
                return CreateHttpResponseException("Exception encountered while retrieving disbursement info. See log for details");
            }
            catch (KeyNotFoundException knfe)
            {
                _logger.LogError(knfe, knfe.Message);
                return CreateHttpResponseException("Could not locate requested disbursement data. See log for details", System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Unknown error occurred while retrieving disbursement info. See log for more details.");
            }
        }

        /// <summary>
        /// Returns information about potentially untransmitted D7 financial aid, based on
        /// current charges, credits, and awarded aid.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="criteria">The <see cref="PotentialD7FinancialAidCriteria"/> criteria of
        /// potential financial aid for which to search.</param>
        /// <returns>Enumeration of <see cref="Dtos.Finance.AccountActivity.PotentialD7FinancialAid"/> 
        /// awards and potential award amounts.</returns>
        /// 
        [HttpPost]
        [HeaderVersionRoute("/qapi/potential-d7-financial-aid", 1, true, Name = "QueryStudentPotentialD7FinancialAidAsync")]
        public async Task<ActionResult<IEnumerable<PotentialD7FinancialAid>>> QueryStudentPotentialD7FinancialAidAsync([FromBody]PotentialD7FinancialAidCriteria criteria)
        {
            if (criteria == null)
            {
                return CreateHttpResponseException("criteria cannot be null");
            }

            try
            {
                return Ok(await _service.GetPotentialD7FinancialAidAsync(criteria));
            }
            catch (ColleagueSessionExpiredException csee)
            {
                _logger.LogError(csee, csee.Message);
                return CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (ArgumentNullException ane)
            {
                _logger.LogError(ane, ane.Message);
                return CreateHttpResponseException("One of the provided arguments is invalid. See log for details");
            }
            catch (PermissionsException pe)
            {
                _logger.LogError(pe, pe.Message);
                return CreateHttpResponseException("Permission denied to retrieve finacial aid data. See log for details", System.Net.HttpStatusCode.Forbidden);
            }
            catch (ApplicationException ae)
            {
                _logger.LogError(ae, ae.Message);
                return CreateHttpResponseException("Exception encountered while retrieving financial aid info. See log for details");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Unknown error occurred while retrieving financial info. See log for more details.");
            }
        }
    }

}
