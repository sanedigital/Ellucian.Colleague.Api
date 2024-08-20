// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Dtos.Student.InstantEnrollment;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Instant Enrollment 
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class InstantEnrollmentController : BaseCompressedApiController
    {
        private readonly IInstantEnrollmentService _instantEnrollmentService;
        private readonly ICourseService _courseService;
        private readonly ILogger _logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initializes a new instance of the InstantEnrollmentController class.
        /// </summary>
        /// <param name="courseService">Service of type <see cref="ICourseService">ICourseService</see></param>
        /// <param name="service">Service of type <see cref="IInstantEnrollmentService">IInstantEnrollmentService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor">Interface to action context accessor</param>
        /// <param name="apiSettings"></param>
        public InstantEnrollmentController(ICourseService courseService, IInstantEnrollmentService service, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _instantEnrollmentService = service;
            _courseService = courseService;
            this._logger = logger;
        }

        /// <summary>
        /// Performs a search of sections that are available for Instant Enrollment only.
        /// The criteria supplied is keyword and various filters which may be used to search and narrow list of sections.
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        /// <accessComments>Any authenticated user can perform search on sections and view Instant Enrollment catalog.</accessComments>
        /// <note>Instant Enrollment course section data is cached for 24 hours.</note>
        [HttpPost]
        [HeaderVersionRoute("/instant-enrollment/sections/search", 2, true, RouteConstants.EllucianInstantEnrollmentFormat, Name = "InstantEnrollmentCourseSearch2")]
        public async Task<ActionResult<SectionPage2>> PostInstantEnrollmentCourseSearch2Async([FromBody] InstantEnrollmentCourseSearchCriteria criteria, int pageSize = 10, int pageIndex = 0)
        {
            try
            {
                // Logging the timings for monitoring
                _logger.LogInformation("Call Course Search Service from Courses controller... ");
                var watch = new Stopwatch();
                watch.Start();
                SectionPage2 sectionPage = await _courseService.InstantEnrollmentSearch2Async(criteria, pageSize, pageIndex);
                watch.Stop();
                _logger.LogInformation("Course search returned in: " + watch.ElapsedMilliseconds.ToString());
                return sectionPage;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                string message = "Session has expired while retrieving Colleague Self-Service instant enrollment course search data";
                _logger.LogError(csse, message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString() + ex.StackTrace);
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// This is to mock registration for classes and return sections registered with the associated cost.
        /// </summary>
        /// <param name="proposedRegistration">proposed registration of type <see cref="InstantEnrollmentProposedRegistration">InstantEnrollmentProposedRegistration</see></param>
        /// <accessComments>Any authenticated user can complete a proposed registration and retrieve costs for the registered sections for themselves. 
        /// Additionally, users with the IE.ALLOW.ALL permission can can complete a proposed registration and retrieve costs for the registered sections</accessComments>
        [HttpPost]
        [HeaderVersionRoute("/instant-enrollment/proposed-registration", 1, true, RouteConstants.EllucianInstantEnrollmentFormat, Name = "InstantEnrollmentProposedRegistration")]
        public async Task<ActionResult<InstantEnrollmentProposedRegistrationResult>> PostProposedRegistrationForClassesAsync([FromBody] InstantEnrollmentProposedRegistration proposedRegistration)
        {
            try
            {
                if (proposedRegistration == null)
                {
                    throw new ArgumentNullException("proposedRegistration", "proposed registration information is required in order to complete mock registrations and retrieve cost for sections for instant enrollment");

                }
                if (proposedRegistration.ProposedSections == null || proposedRegistration.ProposedSections.Count == 0)
                {
                    throw new ArgumentException("ProposedSections", "proposed registration should have proposed sections in order to mock registration and retrieve associated cost for Instant Enrollment");
                }
                InstantEnrollmentProposedRegistrationResult proposedResult = await _instantEnrollmentService.ProposedRegistrationForClassesAsync(proposedRegistration);
                return proposedResult;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                string message = "Session has expired while attempting to complete the mock registration for classes selected for instant enrollment";
                _logger.LogError(csse, message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                string exceptionMsg = "User is not permitted to complete the mock registration for classes selected for instant enrollment";
                _logger.LogError(pex, exceptionMsg);
                return CreateHttpResponseException(exceptionMsg, HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Proposed registration argument was not provided in order to complete proposed registration for classes selected for instant enrollment", HttpStatusCode.BadRequest);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("An invalid argument was supplied for proposed registrations.", HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Couldn't complete the mock registration for classes selected for instant enrollment", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// This is to register for classes and return sections registered when the total cost is zero.
        /// </summary>
        /// <param name="zeroCostRegistration">zero cost registration of type <see cref="InstantEnrollmentZeroCostRegistration">InstantEnrollmentZeroCostRegistration</see></param>
        /// <accessComments>Any authenticated user can register for classes for themselves when the total cost is zero.
        /// Additionally, users with the IE.ALLOW.ALL permission can register for classes when the total cost is zero.</accessComments>
        [HttpPost]
        [HeaderVersionRoute("/instant-enrollment/zero-cost-registration", 1, true, RouteConstants.EllucianInstantEnrollmentFormat, Name = "InstantEnrollmentZeroCostRegistration")]
        public async Task<ActionResult<InstantEnrollmentZeroCostRegistrationResult>> PostZeroCostRegistrationForClassesAsync([FromBody] InstantEnrollmentZeroCostRegistration zeroCostRegistration)
        {
            try
            {
                if (zeroCostRegistration == null)
                {
                    throw new ArgumentNullException("zeroCostRegistration", "registration information is required in order to complete a zero cost registration for instant enrollment.");
                }

                if (zeroCostRegistration.ProposedSections == null || zeroCostRegistration.ProposedSections.Count == 0)
                {
                    throw new ArgumentException("zeroCostRegistration.ProposedSections", "at least one proposed section is required in order to complete a zero cost registration for instant enrollment.");
                }

                var zeroCostRegistrationResult = await _instantEnrollmentService.ZeroCostRegistrationForClassesAsync(zeroCostRegistration);
                return zeroCostRegistrationResult;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                string message = "Session has expired while attempting to complete the zero cost registration for classes selected for instant enrollment";
                _logger.LogError(csse, message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                string exceptionMsg = "User is not permitted to complete the zero cost registration for classes selected for instant enrollment";
                _logger.LogError(pex, exceptionMsg);
                return CreateHttpResponseException(exceptionMsg, HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("A required zero cost registration argument was not provided to register for classes for instant enrollment.", HttpStatusCode.BadRequest);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("An invalid argument was supplied for the zero cost registration.", HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Could not complete the zero cost registration for selected classes for instant enrollment.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// This is to register and pay for classes with an electronic transfer.
        /// </summary>
        /// <param name="echeckRegistration">echeck registration of type <see cref="InstantEnrollmentEcheckRegistration">InstantEnrollmentEcheckRegistration</see></param>
        /// <accessComments>Any authenticated user can register for classes for themselves and pay the costs for the registered sections using an electronic check.
        /// Additionally, users with the IE.ALLOW.ALL permission can register for classes and pay the costs for the registered sections using an electronic check.</accessComments>
        [HttpPost]
        [HeaderVersionRoute("/instant-enrollment/echeck-registration", 1, true, RouteConstants.EllucianInstantEnrollmentFormat, Name = "InstantEnrollmentEcheckRegistration")]
        public async Task<ActionResult<InstantEnrollmentEcheckRegistrationResult>> PostEcheckRegistrationForClassesAsync([FromBody] InstantEnrollmentEcheckRegistration echeckRegistration)
        {
            try
            {
                if (echeckRegistration == null)
                {
                    throw new ArgumentNullException("echeckRegistration", "echeck registration information is required in order to complete registrations and pay the costs for sections for instant enrollment");

                }
                if (echeckRegistration.ProposedSections == null || echeckRegistration.ProposedSections.Count == 0)
                {
                    throw new ArgumentException("ProposedSections", "echeck registration should have proposed sections in order to complete registrations and pay the costs for sections for Instant Enrollment");
                }
                InstantEnrollmentEcheckRegistrationResult echeckResult = await _instantEnrollmentService.EcheckRegistrationForClassesAsync(echeckRegistration);
                return echeckResult;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                string message = "Session has expired while attempting to complete the echeck registration for classes selected for instant enrollment";
                _logger.LogError(csse, message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                string exceptionMsg = "User is not permitted to complete the echeck registration for classes selected for instant enrollment";
                _logger.LogError(pex, exceptionMsg);
                return CreateHttpResponseException(exceptionMsg, HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Echeck registration argument was not provided in order to complete registration for classes selected for instant enrollment", HttpStatusCode.BadRequest);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("An invalid argument was supplied", HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Couldn't complete the echeck registration for classes selected for instant enrollment", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Start an instant enrollment payment gateway transaction, which includes registering the student and creating the student if needed.
        /// </summary>
        /// <param name="proposedRegistration">A <see cref="InstantEnrollmentPaymentGatewayRegistration">InstantEnrollmentProposedRegistration</see>containing the information needed to start the payment gateway transaction.</param>
        /// <returns>A <see cref="InstantEnrollmentStartPaymentGatewayRegistrationResult">InstantEnrollmentStartPaymentGatewayRegistrationResult</see> containing the result of the operation.</returns>
        /// <accessComments>Any authenticated user can register for classes for themselves and pay the costs for the registered sections using a credit card.
        /// Additionally, users with the IE.ALLOW.ALL permission can register for classes and pay the costs for the registered sections using a credit card.</accessComments>
        [HttpPost]
        [HeaderVersionRoute("/instant-enrollment/start-payment-gateway-transaction", 1, true, RouteConstants.EllucianInstantEnrollmentFormat, Name = "InstantEnrollmentStartPaymentGatewayTransaction")]
        public async Task<ActionResult<InstantEnrollmentStartPaymentGatewayRegistrationResult>> PostStartInstantEnrollmentPaymentGatewayTransaction([FromBody] InstantEnrollmentPaymentGatewayRegistration proposedRegistration)
        {
            try
            {
                if (proposedRegistration == null)
                {
                    throw new ArgumentNullException("proposedRegistration");

                }
                if (proposedRegistration.ProposedSections == null || proposedRegistration.ProposedSections.Count == 0)
                {
                    throw new ArgumentException("ProposedSections", "proposed registration must have proposed sections");
                }
                InstantEnrollmentStartPaymentGatewayRegistrationResult proposedResult = await _instantEnrollmentService.StartInstantEnrollmentPaymentGatewayTransaction(proposedRegistration);
                return proposedResult;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                string message = "Session has expired while attempting to start the payment gateway instant enrollment registration";
                _logger.LogError(csse, message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                string exceptionMsg = "User is not permitted to start the payment gateway instant enrollment registration";
                _logger.LogError(pex, exceptionMsg);
                return CreateHttpResponseException(exceptionMsg, HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("A required argument was not provided.", HttpStatusCode.BadRequest);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("An invalid argument was supplied.", HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Unable to start the payment gateway instant enrollment registration", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves instant enrollment payment acknowledgement paragraph text for a given <see cref="InstantEnrollmentPaymentAcknowledgementParagraphRequest"/>
        /// </summary>
        /// <param name="request">A <see cref="InstantEnrollmentPaymentAcknowledgementParagraphRequest"/></param>
        /// <returns>Instant enrollment payment acknowledgement paragraph text</returns>
        /// <accessComments>Any authenticated user retrieve instant enrollment payment acknowledgement paragraph text for themselves.
        /// Additionally, any user with the IE.ALLOW.ALL permission can retrieve instant enrollment payment acknowledgement paragraph text</accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/instant-enrollment/payment-acknowledgement-paragraph-text", 1, true, RouteConstants.EllucianInstantEnrollmentFormat, Name = "GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync")]
        public async Task<ActionResult<IEnumerable<string>>> GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync([FromBody] InstantEnrollmentPaymentAcknowledgementParagraphRequest request)
        {
            if (request == null)
            {
                var nullRequestMsg = "An instant enrollment payment acknowledgement paragraph request is required to get instant enrollment payment acknowledgement paragraph text.";
                return CreateHttpResponseException(nullRequestMsg, HttpStatusCode.BadRequest);
            }
            try
            {
                return Ok(await _instantEnrollmentService.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(request));
            }
            catch (ColleagueSessionExpiredException csse)
            {
                string message = "Session has expired while attempting to retrieve instant enrollment payment acknowledgement paragraph";
                _logger.LogError(csse, message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                string exceptionMsg = string.Format("User is not permitted to retrieve instant enrollment payment acknowledgement paragraph text for person {0}", request.PersonId);
                _logger.LogError(pex, exceptionMsg);
                return CreateHttpResponseException(exceptionMsg, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                string exceptionMsg = string.Format("An error occurred while attempting to retrieve instant enrollment payment acknowledgement paragraph text for person {0}", request.PersonId);
                if (!string.IsNullOrEmpty(request.CashReceiptId))
                {
                    exceptionMsg += string.Format(" for cash receipt {0}", request.CashReceiptId);
                }
                _logger.LogError(ex, exceptionMsg);
                return CreateHttpResponseException(exceptionMsg, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Query persons matching the criteria using the ELF duplicate checking criteria configured for Instant Enrollment.
        /// </summary>
        /// <param name="criteria">The <see cref="PersonMatchCriteriaInstantEnrollment">person attributes for which matching persons in Colleague are searched.</see> </param>
        /// <returns>Result of a person biographic/demographic matching inquiry for Instant Enrollment</returns>
        /// <accessComments>Users must have the IE.ALLOW.ALL permission to query person matches for Instant Enrollment</accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/persons", 1, false, RouteConstants.EllucianInstantEnrollmentFormat, Name = "GetPersonMatchingResultsInstantEnrollment")]
        public async Task<ActionResult<InstantEnrollmentPersonMatchResult>> QueryPersonMatchResultsInstantEnrollmentByPostAsync([FromBody] PersonMatchCriteriaInstantEnrollment criteria)
        {
            try
            {
                return await _instantEnrollmentService.QueryPersonMatchResultsInstantEnrollmentByPostAsync(criteria);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                string message = "Session has expired while attempting to query person matches for instant enrollment";
                _logger.LogError(csse, message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                string exceptionMsg = "User is not permitted to query person matches for instant enrollment.";
                _logger.LogError(pex, exceptionMsg);
                return CreateHttpResponseException(exceptionMsg, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves instant enrollment cash receipt acknowledgement for a given <see cref="InstantEnrollmentCashReceiptAcknowledgementRequest"/>
        /// </summary>
        /// <param name="request">A <see cref="InstantEnrollmentCashReceiptAcknowledgementRequest"/></param>
        /// <returns>Instant enrollment cash receipt acknowledgement</returns>
        /// <accessComments>Any authenticated user can retrieve instant enrollment cash receipt acknowledgement data for themselves.
        /// Additionally, any user with the IE.ALLOW.ALL permission can retrieve cash receipt acknowledgement data</accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/instant-enrollment/cash-receipt-acknowledgement", 1, true, RouteConstants.EllucianInstantEnrollmentFormat, Name = "GetInstantEnrollmentCashReceiptAcknowledgementAsync")]
        public async Task<ActionResult<InstantEnrollmentCashReceiptAcknowledgement>> GetInstantEnrollmentCashReceiptAcknowledgementAsync([FromBody] InstantEnrollmentCashReceiptAcknowledgementRequest request)
        {
            if (request == null)
            {
                var nullRequestMsg = "An instant enrollment cash receipt acknowledgement request is required to get instant enrollment cash receipt acknowledgement.";
                return CreateHttpResponseException(nullRequestMsg, HttpStatusCode.BadRequest);
            }
            try
            {
                return await _instantEnrollmentService.GetInstantEnrollmentCashReceiptAcknowledgementAsync(request);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                string message = "Session has expired while attempting to retrieve instant enrollment cash receipt acknowledgement";
                _logger.LogError(csse, message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                string exceptionMsg = "User is not permitted to retrieve instant enrollment cash receipt acknowledgement";
                if (!string.IsNullOrEmpty(request.TransactionId))
                {
                    exceptionMsg += string.Format(" for e-commerce transaction id {0}", request.TransactionId);
                }
                if (!string.IsNullOrEmpty(request.CashReceiptId))
                {
                    exceptionMsg += string.Format(" for cash receipt id {0}", request.CashReceiptId);
                }
                _logger.LogError(pex, exceptionMsg);
                return CreateHttpResponseException(exceptionMsg, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                string exceptionMsg = "An error occurred while attempting to retrieve an instant enrollment cash receipt acknowledgement";
                if (!string.IsNullOrEmpty(request.TransactionId))
                {
                    exceptionMsg += string.Format(" for e-commerce transaction id {0}", request.TransactionId);
                }
                if (!string.IsNullOrEmpty(request.CashReceiptId))
                {
                    exceptionMsg += string.Format(" for cash receipt id {0}", request.CashReceiptId);
                }
                _logger.LogError(ex, exceptionMsg);
                return CreateHttpResponseException(exceptionMsg, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets information the programs in which the specified student is enrolled.
        /// </summary>
        /// <param name="studentId">Student's ID</param>
        /// <param name="currentOnly">Boolean to indicate whether this request is for active student programs, or ended/past programs as well</param>
        /// <returns>All <see cref="Dtos.Student.StudentProgram2">Programs</see> in which the specified student is enrolled.</returns>
        /// <accessComments>
        /// Student information can be retrieved only if:
        /// 1. A Student is accessing its own data.
        /// 2. A user with permission of IE.ALLOW.ALL is accessing the student's data.
        /// </accessComments>
        /// <note>Student Program is cached for 5 minutes.</note>
        [HttpGet]
        [HeaderVersionRoute("/students/{studentId}/programs", 1, false, RouteConstants.EllucianInstantEnrollmentFormat, Name = "GetInstantEnrollmentStudentPrograms2Async")]
        public async Task<ActionResult<IEnumerable<Dtos.Student.StudentProgram2>>> GetInstantEnrollmentStudentPrograms2Async(string studentId, bool currentOnly = true)
        {
            if (string.IsNullOrWhiteSpace(studentId))
            {
                var nullRequestMsg = "An instant enrollment student id is required to retrieve student programs.";
                return CreateHttpResponseException(nullRequestMsg, HttpStatusCode.BadRequest);
            }

            try
            {
                return Ok(await _instantEnrollmentService.GetInstantEnrollmentStudentPrograms2Async(studentId, currentOnly));
            }
            catch (ColleagueSessionExpiredException csse)
            {
                string message = "Session has expired while attempting to retrieve student programs";
                _logger.LogError(csse, message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                string exceptionMsg = string.Format("User is not permitted to retrieve student programs for person {0}", studentId);
                _logger.LogError(pex, exceptionMsg);
                return CreateHttpResponseException(exceptionMsg, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                string exceptionMsg = string.Format("An error occurred while attempting to retrieve student programs for person {0}", studentId);
                _logger.LogError(ex, exceptionMsg);
                return CreateHttpResponseException(exceptionMsg, HttpStatusCode.BadRequest);
            }
        }
    }
}
