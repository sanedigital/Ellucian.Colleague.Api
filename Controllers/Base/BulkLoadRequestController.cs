// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;

using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Routes;
using Ellucian.Web.Http.Models;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to BulkLoadRequest
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class BulkLoadRequestController : BaseCompressedApiController
    {
        private readonly IBulkLoadRequestService _bulkLoadRequestService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the BulkLoadRequestController class.
        /// </summary>
        /// <param name="bulkLoadRequestService">Service of type <see cref="IBulkLoadRequestService">IBulkLoadRequestService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public BulkLoadRequestController(IBulkLoadRequestService bulkLoadRequestService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _bulkLoadRequestService = bulkLoadRequestService;
            this._logger = logger;
        }

        /// <summary>
        /// Create (POST) a new BulkLoadRequest
        /// </summary>
        /// <param name="bulkLoadRequestDto">DTO of the new BulkLoadRequest</param>
        /// <returns>A BulkLoadRequest object <see cref="Dtos.BulkLoadRequest"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        [HeaderVersionBulkRepresentationRoute("/qapi/ledger-activities", 1, false, RouteConstants.HedtechIntegrationBulkRequestMediaTypeFormat, Name = "BulkLedgerActivitiesRequestV11_1_0", MediaTypeTemplate = RouteConstants.HedtechIntegrationMediaTypeFormat, MediaTypeVersion = "11.1.0", PermissionCode = "VIEW.LEDGER.ACTIVITIES")]
        [HeaderVersionBulkRepresentationRoute("/qapi/ledger-activities", 1, false, RouteConstants.HedtechIntegrationBulkRequestMediaTypeFormat, Name = "BulkLedgerActivitiesRequestV11", MediaTypeTemplate = RouteConstants.HedtechIntegrationMediaTypeFormat, MediaTypeVersion = "11.0.0", PermissionCode = "VIEW.LEDGER.ACTIVITIES")]
        [HeaderVersionBulkRepresentationRoute("/qapi/persons", 1, true, RouteConstants.HedtechIntegrationBulkRequestMediaTypeFormat, Name = "BulkPersonRequestV1", MediaTypeTemplate = RouteConstants.HedtechIntegrationMediaTypeFormat, MediaTypeVersion = "12.3.0", PermissionCode = "VIEW.ANY.PERSON")]
        [HeaderVersionBulkRepresentationRoute("/qapi/section-registrations", 1, false, RouteConstants.HedtechIntegrationBulkRequestMediaTypeFormat, Name = "BulkSectionRegistrationsRequestV1", MediaTypeTemplate = RouteConstants.HedtechIntegrationMediaTypeFormat, MediaTypeVersion = "16.0.0", PermissionCode = "VIEW.REGISTRATIONS")]
        [HeaderVersionBulkRepresentationRoute("/qapi/student-academic-periods", 1, false, RouteConstants.HedtechIntegrationBulkRequestMediaTypeFormat, Name = "BulkStudentAcademicPeriodsRequestV1", MediaTypeTemplate = RouteConstants.HedtechIntegrationMediaTypeFormat, MediaTypeVersion = "1.0.0", PermissionCode = "VIEW.STUDENT.ACADEMIC.PERIODS")]
        [HeaderVersionBulkRepresentationRoute("/qapi/student-grade-point-averages", 1, false, RouteConstants.HedtechIntegrationBulkRequestMediaTypeFormat, Name = "BulkStudentGradePointAveragesRequestV1", MediaTypeTemplate = RouteConstants.HedtechIntegrationMediaTypeFormat, MediaTypeVersion = "1.0.0", PermissionCode = "VIEW.STUDENT.GRADE.POINT.AVERAGES")]
        [HeaderVersionBulkRepresentationRoute("/qapi/student-transcript-grades", 1, false, RouteConstants.HedtechIntegrationBulkRequestMediaTypeFormat, Name = "BulkStudentTranscriptGradesRequestV1", MediaTypeTemplate = RouteConstants.HedtechIntegrationMediaTypeFormat, MediaTypeVersion = "1.0.0", PermissionCode = "VIEW.STUDENT.TRANSCRIPT.GRADES")]
        [HeaderVersionRoute("/qapi/ledger-activities", 1, false, RouteConstants.HedtechIntegrationBulkRequestMediaTypeFormat, Name = "BulkLedgerActivitiesRequestV11_1_0")]
        [HeaderVersionRoute("/qapi/persons", 1, true, RouteConstants.HedtechIntegrationBulkRequestMediaTypeFormat, Name = "BulkPersonRequestV1")]
        [HeaderVersionRoute("/qapi/section-registrations", 1, false, RouteConstants.HedtechIntegrationBulkRequestMediaTypeFormat, Name = "BulkSectionRegistrationsRequestV1")]
        [HeaderVersionRoute("/qapi/student-academic-periods", 1, false, RouteConstants.HedtechIntegrationBulkRequestMediaTypeFormat, Name = "BulkStudentAcademicPeriodsRequestV1")]
        [HeaderVersionRoute("/qapi/student-grade-point-averages", 1, false, RouteConstants.HedtechIntegrationBulkRequestMediaTypeFormat, Name = "BulkStudentGradePointAveragesRequestV1")]
        [HeaderVersionRoute("/qapi/student-transcript-grades", 1, false, RouteConstants.HedtechIntegrationBulkRequestMediaTypeFormat, Name = "BulkStudentTranscriptGradesRequestV1")]
        public async Task<ActionResult<Dtos.BulkLoadRequest>> PostBulkLoadRequestAsync(Dtos.BulkLoadRequest bulkLoadRequestDto)
        {
            if (bulkLoadRequestDto == null)
            {
                return CreateHttpResponseException("Request body must contain a valid BulkLoadRequest.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(bulkLoadRequestDto.RequestorTrackingId))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null BulkLoadRequest id",
                    IntegrationApiUtility.GetDefaultApiError("RequestorId is a required property.")));
            }

            if (string.IsNullOrEmpty(bulkLoadRequestDto.ApplicationId))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null ApplicationId",
                    IntegrationApiUtility.GetDefaultApiError("ApplicationId is a required property.")));
            }

            Guid guidOutput;
          
            if (!Guid.TryParse(bulkLoadRequestDto.ApplicationId, out guidOutput))
            {
                return CreateHttpResponseException(new IntegrationApiException("ApplicationId",
                   IntegrationApiUtility.GetDefaultApiError("Must provide a valid GUID for ApplicationId.")));
            }


            try
            {
                var ethosRouteInfo = GetEthosResourceRouteInfo();
                bulkLoadRequestDto.ResourceName = ethosRouteInfo.ResourceName;

                var routeData = _actionContextAccessor.ActionContext.RouteData;

                var bulkRepresentationRoute = _actionContextAccessor.ActionContext.ActionDescriptor.ActionConstraints.FirstOrDefault(c => c is HeaderVersionBulkRepresentationRouteAttribute) as HeaderVersionBulkRepresentationRouteAttribute;

                // see if the bulk representation attribute is present
                if (string.IsNullOrEmpty(bulkLoadRequestDto.Representation) || bulkLoadRequestDto.Representation.Contains("application/json"))
                {
                    // check for the route information
                    if (bulkRepresentationRoute != null && !string.IsNullOrWhiteSpace(bulkRepresentationRoute.MediaTypeTemplate))
                    {
                        bulkLoadRequestDto.Representation = string.Format(bulkRepresentationRoute.MediaTypeTemplate, bulkRepresentationRoute.MediaTypeVersion ?? string.Empty);
                    }
                    else
                    {
                        return CreateHttpResponseException(new IntegrationApiException("Null Default Bulk Representation",
                            IntegrationApiUtility.GetDefaultApiError("Default Bulk Representation is not set on route.")));
                    }
                }

                string permissionCode = string.Empty;
                if (!string.IsNullOrWhiteSpace(bulkRepresentationRoute?.PermissionCode))
                {
                    permissionCode = bulkRepresentationRoute?.PermissionCode;
                }
                else
                {
                    return CreateHttpResponseException(new IntegrationApiException("Null PermissionCode",
                        IntegrationApiUtility.GetDefaultApiError("PermissionCode is not set on route.")));
                }

                return await _bulkLoadRequestService.CreateBulkLoadRequestAsync(bulkLoadRequestDto, permissionCode);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ConfigurationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }


        /// <summary>
        ///  Get the status of the bulk request
        /// </summary>
        /// <param name="criteria">filter</param>
        /// <param name="guid">guid</param>
        /// <returns>The requested <see cref="BulkLoadGet">object</see></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.BulkLoadGet)), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HttpGet]
        [HeaderVersionBulkRepresentationRoute("/ledger-activities/{guid}", 1, false, RouteConstants.HedtechIntegrationBulkRequestMediaTypeFormat, Name = "BulkLedgerActivitiesStatusV1", PermissionCode = "VIEW.LEDGER.ACTIVITIES")]
        [HeaderVersionBulkRepresentationRoute("/persons/{guid}", 1, false, RouteConstants.HedtechIntegrationBulkRequestMediaTypeFormat, Name = "BulkPersonRequestStatusV1", PermissionCode = "VIEW.ANY.PERSON")]
        [HeaderVersionBulkRepresentationRoute("/section-registrations/{guid}", 1, false, RouteConstants.HedtechIntegrationBulkRequestMediaTypeFormat, Name = "BulkSectionRegistrationsRequestStatusV1", PermissionCode = "VIEW.REGISTRATIONS")]
        [HeaderVersionBulkRepresentationRoute("/student-academic-periods/{guid}", 1, false, RouteConstants.HedtechIntegrationBulkRequestMediaTypeFormat, Name = "BulkStudentAcademicPeriodsRequestStatusV1", PermissionCode = "VIEW.STUDENT.ACADEMIC.PERIODS")]
        [HeaderVersionBulkRepresentationRoute("/student-grade-point-averages/{guid}", 1, false, RouteConstants.HedtechIntegrationBulkRequestMediaTypeFormat, Name = "BulkStudentGradePointAveragesRequestStatusV1", PermissionCode = "VIEW.STUDENT.GRADE.POINT.AVERAGES")]
        [HeaderVersionBulkRepresentationRoute("/student-transcript-grades/{guid}", 1, false, RouteConstants.HedtechIntegrationBulkRequestMediaTypeFormat, Name = "BulkStudentTranscriptGradesRequestStatusV1", PermissionCode = "VIEW.STUDENT.TRANSCRIPT.GRADES")]
        [HeaderVersionRoute("/ledger-activities/{guid}", 1, false, RouteConstants.HedtechIntegrationBulkRequestMediaTypeFormat, Name = "BulkLedgerActivitiesStatusV1")]
        [HeaderVersionRoute("/persons/{guid}", 1, false, RouteConstants.HedtechIntegrationBulkRequestMediaTypeFormat, Name = "BulkPersonRequestStatusV1")]
        [HeaderVersionRoute("/section-registrations/{guid}", 1, false, RouteConstants.HedtechIntegrationBulkRequestMediaTypeFormat, Name = "BulkSectionRegistrationsRequestStatusV1")]
        [HeaderVersionRoute("/student-academic-periods/{guid}", 1, false, RouteConstants.HedtechIntegrationBulkRequestMediaTypeFormat, Name = "BulkStudentAcademicPeriodsRequestStatusV1")]
        [HeaderVersionRoute("/student-grade-point-averages/{guid}", 1, false, RouteConstants.HedtechIntegrationBulkRequestMediaTypeFormat, Name = "BulkStudentGradePointAveragesRequestStatusV1")]
        [HeaderVersionRoute("/student-transcript-grades/{guid}", 1, false, RouteConstants.HedtechIntegrationBulkRequestMediaTypeFormat, Name = "BulkStudentTranscriptGradesRequestStatusV1")]
        public async Task<ActionResult<Dtos.BulkLoadGet>> GetBulkLoadRequestStatusAsync(QueryStringFilter criteria, [FromRoute] string guid = null)
        {

            var filter = GetFilterObject<Dtos.BulkLoadGet>(_logger, "criteria");

            if (CheckForEmptyFilterParameters())
            {
                return CreateHttpResponseException(new IntegrationApiException("Null requestorTrackingId",
                    IntegrationApiUtility.GetDefaultApiError("requestorTrackingId is a required filter.")));
            }

            var id = filter.RequestorTrackingId;

            // if the filter is empty, do we have a guid in the URL?
            if (string.IsNullOrEmpty(id))
            {
                id = guid;
            }

            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null requestorTrackingId",
                    IntegrationApiUtility.GetDefaultApiError("requestorTrackingId is a required property.")));
            }

            var bulkRepresentationRoute = _actionContextAccessor.ActionContext.ActionDescriptor.ActionConstraints.FirstOrDefault(c => c is HeaderVersionBulkRepresentationRouteAttribute) as HeaderVersionBulkRepresentationRouteAttribute;

            string permissionCode = string.Empty;
            if (bulkRepresentationRoute != null && !string.IsNullOrWhiteSpace(bulkRepresentationRoute.PermissionCode))
            {
                permissionCode = bulkRepresentationRoute.PermissionCode;
            }
            else
            {
                return CreateHttpResponseException(new IntegrationApiException("Null PermissionCode",
                    IntegrationApiUtility.GetDefaultApiError("PermissionCode is not set on route.")));
            }

            try
            {
                return await _bulkLoadRequestService.GetBulkLoadRequestStatus(GetRouteResourceName(), id, permissionCode);
            }
            catch (PermissionsException e)
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }
    }
}
