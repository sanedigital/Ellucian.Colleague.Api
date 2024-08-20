// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.Finance;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Finance.AccountDue;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using System.ComponentModel;
using System.Net;

namespace Ellucian.Colleague.Api.Controllers.Finance
{
    /// <summary>
    /// Provides access to get student financial account due information.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Finance)]
    [Metadata(ApiDescription = "Provides access to data for accounts due", ApiDomain = "Finance")]
    public class AccountDueController : BaseCompressedApiController
    {
        private readonly IAccountDueService _service;
        private readonly ILogger _logger;

        /// <summary>
        /// AccountDueController class constructor
        /// </summary>
        /// <param name="service">Service of type <see cref="IAccountDueService">IAccountDueService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public AccountDueController(IAccountDueService service, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _service = service;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves the account due data for a student broken out by term.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">Student ID</param>
        /// <returns>The student's <see cref="AccountDue">account due</see> data</returns>
        [HttpGet]
        [HeaderVersionRoute("/account-due/term/admin/{studentId}", 1, true, Name = "GetPaymentsDueByTermForStudent")]
        [HeaderVersionRoute("/student-account-due-term/{studentId}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, IsEthosEnabled = true, Name = "EthosGetPaymentsDueByTermForStudent", IsAdministrative = true, Order = -1000)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<AccountDue> GetAccountDueForStudent(string studentId)
        {
            try
            {
                return Ok(_service.GetAccountDue(studentId));
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
        /// Retrieves the account due data for a student broken out by period.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">Student ID</param>
        /// <returns>The student's <see cref="AccountDuePeriod">account due period</see> data</returns>
        [HttpGet]
        [HeaderVersionRoute("/account-due/period/admin/{studentId}", 1, true, Name = "GetPaymentsDueByPeriodForStudent")]
        [HeaderVersionRoute("/student-account-due-period/{studentId}", "1.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, IsEthosEnabled = true, Name = "GetAccountDuePeriodForStudent", Order = -1000)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<AccountDuePeriod> GetAccountDuePeriodForStudent(string studentId)
        {
            try
            {
                return Ok(_service.GetAccountDuePeriod(studentId));
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


    }
}
