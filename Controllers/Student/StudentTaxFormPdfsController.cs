// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.

using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using Ellucian.Colleague.Coordination.Base.Services;
using System.Linq;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Colleague.Api;
using Microsoft.AspNetCore.Hosting;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// This is the controller for tax form pdf.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentTaxFormPdfsController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;
        private readonly IStudentTaxFormPdfService taxFormPdfService;
        private readonly ITaxFormConsentService taxFormConsentService;
        private readonly IConfigurationService configurationService;
        private readonly IWebHostEnvironment webHostEnvironment;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initialize the Tax Form pdf controller.
        /// </summary>
        public StudentTaxFormPdfsController(IAdapterRegistry adapterRegistry, ILogger logger, IStudentTaxFormPdfService taxFormPdfService, ITaxFormConsentService taxFormConsentService, IConfigurationService configurationService, IActionContextAccessor actionContextAccessor, IWebHostEnvironment webHostEnvironment, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
            this.taxFormPdfService = taxFormPdfService;
            this.taxFormConsentService = taxFormConsentService;
            this.configurationService = configurationService;
            this.webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Returns the data to be printed on the pdf for the 1098 tax form.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the 1098.</param>
        /// <param name="recordId">The record ID where the 1098 pdf data is stored</param>
        /// <accessComments>
        /// Requires permission VIEW.1098 for the student.
        /// Requires permission VIEW.1098 for someone who currently has permission to proxy for the student requested.
        /// Requires permission VIEW.STUDENT.1098 for admin view.
        /// The tax form record requested must belong to the person ID requested.
        /// </accessComments>
        /// <returns>An HttpResponseMessage containing a byte array representing a PDF.</returns>
        [HttpGet]
        [HeaderVersionRoute("/persons/{personId}/form1098ts/{recordId}", 2, true, RouteConstants.EllucianPDFMediaTypeFormat, Name = "Get1098TaxFormPdf3")]
        public async Task<IActionResult> Get1098TaxFormPdf2Async(string personId, string recordId)
        {
            try
            {
                if (string.IsNullOrEmpty(personId))
                    return CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);
                if (string.IsNullOrEmpty(recordId))
                    return CreateHttpResponseException("Record ID must be specified.", HttpStatusCode.BadRequest);

                var consents = await taxFormConsentService.Get2Async(personId, TaxFormTypes.Form1098);
                consents = consents.OrderByDescending(c => c.TimeStamp);
                var mostRecentConsent = consents.FirstOrDefault();

                // Check if the person has consented to receiving their 1098 online OR if the user is an admin user (which ignores the consent check)
                // If the consent check fails and the client requires consent to view 1098 forms online (T9TD and T9ED) throw an "Unauthorized" HTTP exception.
                // Note that the required permission checks are handled during data retrieval (in the "Get1098TaxFormData" method).
                var canViewAsAdmin = await taxFormConsentService.CanViewTaxDataWithOrWithoutConsent2Async(TaxFormTypes.Form1098);
                if ((mostRecentConsent == null || !mostRecentConsent.HasConsented) && !canViewAsAdmin)
                {
                    var configuration = await configurationService.GetTaxFormConsentConfiguration2Async(TaxFormTypes.Form1098);

                    if (!configuration.IsBypassingConsentPermitted)
                    {
                        return CreateHttpResponseException("Consent is required to view this information.", HttpStatusCode.Unauthorized);
                    }
                }

                string pdfTemplatePath = string.Empty;
                var pdfData = await taxFormPdfService.Get1098TaxFormData(personId, recordId);
                if (pdfData != null && !String.IsNullOrEmpty(pdfData.TaxFormName))
                {
                    pdfTemplatePath = GetPdfTemplatePath(pdfData);
                }

                var pdfBytes = taxFormPdfService.Populate1098Report(pdfData, pdfTemplatePath);

                // Create and return the HTTP response object
                var fileNameString = "TaxForm1098" + "_" + recordId + ".pdf";
                return File(pdfBytes, "application/pdf", fileNameString);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException(pe.Message, HttpStatusCode.Forbidden);
            }
            catch (ApplicationException ae)
            {
                logger.LogError(ae, ae.Message);
                return CreateHttpResponseException("Error retrieving 1098 PDF data.", HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return CreateHttpResponseException("Error retrieving 1098 PDF data.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get the appropriate tax form template. 
        /// Updated in 2023 to use FastReport (.frx) instead of Microsoft RDLC Report Designer (.rdlc) and 
        /// keep only tax years up to 2016.
        /// </summary>
        /// <param name="pdfData"></param>
        /// <returns></returns>
        /// <exception cref="ApplicationException">Cannot find a templae for the year.</exception>
        private string GetPdfTemplatePath(Domain.Student.Entities.Form1098PdfData pdfData)
        {
            string pdfTemplatePath;
            // Determine which PDF template to use. As of 2023 we use FastReport (.frx).
            switch (pdfData.TaxYear)
            {
                case "2023":
                    pdfTemplatePath = Path.Combine(webHostEnvironment.ContentRootPath, "Reports/Student/2023-" + pdfData.TaxFormName + ".frx");
                    break;
                case "2022":
                    pdfTemplatePath = Path.Combine(webHostEnvironment.ContentRootPath, "Reports/Student/2022-" + pdfData.TaxFormName + ".frx");
                    break;
                case "2021":
                    pdfTemplatePath = Path.Combine(webHostEnvironment.ContentRootPath, "Reports/Student/2021-" + pdfData.TaxFormName + ".frx");
                    break;
                case "2020":
                    pdfTemplatePath = Path.Combine(webHostEnvironment.ContentRootPath, "Reports/Student/2020-" + pdfData.TaxFormName + ".frx");
                    break;
                case "2019":
                    pdfTemplatePath = Path.Combine(webHostEnvironment.ContentRootPath, "Reports/Student/2019-" + pdfData.TaxFormName + ".frx");
                    break;
                case "2018":
                    pdfTemplatePath = Path.Combine(webHostEnvironment.ContentRootPath, "Reports/Student/2018-" + pdfData.TaxFormName + ".frx");
                    break;
                case "2017":
                    pdfTemplatePath = Path.Combine(webHostEnvironment.ContentRootPath, "Reports/Student/2017-" + pdfData.TaxFormName + ".frx");
                    break;
                case "2016":
                    pdfTemplatePath = Path.Combine(webHostEnvironment.ContentRootPath, "Reports/Student/2016-" + pdfData.TaxFormName + ".frx");
                    break;
                default:
                    var message = string.Format("Incorrect Tax Year {0}", pdfData.TaxYear);
                    logger.LogError(message);
                    throw new ApplicationException(message);
            }

            return pdfTemplatePath;
        }

        /// <summary>
        /// Returns the data to be printed on the pdf for the T2202a tax form.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the T2202a.</param>
        /// <param name="recordId">The record ID where the T2202a pdf data is stored</param>
        /// <accessComments>
        /// Requires permission VIEW.T2202A for the student.
        /// Requires permission VIEW.T2202A for someone who currently has permission to proxy for the student requested.
        /// Requires permission VIEW.STUDENT.T2202A for admin view.
        /// The tax form record requested must belong to the person ID requested.
        /// </accessComments>
        /// <returns>An HttpResponseMessage containing a byte array representing a PDF.</returns>
        [HttpGet]
        [HeaderVersionRoute("/persons/{personId}/formT2202as/{recordId}", 2, true, RouteConstants.EllucianPDFMediaTypeFormat, Name = "GetT2202aTaxFormPdf2")]
        public async Task<IActionResult> GetT2202aTaxFormPdf2Async(string personId, string recordId)
        {
            try
            {
                if (string.IsNullOrEmpty(personId))
                    return CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);
                if (string.IsNullOrEmpty(recordId))
                    return CreateHttpResponseException("Record ID must be specified.", HttpStatusCode.BadRequest);

                var config = await configurationService.GetTaxFormConsentConfiguration2Async(TaxFormTypes.FormT2202A);

                // if consents are hidden, don't bother evaluating them
                if (config == null || !config.HideConsent)
                {
                    logger.LogDebug("Using consents for T2202 tax forms");
                    var consents = await taxFormConsentService.Get2Async(personId, TaxFormTypes.FormT2202A);
                    consents = consents.OrderByDescending(c => c.TimeStamp);
                    var mostRecentConsent = consents.FirstOrDefault();

                    // ************* T4s and T2202As are special cases based on CRA regulations! *************
                    // Check if the person has explicitly withheld consent to receiving their T2202a online - if they opted out, throw exception
                    var canViewAsAdmin = await taxFormConsentService.CanViewTaxDataWithOrWithoutConsent2Async(TaxFormTypes.FormT2202A);
                    if ((mostRecentConsent != null && !mostRecentConsent.HasConsented) && !canViewAsAdmin)
                    {
                        logger.LogDebug("Consent is required to view T2202 information.");
                        return CreateHttpResponseException("Consent is required to view this information.", HttpStatusCode.Unauthorized);
                    }
                }
                string pdfTemplatePath = string.Empty;
                var pdfData = await taxFormPdfService.GetT2202aTaxFormData(personId, recordId);
                if (pdfData != null && pdfData.TaxYear != null)
                {
                    logger.LogDebug("Retrieving T2202 PDF for tax year '" + pdfData.TaxYear + "'");
                }
                else
                {
                    logger.LogDebug("No PDF data and/or tax year found.");
                }

                // Determine which PDF template to use.
                // Updated in 2023 to use FastReport (.frx) instead of Microsoft RDLC Report Designer (.rdlc) and 
                // keep only tax years up to 2016.
                int taxYear;

                if (Int32.TryParse(pdfData.TaxYear, out taxYear))
                {
                    if (taxYear == 2016 || taxYear == 2017)
                    {
                        pdfTemplatePath = Path.Combine(webHostEnvironment.ContentRootPath, "Reports/Student/20XX-T2202a.frx");
                    }
                    else if (taxYear == 2018)
                    {
                        pdfTemplatePath = Path.Combine(webHostEnvironment.ContentRootPath, "Reports/Student/2018-T2202a.frx");
                    }
                    else if (taxYear == 2019)
                    {
                        pdfTemplatePath = Path.Combine(webHostEnvironment.ContentRootPath, "Reports/Student/2019-T2202.frx");
                    }
                    else if (taxYear == 2020)
                    {
                        pdfTemplatePath = Path.Combine(webHostEnvironment.ContentRootPath, "Reports/Student/2020-T2202.frx");
                    }
                    else if (taxYear == 2021)
                    {
                        pdfTemplatePath = Path.Combine(webHostEnvironment.ContentRootPath, "Reports/Student/2021-T2202.frx");
                    }
                    else if (taxYear == 2022)
                    {
                        pdfTemplatePath = Path.Combine(webHostEnvironment.ContentRootPath, "Reports/Student/2022-T2202.frx");
                    }
                    else if (taxYear == 2023)
                    {
                        pdfTemplatePath = Path.Combine(webHostEnvironment.ContentRootPath, "Reports/Student/2023-T2202.frx");
                    }
                    else
                    {
                        var message = string.Format("Unsupported Tax Year: {0}", pdfData.TaxYear);
                        logger.LogError(message);
                        throw new ApplicationException(message);
                    }
                }
                else
                {
                    var message = string.Format("Incorrect Tax Year {0}", pdfData.TaxYear);
                    logger.LogError(message);
                    throw new ApplicationException(message);
                }
                logger.LogDebug("Template found. Using '" + (pdfTemplatePath ?? string.Empty) + "'");

                var pdfBytes = new byte[0];
                pdfBytes = taxFormPdfService.PopulateT2202aReport(pdfData, pdfTemplatePath);

                var fileNameString = "TaxFormT2202a" + "_" + recordId + ".pdf";
                return File(pdfBytes, "application/pdf", fileNameString);

            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException(pe.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Error retrieving T2202A PDF data.", HttpStatusCode.BadRequest);
            }
        }


        #region OBSOLETE METHODS

        /// <summary>
        /// Returns the data to be printed on the pdf for the 1098 tax form.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the 1098.</param>
        /// <param name="recordId">The record ID where the 1098 pdf data is stored</param>
        /// <accessComments>
        /// Requires permission VIEW.1098 for the student.
        /// Requires permission VIEW.1098 for someone who currently has permission to proxy for the student requested.
        /// Requires permission VIEW.STUDENT.1098 for admin view.
        /// The tax form record requested must belong to the person ID requested.
        /// </accessComments>
        /// <returns>An HttpResponseMessage containing a byte array representing a PDF.</returns>
        [Obsolete("Obsolete as of API 1.29.1. Use Get1098TaxFormPdf2Async instead.")]
        [HttpGet]
        [HeaderVersionRoute("/persons/{personId}/form1098ts/{recordId}", 1, false, RouteConstants.EllucianPDFMediaTypeFormat, Name = "Get1098TaxFormPdf2")]
        public async Task<IActionResult> Get1098TaxFormPdf(string personId, string recordId)
        {
            if (string.IsNullOrEmpty(personId))
                return CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

            if (string.IsNullOrEmpty(recordId))
                return CreateHttpResponseException("Record ID must be specified.", HttpStatusCode.BadRequest);

            var consents = await taxFormConsentService.GetAsync(personId, Dtos.Base.TaxForms.Form1098);
            consents = consents.OrderByDescending(c => c.TimeStamp);
            var mostRecentConsent = consents.FirstOrDefault();

            // Check if the person has consented to receiving their 1098 online - if not, throw exception
            var canViewAsAdmin = await taxFormConsentService.CanViewTaxDataWithOrWithoutConsent(Dtos.Base.TaxForms.Form1098);
            if ((mostRecentConsent == null || !mostRecentConsent.HasConsented) && !canViewAsAdmin)
            {
                return CreateHttpResponseException("Consent is required to view this information.", HttpStatusCode.Unauthorized);
            }

            string pdfTemplatePath = string.Empty;
            try
            {
                var pdfData = await taxFormPdfService.Get1098TaxFormData(personId, recordId);
                if (pdfData != null && !String.IsNullOrEmpty(pdfData.TaxFormName))
                {
                    pdfTemplatePath = GetPdfTemplatePath(pdfData);
                }

                var pdfBytes = taxFormPdfService.Populate1098Report(pdfData, pdfTemplatePath);

                var fileNameString = "TaxForm1098" + "_" + recordId + ".pdf";
                return File(pdfBytes, "application/pdf", fileNameString);

            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException(pe.Message, HttpStatusCode.Forbidden);
            }
            catch (ApplicationException ae)
            {
                logger.LogError(ae, ae.Message);
                return CreateHttpResponseException("Error retrieving 1098 PDF data.", HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return CreateHttpResponseException("Error retrieving 1098 PDF data.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns the data to be printed on the pdf for the T2202a tax form.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the T2202a.</param>
        /// <param name="recordId">The record ID where the T2202a pdf data is stored</param>
        /// <accessComments>
        /// Requires permission VIEW.T2202A for the student.
        /// Requires permission VIEW.T2202A for someone who currently has permission to proxy for the student requested.
        /// Requires permission VIEW.STUDENT.T2202A for admin view.
        /// The tax form record requested must belong to the person ID requested.
        /// </accessComments>
        /// <returns>An HttpResponseMessage containing a byte array representing a PDF.</returns>
        [Obsolete("Obsolete as of API 1.29.1. Use GetT2202aTaxFormPdf2Async instead.")]
        [HttpGet]
        [HeaderVersionRoute("/persons/{personId}/formT2202as/{recordId}", 1, false, RouteConstants.EllucianPDFMediaTypeFormat, Name = "GetT2202aTaxFormPdf")]
        public async Task<IActionResult> GetT2202aTaxFormPdf(string personId, string recordId)
        {
            if (string.IsNullOrEmpty(personId))
                return CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

            if (string.IsNullOrEmpty(recordId))
                return CreateHttpResponseException("Record ID must be specified.", HttpStatusCode.BadRequest);

            var consents = await taxFormConsentService.GetAsync(personId, Dtos.Base.TaxForms.FormT2202A);
            consents = consents.OrderByDescending(c => c.TimeStamp);
            var mostRecentConsent = consents.FirstOrDefault();

            // ************* T4s and T2202As are special cases based on CRA regulations! *************
            // Check if the person has explicitly withheld consent to receiving their T2202a online - if they opted out, throw exception
            var canViewAsAdmin = await taxFormConsentService.CanViewTaxDataWithOrWithoutConsent(Dtos.Base.TaxForms.FormT2202A);
            if ((mostRecentConsent != null && !mostRecentConsent.HasConsented) && !canViewAsAdmin)
            {
                return CreateHttpResponseException("Consent is required to view this information.", HttpStatusCode.Unauthorized);
            }

            string pdfTemplatePath = string.Empty;
            try
            {
                var pdfData = await taxFormPdfService.GetT2202aTaxFormData(personId, recordId);

                // Determine which PDF template to use.
                int taxYear;

                if (Int32.TryParse(pdfData.TaxYear, out taxYear))
                {
                    if (taxYear >= 2008 && taxYear <= 2017)
                    {

                        pdfTemplatePath = Path.Combine(webHostEnvironment.ContentRootPath, "Reports/Student/20XX-T2202a.rdlc");
                    }
                    else if (taxYear == 2018)
                    {
                        pdfTemplatePath = Path.Combine(webHostEnvironment.ContentRootPath, "Reports/Student/2018-T2202a.rdlc");
                    }
                    else if (taxYear == 2019)
                    {
                        pdfTemplatePath = Path.Combine(webHostEnvironment.ContentRootPath, "Reports/Student/2019-T2202.rdlc");
                    }
                }
                else
                {
                    var message = string.Format("Incorrect Tax Year {0}", pdfData.TaxYear);
                    logger.LogError(message);
                    throw new ApplicationException(message);
                }

                var pdfBytes = new byte[0];
                pdfBytes = taxFormPdfService.PopulateT2202aReport(pdfData, pdfTemplatePath);

                var fileNameString = "TaxFormT2202a" + "_" + recordId + ".pdf";
                return File(pdfBytes, "application/pdf", fileNameString);

            }
            catch (PermissionsException pe)
            {
                logger.LogError(pe, pe.Message);
                return CreateHttpResponseException(pe.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return CreateHttpResponseException("Error retrieving T2202A PDF data.", HttpStatusCode.BadRequest);
            }
        }

        #endregion
    }
}
