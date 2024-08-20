// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using System;
using System.ComponentModel;
using System.Threading.Tasks;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Web;
using System.Net.Http.Headers;
using System.Net;
using Ellucian.Colleague.Coordination.Base.Services;
using System.Linq;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Colleague.Api;
using Microsoft.AspNetCore.Hosting;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to FormT4aPdfData objects.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class ColleagueFinanceTaxFormPdfsController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;
        private readonly IColleagueFinanceTaxFormPdfService taxFormPdfService;
        private readonly ITaxFormConsentService taxFormConsentService;
        private readonly IConfigurationService configurationService;
        private readonly IWebHostEnvironment webHostEnvironment;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initialize the controller.
        /// </summary>
        /// <param name="adapterRegistry">Adapter Registry</param>
        /// <param name="logger">Logger object</param>
        /// <param name="taxFormPdfService">FormT4aPdfData service object</param>
        /// <param name="taxFormConsentService">Form T4A consent service object</param>
        /// <param name="configurationService">Configuation service object</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="webHostEnvironment"></param>
        /// <param name="apiSettings"></param>
        public ColleagueFinanceTaxFormPdfsController(IAdapterRegistry adapterRegistry, ILogger logger,
            IColleagueFinanceTaxFormPdfService taxFormPdfService, ITaxFormConsentService taxFormConsentService,
            IConfigurationService configurationService, IActionContextAccessor actionContextAccessor,
            IWebHostEnvironment webHostEnvironment, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
            this.taxFormPdfService = taxFormPdfService;
            this.taxFormConsentService = taxFormConsentService;
            this.configurationService = configurationService;
            this.webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Returns the data to be printed on the pdf for the T4A tax form.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the T4A.</param>
        /// <param name="recordId">The record ID where the T4A pdf data is stored</param>  
        /// <accessComments>
        /// Requires permission VIEW.T4A for the recipient.
        /// Requires permission VIEW.T4A for someone who currently has permission to proxy for the recipient requested.
        /// Requires permission VIEW.RECIPIENT.T4A for admin view.
        /// The tax form record requested must belong to the person ID requested.
        /// </accessComments>        
        /// <returns>An ActionResult containing a byte array representing a PDF.</returns>
        [HttpGet]
        [HeaderVersionRoute("/persons/{personId}/formT4as/{recordId}", 2, true, RouteConstants.EllucianPDFMediaTypeFormat, Name = "GetT4aTaxFormPdf2")]
        public async Task<IActionResult> GetFormT4aPdf2Async(string personId, string recordId)
        {
            try
            {
                if (string.IsNullOrEmpty(personId))
                    return CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

                if (string.IsNullOrEmpty(recordId))
                    return CreateHttpResponseException("Record ID must be specified.", HttpStatusCode.BadRequest);

                var config = await configurationService.GetTaxFormConsentConfiguration2Async(TaxFormTypes.FormT4A);

                // if consents are hidden, don't bother evaluating them
                if (config == null || !config.HideConsent)
                {
                    logger.LogDebug("Using consents for T4A tax forms");

                    var consents = await taxFormConsentService.Get2Async(personId, TaxFormTypes.FormT4A);
                    consents = consents.OrderByDescending(c => c.TimeStamp);
                    var mostRecentConsent = consents.FirstOrDefault();

                    // Check if the person has consented to receiving their T4A online - if not, throw exception
                    var canViewAsAdmin = await taxFormConsentService.CanViewTaxDataWithOrWithoutConsent2Async(TaxFormTypes.FormT4A);

                    if ((mostRecentConsent == null || !mostRecentConsent.HasConsented) && !canViewAsAdmin)
                    {
                        logger.LogDebug("Consent is required to view T4A information.");
                        return CreateHttpResponseException("Consent is required to view this information.", HttpStatusCode.Unauthorized);
                    }
                }
                
                string pdfTemplatePath = string.Empty;
                var pdfData = await taxFormPdfService.GetFormT4aPdfDataAsync(personId, recordId);
                if (pdfData != null && pdfData.TaxYear != null)
                {
                    logger.LogDebug("Retrieving T4A PDF for tax year '" + pdfData.TaxYear + "'");
                }
                else
                {
                    logger.LogDebug("No PDF data and/or tax year found.");
                }

                // Updated in 2023 to use FastReport (.frx) instead of Microsoft RDLC Report Designer (.rdlc) and 
                // keep only tax years up to 2016.          

                // Determine which PDF template to use.As of 2023 we use FastReport (.frx).
                switch (pdfData.TaxYear)
                {
                    case "2023":
                        pdfTemplatePath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/ColleagueFinance/2023-T4A.frx");
                        break;
                    case "2022":
                        pdfTemplatePath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/ColleagueFinance/2022-T4A.frx");
                        break;
                    case "2021":
                        pdfTemplatePath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/ColleagueFinance/2021-T4A.frx");
                        break;
                    case "2020":
                        pdfTemplatePath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/ColleagueFinance/2020-T4A.frx");
                        break;
                    case "2019":
                        pdfTemplatePath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/ColleagueFinance/2019-T4A.frx");
                        break;
                    case "2018":
                        pdfTemplatePath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/ColleagueFinance/2018-T4A.frx");
                        break;
                    case "2017":
                        pdfTemplatePath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/ColleagueFinance/2017-T4A.frx");
                        break;
                    case "2016":
                        pdfTemplatePath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/ColleagueFinance/2016-T4A.frx");
                        break;
                    default:
                        var message = string.Format("Incorrect Tax Year {0}", pdfData.TaxYear);
                        logger.LogError(message);
                        throw new ApplicationException(message);
                }
                logger.LogDebug("Template found. Using '" + (pdfTemplatePath ?? string.Empty) + "'");

                var pdfBytes = taxFormPdfService.PopulateT4aPdf(pdfData, pdfTemplatePath);
                // Create and return the HTTP response object
                var fileNameString = "TaxFormT4A" + "_" + recordId;
                return File(pdfBytes, "application/pdf", fileNameString + ".pdf");
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex, peex.Message);
                return CreateHttpResponseException("Insufficient permissions to get the T4A PDF data.", HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return CreateHttpResponseException("Error retrieving T4A PDF data.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns the data to be printed on the pdf for the 1099-MISC tax form.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the 1099-MISC.</param>
        /// <param name="recordId">The record ID where the 1099-MISC pdf data is stored</param>         
        /// <accessComments>
        /// Requires permission VIEW.1099MISC.
        /// The tax form record requested must belong to the current user.       
        /// </accessComments>
        /// <returns>An IActionResult containing a byte array representing a PDF.</returns> 
        [HttpGet]
        [HeaderVersionRoute("/persons/{personId}/form1099Miscs/{recordId}", 2, true, RouteConstants.EllucianPDFMediaTypeFormat, Name = "Get1099MiscTaxFormPdf2")]
        public async Task<IActionResult> Get1099MiscTaxFormPdf2Async(string personId, string recordId)
        {
            try
            {
                if (string.IsNullOrEmpty(personId))
                    return CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

                if (string.IsNullOrEmpty(recordId))
                    return CreateHttpResponseException("Record ID must be specified.", HttpStatusCode.BadRequest);
            
                var consents = await taxFormConsentService.Get2Async(personId, TaxFormTypes.Form1099MI);
                consents = consents.OrderByDescending(c => c.TimeStamp);
                var mostRecentConsent = consents.FirstOrDefault();

                if (mostRecentConsent == null || !mostRecentConsent.HasConsented)
                {
                    return CreateHttpResponseException("Consent is required to view this information.", HttpStatusCode.Unauthorized);
                }

                string pdfTemplatePath = string.Empty;
                var pdfData = await taxFormPdfService.Get1099MiscPdfDataAsync(personId, recordId);
                // Determine which PDF template to use.
                // Any new year has to be added to the list in the ColleagueFinanceConstants class.
                // Updated in 2023 to use FastReport (.frx) instead of Microsoft RDLC Report Designer (.rdlc) and 
                // keep only tax years up to 2016.
                switch (pdfData.TaxYear)
                {
                    case "2018":
                    case "2017":
                    case "2016":
                    case "2019":
                        pdfTemplatePath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/ColleagueFinance/20XX-1099MI.frx");
                        break;
                    case "2020":
                        pdfTemplatePath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/ColleagueFinance/2020-1099MI.frx");
                        break;
                    case "2021":
                        pdfTemplatePath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/ColleagueFinance/2021-1099MI.frx");
                        break;
                    case "2022":
                        pdfTemplatePath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/ColleagueFinance/2022-1099MI.frx");
                        break;
                    case "2023":
                        pdfTemplatePath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/ColleagueFinance/2023-1099MI.frx");
                        break;
                    default:
                        var message = string.Format("Incorrect Tax Year {0}", pdfData.TaxYear);
                        logger.LogError(message);
                        throw new ApplicationException(message);
                }

                var pdfBytes = taxFormPdfService.Populate1099MiscPdf(pdfData, pdfTemplatePath);

                // Create and return the HTTP response object
                var fileNameString = "TaxForm1099MI" + "_" + recordId;
                return File(pdfBytes, "application/pdf", fileNameString + ".pdf");

            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex, peex.Message);
                return CreateHttpResponseException("Insufficient permissions to get the 1099-MISC PDF data.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Error retrieving 1099-MISC PDF data.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns the data to be printed on the pdf for the 1099-NEC tax form.
        /// Updated in 2023 to use FastReport (.frx) instead of Microsoft RDLC Report Designer (.rdlc) and 
        /// keep only tax years up to 2016.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the 1099-NEC.</param>
        /// <param name="recordId">The record ID where the 1099-NEC pdf data is stored</param>         
        /// <accessComments>
        /// Requires permission VIEW.1099NEC.
        /// The tax form record requested must belong to the current user.       
        /// </accessComments>
        /// <returns>An PDF document as byte array.</returns>  
        [HttpGet]
        [HeaderVersionRoute("/persons/{personId}/form1099Nec/{recordId}", 1, true, RouteConstants.EllucianPDFMediaTypeFormat, Name = "Get1099NecTaxFormPdf")]
        public async Task<IActionResult> Get1099NecTaxFormPdfAsync(string personId, string recordId)
        {
            try
            {
                if (string.IsNullOrEmpty(personId))
                    return CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

                if (string.IsNullOrEmpty(recordId))
                    return CreateHttpResponseException("Record ID must be specified.", HttpStatusCode.BadRequest);
            
                // Check if the person has consented to receiving their 1099-NEC online - if not, throw exception
                var consents = await taxFormConsentService.Get2Async(personId, TaxFormTypes.Form1099NEC);
                consents = consents.OrderByDescending(c => c.TimeStamp);
                var mostRecentConsent = consents.FirstOrDefault();

                if (mostRecentConsent == null || !mostRecentConsent.HasConsented)
                {
                    return CreateHttpResponseException("Consent is required to view this information.", HttpStatusCode.Unauthorized);
                }

                string pdfTemplatePath = string.Empty;
                var pdfData = await taxFormPdfService.Get1099NecPdfDataAsync(personId, recordId);

                // Determine which PDF template to use. As of 2023 we use FastReport (.frx).
                // Any new year has to be added to the list in the ColleagueFinanceConstants class.
                switch (pdfData.TaxYear)
                {
                    case "2020":
                        pdfTemplatePath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/ColleagueFinance/2020-1099NEC.frx");
                        break;
                    case "2021":
                        pdfTemplatePath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/ColleagueFinance/2021-1099NEC.frx");
                        break;
                    case "2022":
                        pdfTemplatePath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/ColleagueFinance/2022-1099NEC.frx");
                        break;
                    case "2023":
                        pdfTemplatePath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/ColleagueFinance/2023-1099NEC.frx");
                        break;
                    default:
                        var message = string.Format("Incorrect Tax Year {0}", pdfData.TaxYear);
                        logger.LogError(message);
                        throw new ApplicationException(message);
                }

                var pdfBytes = taxFormPdfService.Populate1099NecPdf(pdfData, pdfTemplatePath);

                // Create and return the HTTP response object
                
                var fileNameString = "TaxForm1099NEC" + "_" + recordId;
                return File(pdfBytes, "application/pdf", fileNameString + ".pdf");

            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex, peex.Message);
                return CreateHttpResponseException("Insufficient permissions to get the 1099-NEC PDF data.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Error retrieving 1099-NEC PDF data.", HttpStatusCode.BadRequest);
            }
        }


        #region OBSOLETE METHODS

        /// <summary>
        /// Returns the data to be printed on the pdf for the T4A tax form.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the T4A.</param>
        /// <param name="recordId">The record ID where the T4A pdf data is stored</param>  
        /// <accessComments>
        /// Requires permission VIEW.T4A for the recipient.
        /// Requires permission VIEW.T4A for someone who currently has permission to proxy for the recipient requested.
        /// Requires permission VIEW.RECIPIENT.T4A for admin view.
        /// The tax form record requested must belong to the person ID requested.
        /// </accessComments>        
        /// <returns>An IActionResult containing a byte array representing a PDF.</returns>
        [Obsolete("Obsolete as of API 1.29.1. Use GetFormT4aPdf2Async instead.")]
        [HttpGet]
        [HeaderVersionRoute("/persons/{personId}/formT4as/{recordId}", 1, false, RouteConstants.EllucianPDFMediaTypeFormat, Name = "GetT4aTaxFormPdf")]
        public async Task<IActionResult> GetFormT4aPdfAsync(string personId, string recordId)
        {
            if (string.IsNullOrEmpty(personId))
                return CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

            if (string.IsNullOrEmpty(recordId))
                return CreateHttpResponseException("Record ID must be specified.", HttpStatusCode.BadRequest);

            var consents = await taxFormConsentService.GetAsync(personId, Dtos.Base.TaxForms.FormT4A);
            consents = consents.OrderByDescending(c => c.TimeStamp);
            var mostRecentConsent = consents.FirstOrDefault();

            // Check if the person has consented to receiving their T4A online - if not, throw exception
            var canViewAsAdmin = await taxFormConsentService.CanViewTaxDataWithOrWithoutConsent(Dtos.Base.TaxForms.FormT4A);
            if ((mostRecentConsent == null || !mostRecentConsent.HasConsented) && !canViewAsAdmin)
            {
                return CreateHttpResponseException("Consent is required to view this information.", HttpStatusCode.Unauthorized);
            }

            string pdfTemplatePath = string.Empty;
            try
            {
                var pdfData = await taxFormPdfService.GetFormT4aPdfDataAsync(personId, recordId);

                // Determine which PDF template to use.
                switch (pdfData.TaxYear)
                {
                    case "2019":
                        pdfTemplatePath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/ColleagueFinance/2019-T4A.rdlc");
                        break;
                    case "2018":
                        pdfTemplatePath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/ColleagueFinance/2018-T4A.rdlc");
                        break;
                    case "2017":
                        pdfTemplatePath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/ColleagueFinance/2017-T4A.rdlc");
                        break;
                    case "2016":
                        pdfTemplatePath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/ColleagueFinance/2016-T4A.rdlc");
                        break;
                    case "2015":
                        pdfTemplatePath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/ColleagueFinance/2015-T4A.rdlc");
                        break;
                    case "2014":
                        pdfTemplatePath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/ColleagueFinance/2014-T4A.rdlc");
                        break;
                    case "2013":
                        pdfTemplatePath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/ColleagueFinance/2013-T4A.rdlc");
                        break;
                    case "2012":
                        pdfTemplatePath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/ColleagueFinance/2012-T4A.rdlc");
                        break;
                    case "2011":
                        pdfTemplatePath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/ColleagueFinance/2011-T4A.rdlc");
                        break;
                    case "2010":
                        pdfTemplatePath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/ColleagueFinance/2010-T4A.rdlc");
                        break;
                    case "2009":
                        pdfTemplatePath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/ColleagueFinance/2009-T4A.rdlc");
                        break;
                    case "2008":
                        pdfTemplatePath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/ColleagueFinance/2008-T4A.rdlc");
                        break;
                    default:
                        var message = string.Format("Incorrect Tax Year {0}", pdfData.TaxYear);
                        logger.LogError(message);
                        throw new ApplicationException(message);
                }

                var pdfBytes = taxFormPdfService.PopulateT4aPdf(pdfData, pdfTemplatePath);

                // Create and return the HTTP response object
                var fileNameString = "TaxFormT4A" + "_" + recordId;
                return File(pdfBytes, "application/pdf", fileNameString + ".pdf");

            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex, peex.Message);
                return CreateHttpResponseException("Insufficient permissions to get the T4A PDF data.", HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return CreateHttpResponseException("Error retrieving T4A PDF data.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns the data to be printed on the pdf for the 1099-MISC tax form.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the 1099-MISC.</param>
        /// <param name="recordId">The record ID where the 1099-MISC pdf data is stored</param>         
        /// <accessComments>
        /// Requires permission VIEW.1099MISC.
        /// The tax form record requested must belong to the current user.       
        /// </accessComments>
        /// <returns>An IActionResult containing a byte array representing a PDF.</returns> 
        [Obsolete("Obsolete as of API 1.29.1. Use Get1099MiscTaxFormPdf2Async instead.")]
        [HttpGet]
        [HeaderVersionRoute("/persons/{personId}/form1099Miscs/{recordId}", 1, false, RouteConstants.EllucianPDFMediaTypeFormat, Name = "Get1099MiscTaxFormPdf")]
        public async Task<IActionResult> Get1099MiscTaxFormPdfAsync(string personId, string recordId)
        {
            if (string.IsNullOrEmpty(personId))
                return CreateHttpResponseException("Person ID must be specified.", HttpStatusCode.BadRequest);

            if (string.IsNullOrEmpty(recordId))
                return CreateHttpResponseException("Record ID must be specified.", HttpStatusCode.BadRequest);

            var consents = await taxFormConsentService.GetAsync(personId, Dtos.Base.TaxForms.Form1099MI);
            consents = consents.OrderByDescending(c => c.TimeStamp);
            var mostRecentConsent = consents.FirstOrDefault();

            if (mostRecentConsent == null || !mostRecentConsent.HasConsented)
            {
                return CreateHttpResponseException("Consent is required to view this information.", HttpStatusCode.Unauthorized);
            }

            string pdfTemplatePath = string.Empty;
            try
            {
                var pdfData = await taxFormPdfService.Get1099MiscPdfDataAsync(personId, recordId);
                // Determine which PDF template to use.
                // Any new year has to be added to the list in the ColleagueFinanceConstants class.
                switch (pdfData.TaxYear)
                {
                    case "2009":
                        pdfTemplatePath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/ColleagueFinance/2009-1099MI.rdlc");
                        break;
                    case "2008":
                        pdfTemplatePath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/ColleagueFinance/2008-1099MI.rdlc");
                        break;
                    case "2018":
                    case "2017":
                    case "2016":
                    case "2015":
                    case "2011":
                    case "2010":
                    case "2012":
                    case "2019":
                        pdfTemplatePath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/ColleagueFinance/20XX-1099MI.rdlc");
                        break;
                    case "2014":
                        pdfTemplatePath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/ColleagueFinance/2014-1099MI.rdlc");
                        break;
                    case "2013":
                        pdfTemplatePath = System.IO.Path.Combine(webHostEnvironment.ContentRootPath, "Reports/ColleagueFinance/2013-1099MI.rdlc");
                        break;
                    default:
                        var message = string.Format("Incorrect Tax Year {0}", pdfData.TaxYear);
                        logger.LogError(message);
                        throw new ApplicationException(message);
                }

                var pdfBytes = taxFormPdfService.Populate1099MiscPdf(pdfData, pdfTemplatePath);

                // Create and return the HTTP response object
                
                var fileNameString = "TaxForm1099MI" + "_" + recordId;
                return File(pdfBytes, "application/pdf", fileNameString + ".pdf");

            }
            catch (PermissionsException peex)
            {
                logger.LogError(peex, peex.Message);
                return CreateHttpResponseException("Insufficient permissions to get the 1099-MISC PDF data.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return CreateHttpResponseException("Error retrieving 1099-MISC PDF data.", HttpStatusCode.BadRequest);
            }
        }

        #endregion
    }
}
