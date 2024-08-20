// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using System.Net.Http;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http;
using Ellucian.Web.Http.ModelBinding;


namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// The controller for student payments for the Ellucian Data Model.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    [Authorize]
    public class StudentPaymentsController : BaseCompressedApiController
    {
        private readonly IStudentPaymentService studentPaymentService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the StudentPaymentController object
        /// </summary>
        /// <param name="studentPaymentService">student payments service object</param>
        /// <param name="logger">Logger object</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public StudentPaymentsController(IStudentPaymentService studentPaymentService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.studentPaymentService = studentPaymentService;
            this.logger = logger;
        }

        #region Student payments V6

        /// <summary>
        /// Retrieves a specified student payment for the data model version 6
        /// </summary>
        /// <param name="id">The requested student payment GUID</param>
        /// <returns>A StudentPayment DTO</returns>
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/student-payments/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentPaymentsById", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentPayment>> GetByIdAsync([FromRoute] string id)
        {
            bool bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id", "id is required.");
                }

                var studentPayment = await studentPaymentService.GetByIdAsync(id);

                if (studentPayment != null)
                {

                    AddEthosContextProperties(await studentPaymentService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await studentPaymentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { studentPayment.Id }));
                }

                return studentPayment;
            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error getting student payment");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves all student payments for the data model version 6
        /// </summary>
        /// <returns>A Collection of StudentPayments</returns>
        [HttpGet]
        [ValidateQueryStringFilter(new string[] { "student", "academicPeriod", "accountingCode", "paymentType" }, false, true)]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/student-payments", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAllStudentPayments", IsEedmSupported = true)]
        public async Task<IActionResult> GetAsync(Paging page, [FromQuery] string student = "", string academicPeriod = "", string accountingCode = "", string paymentType = "")
        {

            string criteria = string.Concat(student, academicPeriod, accountingCode, paymentType);

            //valid query parameter but empty argument
            if ((!string.IsNullOrEmpty(criteria)) && (string.IsNullOrEmpty(criteria.Replace("\"", ""))))
            {
                return new PagedActionResult<IEnumerable<Dtos.StudentPayment>>(new List<Dtos.StudentPayment>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }

            if (student == null || academicPeriod == null || accountingCode == null || paymentType == null)
                // null vs. empty string means they entered a filter with no criteria and we should return an empty set.
                return new PagedActionResult<IEnumerable<Dtos.StudentPayment>>(new List<Dtos.StudentPayment>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            bool bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (page == null)
            {
                page = new Paging(200, 0);
            }
            try
            {

                var pageOfItems = await studentPaymentService.GetAsync(page.Offset, page.Limit, bypassCache, student, academicPeriod, accountingCode, paymentType);

                AddEthosContextProperties(await studentPaymentService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                  await studentPaymentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                  pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.StudentPayment>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error getting student payment");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Update a single student payment for the data model version 6
        /// </summary>
        /// <param name="id">The requested student payment GUID</param>
        /// <param name="studentPaymentDto">General Ledger DTO from Body of request</param>
        /// <returns>A single StudentPayment</returns>
        [HttpPut]
        [HeaderVersionRoute("/student-payments/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentPayments")]
        [HeaderVersionRoute("/student-payments/{id}", "11.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentPaymentsV11_1_0")]
        [HeaderVersionRoute("/student-payments/{id}", "11", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentPaymentsV11")]
        [HeaderVersionRoute("/student-payments/{id}", "16.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutStudentPaymentsV16_0_0")]
        public async Task<ActionResult<Dtos.StudentPayment>> UpdateAsync([FromRoute] string id, [FromBody] Dtos.StudentPayment studentPaymentDto)
        {
            // The code is in the service and repository to perform this function but at this time, we
            // are not allowing an update or a delete.  Just throw unsupported error instead.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Create a single student payment for the data model version 6
        /// </summary>
        /// <param name="studentPaymentDto">General Ledger DTO from Body of request</param>
        /// <returns>A single StudentPayment</returns>
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpPost]
        [HeaderVersionRoute("/student-payments", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentPayments", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentPayment>> CreateAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.StudentPayment studentPaymentDto)
        {
            try
            {
                if (studentPaymentDto == null)
                {
                    throw new ArgumentNullException("studentPaymentDto", "The request body is required.");
                }
                if (studentPaymentDto.Id != Guid.Empty.ToString())
                {
                    throw new ArgumentNullException("studentChargeDto", "On a post you can not define a GUID");
                }
                ValidateStudentPayments(studentPaymentDto);

                //call import extend method that needs the extracted extension data and the config
                await studentPaymentService.ImportExtendedEthosData(await ExtractExtendedData(await studentPaymentService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), logger));

                //create the student charge
                var studentPayment = await studentPaymentService.CreateAsync(studentPaymentDto);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await studentPaymentService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await studentPaymentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { studentPayment.Id }));

                return studentPayment;
            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error getting student payment");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Helper method to validate Student Payments.
        /// </summary>
        /// <param name="studentPayment">student payment DTO object of type <see cref="Dtos.StudentPayment"/></param>
        private void ValidateStudentPayments(Dtos.StudentPayment studentPayment)
        {
            if (studentPayment.AcademicPeriod == null)
            {
                throw new ArgumentNullException("studentPayments.academicPeriod", "The academic period is required when submitting a student payment. ");
            }
            if (studentPayment.AcademicPeriod != null && string.IsNullOrEmpty(studentPayment.AcademicPeriod.Id))
            {
                throw new ArgumentNullException("studentPayments.academicPeriod", "The academic period id is required when submitting a student payment. ");
            }
            if (studentPayment.Amount == null)
            {
                throw new ArgumentNullException("studentPayments.paymentAmount", "The payment amount cannot be null when submitting a student payment. ");
            }
            if (studentPayment.Amount != null && (studentPayment.Amount.Value == 0 || studentPayment.Amount.Value == null))
            {
                throw new ArgumentNullException("studentPayments.paymentAmount.value", "A student-payments in the amount of zero dollars is not permitted. ");
            }
            if (studentPayment.Amount != null && studentPayment.Amount.Currency != Dtos.EnumProperties.CurrencyCodes.USD && studentPayment.Amount.Currency != Dtos.EnumProperties.CurrencyCodes.CAD)
            {
                throw new ArgumentException("The currency code must be set to either 'USD' or 'CAD'. ", "studentPayments.amount.currency");
            }
            if (studentPayment.PaymentType == Dtos.EnumProperties.StudentPaymentTypes.notset)
            {
                throw new ArgumentException("The paymentType is either invalid or empty and is required when submitting a student payment. ", "studentPayments.paymentType");
            }
            if (studentPayment.Person == null || string.IsNullOrEmpty(studentPayment.Person.Id))
            {
                throw new ArgumentNullException("studentPayments.student.id", "The student id is required when submitting a student payment. ");
            }
            if (studentPayment.AccountingCode != null && string.IsNullOrEmpty(studentPayment.AccountingCode.Id))
            {
                throw new ArgumentException("The accountingCode requires an id when submitting student payments. ", "studentPayments.accountingCode.id");
            }
            if (studentPayment.PaymentType == Dtos.EnumProperties.StudentPaymentTypes.sponsor && studentPayment.AccountingCode == null)
            {
                throw new ArgumentNullException("studentPayments.accountingCode", "The accountingCode is required when submitting sponsor payments. ");
            }
        }

        #endregion

        #region Student payments V11

        /// <summary>
        /// Retrieves a specified student payment for the data model version 6
        /// </summary>
        /// <param name="id">The requested student payment GUID</param>
        /// <returns>A StudentPayment DTO</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/student-payments/{id}", "11.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentPaymentsByIdV11_1_0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentPayment2>> GetByIdAsync2([FromRoute] string id)
        {
            bool bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    var integrationApiException = new IntegrationApiException();
                    integrationApiException.AddError(new IntegrationApiError("Missing.Request.ID", "The request id is required."));
                    throw integrationApiException;
                }
                var studentPayment = await studentPaymentService.GetByIdAsync2(id);

                if (studentPayment != null)
                {

                    AddEthosContextProperties(await studentPaymentService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await studentPaymentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { studentPayment.Id }));
                }

                return studentPayment;
            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error getting student payment");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves all student payments for the data model version 11
        /// </summary>
        /// <returns>A Collection of StudentPayments</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentPayment2)), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/student-payments", "11.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAllStudentPaymentsV11_1_0", IsEedmSupported = true)]
        public async Task<IActionResult> GetAsync2(Paging page, QueryStringFilter criteria)
        {
            string student = "", academicPeriod = "", fundSource = "", fundDestination = "", paymentType = "", usage = "";

            try
            {
                bool bypassCache = false;
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }
                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                var rawFilterData = GetFilterObject<Dtos.StudentPayment2>(logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.StudentPayment2>>(new List<Dtos.StudentPayment2>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                if (rawFilterData != null)
                {
                    student = rawFilterData.Person != null ? rawFilterData.Person.Id : null;
                    academicPeriod = rawFilterData.AcademicPeriod != null ? rawFilterData.AcademicPeriod.Id : null;
                    fundSource = rawFilterData.FundingSource != null ? rawFilterData.FundingSource.Id : null;
                    fundDestination = rawFilterData.FundingDestination != null ? rawFilterData.FundingDestination.Id : null;
                    paymentType = rawFilterData.PaymentType.ToString();
                    if (paymentType == Dtos.EnumProperties.StudentPaymentTypes.notset.ToString())
                    {
                        paymentType = string.Empty;
                    }
                    if (rawFilterData.ReportingDetail != null && rawFilterData.ReportingDetail.Usage != Dtos.EnumProperties.StudentPaymentUsageTypes.notset)
                    {
                        usage = rawFilterData.ReportingDetail.Usage.ToString();
                    }
                }

                var pageOfItems = await studentPaymentService.GetAsync2(page.Offset, page.Limit, bypassCache, student, academicPeriod, fundSource, paymentType, fundDestination, usage);

                AddEthosContextProperties(await studentPaymentService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                           await studentPaymentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                           pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.StudentPayment2>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error getting student payment");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Create a single student payment 
        /// </summary>
        /// <param name="studentPaymentDto">StudentPayment2 DTO from Body of request</param>
        /// <returns>A single StudentPayment</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/student-payments", "11.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentPaymentsV11_1_0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentPayment2>> CreateAsync2([ModelBinder(typeof(EedmModelBinder))] Dtos.StudentPayment2 studentPaymentDto)
        {
            try
            {
                if (studentPaymentDto == null)
                {
                    var integrationApiException = new IntegrationApiException();
                    integrationApiException.AddError(new IntegrationApiError("Missing.Request.Body", "The request body is required."));
                    throw integrationApiException;
                }
                if (studentPaymentDto.Id != Guid.Empty.ToString())
                {
                    var integrationApiException = new IntegrationApiException();
                    integrationApiException.AddError(new IntegrationApiError("Cannot.Set.GUID", "Please use a nil GUID to create a new record."));
                    throw integrationApiException;
                }
                ValidateStudentPayments2(studentPaymentDto);

                //call import extend method that needs the extracted extension data and the config
                await studentPaymentService.ImportExtendedEthosData(await ExtractExtendedData(await studentPaymentService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), logger));

                //create the student payment
                var studentPayment = await studentPaymentService.CreateAsync2(studentPaymentDto);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await studentPaymentService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await studentPaymentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { studentPayment.Id }));

                return studentPayment;
            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error getting student payment");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Helper method to validate Student Payments.
        /// </summary>
        /// <param name="studentPayment">student payment DTO object of type <see cref="Dtos.StudentPayment2"/></param>
        private void ValidateStudentPayments2(Dtos.StudentPayment2 studentPayment)
        {
            var integrationApiException = new IntegrationApiException();
          

            if (studentPayment.AcademicPeriod == null)
            {
                integrationApiException.AddError(new IntegrationApiError("Missing.Required.Property", message: "The academic period is required when submitting a student payment (studentPayments.academicPeriod)."));
            }
            if (studentPayment.AcademicPeriod != null && string.IsNullOrEmpty(studentPayment.AcademicPeriod.Id))
            {
                integrationApiException.AddError(new IntegrationApiError("Missing.Required.Property", message: "The academic period id is required when submitting a student payment (studentPayments.academicPeriod.id)."));
            }
            if (studentPayment.Amount == null)
            {
                 integrationApiException.AddError(new IntegrationApiError("Validation.Exception", message: "The payment amount cannot be null when submitting a student payment (studentPayments.paymentAmount)."));
            }
            if (studentPayment.Amount != null && (studentPayment.Amount.Value == 0 || studentPayment.Amount.Value == null))
            {
               integrationApiException.AddError(new IntegrationApiError("Validation.Exception", message: "A student-payments in the amount of zero dollars is not permitted (studentPayments.paymentAmount.value)."));
            }
            if (studentPayment.Amount != null && studentPayment.Amount.Currency != Dtos.EnumProperties.CurrencyCodes.USD && studentPayment.Amount.Currency != Dtos.EnumProperties.CurrencyCodes.CAD)
            {
                integrationApiException.AddError(new IntegrationApiError("Validation.Exception", message: "The currency code must be set to either 'USD' or 'CAD' (studentPayments.amount.currency)."));
            }
            if (studentPayment.PaymentType == Dtos.EnumProperties.StudentPaymentTypes.notset)
            {
                integrationApiException.AddError(new IntegrationApiError("Validation.Exception", message: "The paymentType is either invalid or empty and is required when submitting a student payment (studentPayments.paymentType)."));
            }
            if (studentPayment.Person == null || string.IsNullOrEmpty(studentPayment.Person.Id))
            {
                integrationApiException.AddError(new IntegrationApiError("Missing.Required.Property", message: "The student id is required when submitting a student payment (studentPayments.student.id)."));
            }
            if (studentPayment.FundingSource != null && string.IsNullOrEmpty(studentPayment.FundingSource.Id))
            {
                integrationApiException.AddError(new IntegrationApiError("Missing.Required.Property", message: "The fundingSource requires an id when submitting student payments (studentPayments.fundingSource.id)."));
            }
            //if (studentPayment.PaymentType == Dtos.EnumProperties.StudentPaymentTypes.sponsor && studentPayment.FundingDestination == null)
            //{
            //    throw new ArgumentNullException("studentPayments.fundingDestination", "The fundingDestination is required when submitting sponsor payments. ");
            //}
            //if (studentPayment.GlPosting == Dtos.EnumProperties.GlPosting.NotSet)
            //{
            //    throw new ArgumentNullException("studentPayments.generalLedgerPosting", "The generalLedgerPosting is required when submitting a sponsor payment.");
            //}
            if (integrationApiException.Errors != null && integrationApiException.Errors.Any())
            {
                throw integrationApiException;
            }

        }

        #endregion

        #region Student payments V16.0.0

        /// <summary>
        /// Retrieves a specified student payment for the data model version 6
        /// </summary>
        /// <param name="id">The requested student payment GUID</param>
        /// <returns>A StudentPayment DTO</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/student-payments/{id}", "16.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetStudentPaymentsByIdDefault", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentPayment3>> GetByIdAsync3([FromRoute] string id)
        {
            bool bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    var integrationApiException = new IntegrationApiException();
                    integrationApiException.AddError(new IntegrationApiError("Missing.Request.ID", "The request id is required."));
                    throw integrationApiException;
                }
                var studentPayment = await studentPaymentService.GetByIdAsync3(id);

                if (studentPayment != null)
                {

                    AddEthosContextProperties(await studentPaymentService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await studentPaymentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { studentPayment.Id }));
                }

                return studentPayment;
            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error getting student payment");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves all student payments for the data model version 11
        /// </summary>
        /// <returns>A Collection of StudentPayments</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentPayment3)), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [HeaderVersionRoute("/student-payments", "16.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetAllStudentPaymentDefault", IsEedmSupported = true)]
        public async Task<IActionResult> GetAsync3(Paging page, QueryStringFilter criteria)
        {
            string student = "", academicPeriod = "", fundSource = "", fundDestination = "", paymentType = "", usage = "";

            try
            {
                bool bypassCache = false;
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }
                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                var rawFilterData = GetFilterObject<Dtos.StudentPayment3>(logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedActionResult<IEnumerable<Dtos.StudentPayment3>>(new List<Dtos.StudentPayment3>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

                if (rawFilterData != null)
                {
                    student = rawFilterData.Person != null ? rawFilterData.Person.Id : null;
                    academicPeriod = rawFilterData.AcademicPeriod != null ? rawFilterData.AcademicPeriod.Id : null;
                    fundSource = rawFilterData.FundingSource != null ? rawFilterData.FundingSource.Id : null;
                    fundDestination = rawFilterData.FundingDestination != null ? rawFilterData.FundingDestination.Id : null;
                    paymentType = rawFilterData.PaymentType.ToString();
                    if (paymentType == Dtos.EnumProperties.StudentPaymentTypes.notset.ToString())
                    {
                        paymentType = string.Empty;
                    }
                    if (rawFilterData.ReportingDetail != null && rawFilterData.ReportingDetail.Usage != Dtos.EnumProperties.StudentPaymentUsageTypes.notset)
                    {
                        usage = rawFilterData.ReportingDetail.Usage.ToString();
                    }
                }

                var pageOfItems = await studentPaymentService.GetAsync3(page.Offset, page.Limit, bypassCache, student, academicPeriod, fundSource, paymentType, fundDestination, usage);

                AddEthosContextProperties(await studentPaymentService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                           await studentPaymentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                           pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.StudentPayment3>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error getting student payment");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Create a single student payment 
        /// </summary>
        /// <param name="studentPaymentDto">StudentPayment2 DTO from Body of request</param>
        /// <returns>A single StudentPayment</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter))]
        [HttpPost]
        [HeaderVersionRoute("/student-payments", "16.0.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostStudentPaymentsV16_0_0", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.StudentPayment3>> CreateAsync3([ModelBinder(typeof(EedmModelBinder))] Dtos.StudentPayment3 studentPaymentDto)
        {
            try
            {
                if (studentPaymentDto == null)
                {
                    var integrationApiException = new IntegrationApiException();
                    integrationApiException.AddError(new IntegrationApiError("Missing.Request.Body", "The request body is required."));
                    throw integrationApiException;
                }
                if (studentPaymentDto.Id != Guid.Empty.ToString())
                {
                    var integrationApiException = new IntegrationApiException();
                    integrationApiException.AddError(new IntegrationApiError("Cannot.Set.GUID", "Please use a nil GUID to create a new record."));
                    throw integrationApiException;
                }
                ValidateStudentPayments3(studentPaymentDto);

                //call import extend method that needs the extracted extension data and the config
                await studentPaymentService.ImportExtendedEthosData(await ExtractExtendedData(await studentPaymentService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), logger));

                //create the student payment
                var studentPayment = await studentPaymentService.CreateAsync3(studentPaymentDto);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await studentPaymentService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await studentPaymentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { studentPayment.Id }));

                return studentPayment;
            }
            catch (PermissionsException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unknown error getting student payment");
                return CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Helper method to validate Student Payments.
        /// </summary>
        /// <param name="studentPayment">student payment DTO object of type <see cref="Dtos.StudentPayment3"/></param>
        private void ValidateStudentPayments3(Dtos.StudentPayment3 studentPayment)
        {
            var integrationApiException = new IntegrationApiException();


            if (studentPayment.AcademicPeriod == null)
            {
                integrationApiException.AddError(new IntegrationApiError("Missing.Required.Property", message: "The academic period is required when submitting a student payment (studentPayments.academicPeriod)."));
            }
            if (studentPayment.AcademicPeriod != null && string.IsNullOrEmpty(studentPayment.AcademicPeriod.Id))
            {
                integrationApiException.AddError(new IntegrationApiError("Missing.Required.Property", message: "The academic period id is required when submitting a student payment (studentPayments.academicPeriod.id)."));
            }
            if (studentPayment.Amount == null)
            {
                integrationApiException.AddError(new IntegrationApiError("Validation.Exception", message: "The payment amount cannot be null when submitting a student payment (studentPayments.paymentAmount)."));
            }
            if (studentPayment.Amount != null && (studentPayment.Amount.Value == 0 || studentPayment.Amount.Value == null))
            {
                integrationApiException.AddError(new IntegrationApiError("Validation.Exception", message: "A student-payments in the amount of zero dollars is not permitted (studentPayments.paymentAmount.value)."));
            }
            if (studentPayment.Amount != null && studentPayment.Amount.Currency != Dtos.EnumProperties.CurrencyCodes.USD && studentPayment.Amount.Currency != Dtos.EnumProperties.CurrencyCodes.CAD)
            {
                integrationApiException.AddError(new IntegrationApiError("Validation.Exception", message: "The currency code must be set to either 'USD' or 'CAD' (studentPayments.amount.currency)."));
            }
            if (studentPayment.PaymentType == Dtos.EnumProperties.StudentPaymentTypes2.notset)
            {
                integrationApiException.AddError(new IntegrationApiError("Validation.Exception", message: "The paymentType is either invalid or empty and is required when submitting a student payment (studentPayments.paymentType)."));
            }
            if (studentPayment.Person == null || string.IsNullOrEmpty(studentPayment.Person.Id))
            {
                integrationApiException.AddError(new IntegrationApiError("Missing.Required.Property", message: "The student id is required when submitting a student payment (studentPayments.student.id)."));
            }
            if (studentPayment.FundingSource != null && string.IsNullOrEmpty(studentPayment.FundingSource.Id))
            {
                integrationApiException.AddError(new IntegrationApiError("Missing.Required.Property", message: "The fundingSource requires an id when submitting student payments (studentPayments.fundingSource.id)."));
            }
            //if (studentPayment.PaymentType == Dtos.EnumProperties.StudentPaymentTypes.sponsor && studentPayment.FundingDestination == null)
            //{
            //    throw new ArgumentNullException("studentPayments.fundingDestination", "The fundingDestination is required when submitting sponsor payments. ");
            //}
            //if (studentPayment.GlPosting == Dtos.EnumProperties.GlPosting.NotSet)
            //{
            //    throw new ArgumentNullException("studentPayments.generalLedgerPosting", "The generalLedgerPosting is required when submitting a sponsor payment.");
            //}
            if (integrationApiException.Errors != null && integrationApiException.Errors.Any())
            {
                throw integrationApiException;
            }

        }

        #endregion

        /// <summary>
        /// Delete a single student payment for the data model version 6
        /// </summary>
        /// <param name="id">The requested student payment GUID</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("/student-payments/{id}", Name = "DeleteStudentPayments", Order = -10)]
        public async Task<ActionResult> DeleteAsync([FromRoute] string id)
        {
            // The code is in the service and repository to perform this function but at this time, we
            // are not allowing an update or a delete.  Just throw unsupported error instead.
            return CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}
