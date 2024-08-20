// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Routes;
using Ellucian.Web.Http.ModelBinding;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Ellucian.Web.Http.Configuration;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Comments
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class CommentsController : BaseCompressedApiController
    {
        private readonly ICommentsService _commentsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the CommentsController class.
        /// </summary>
        /// <param name="commentsService">Service of type <see cref="ICommentsService">ICommentsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public CommentsController(ICommentsService commentsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _commentsService = commentsService;
            this._logger = logger;
        }

        /// <summary>
        /// Return all remarks found in the REMARKS file in Colleague using filter
        /// </summary>
        /// <param name="subjectMatter">find all of the records in REMARKS where the REMARKS.DONOR.ID matches the person or organization ID corresponding to the guid found in subjectMatter.person.id or subjectMatter.organization.id.</param>
        /// <param name="commentSubjectArea">find all of the records in REMARKS where the REMARKS.TYPE matches the code corresponding to the guid in commentSubjectArea.id.</param>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <returns>List of Comments <see cref="Dtos.Comments"/> objects representing matching comments</returns>
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true }), PermissionsFilter(new string[] { BasePermissionCodes.ViewComment, BasePermissionCodes.UpdateComment })]
        [ValidateQueryStringFilter(new string[] { "subjectMatter", "commentSubjectArea" }, false, true)]
        [TypeFilter(typeof(PagingFilter), Arguments = new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
        [HttpGet]
        [HeaderVersionRoute("/comments", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetCommentsDefault", IsEedmSupported = true)]
        public async Task<IActionResult> GetCommentsAsync(Paging page, [FromQuery] string subjectMatter = "", [FromQuery] string commentSubjectArea = "")
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (subjectMatter == null || commentSubjectArea == null)
            {
                return new PagedActionResult<IEnumerable<Dtos.Comments>>(new List<Dtos.Comments>(), page, 0, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
            }

            try
            {
                _commentsService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var pageOfItems = await _commentsService.GetCommentsAsync(page.Offset, page.Limit, subjectMatter, commentSubjectArea, bypassCache);

                AddEthosContextProperties(await _commentsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _commentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.Comments>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

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
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Read (GET) a comment using a GUID
        /// </summary>
        /// <param name="id">GUID to desired comment</param>
        /// <returns>A comment object <see cref="Dtos.Comments"/> in HEDM format</returns>
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { BasePermissionCodes.ViewComment, BasePermissionCodes.UpdateComment })]
        [HeaderVersionRoute("/comments/{id}", 6, true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetCommentsByGuidDefault", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Comments>> GetCommentsByGuidAsync(string id)
        {
            var bypassCache = false;
            if (Request.GetTypedHeaders().CacheControl != null)
            {
                if (Request.GetTypedHeaders().CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                _commentsService.ValidatePermissions(GetPermissionsMetaData());
                var comment = await _commentsService.GetCommentByIdAsync(id);

                if (comment != null)
                {

                    AddEthosContextProperties(await _commentsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _commentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { comment.Id }));
                }


                return comment;

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
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create (POST) a new comment
        /// </summary>
        /// <param name="comment">DTO of the new comment</param>
        /// <returns>A comment object <see cref="Dtos.Comments"/> in HEDM format</returns>
        [HttpPost, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(BasePermissionCodes.UpdateComment)]
        [HeaderVersionRoute("/comments", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostCommentsV6", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Comments>> PostCommentsAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.Comments comment)
        {
            if (comment == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null comment argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            try
            {
                _commentsService.ValidatePermissions(GetPermissionsMetaData());
                //call import extend method that needs the extracted extension data and the config
                await _commentsService.ImportExtendedEthosData(await ExtractExtendedData(await _commentsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the comment
                var commentCreate = await _commentsService.PostCommentAsync(comment);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _commentsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _commentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { commentCreate.Id }));

                return commentCreate;
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
        /// Update (PUT) an existing comment
        /// </summary>
        /// <param name="id">GUID of the comment to update</param>
        /// <param name="comment">DTO of the updated comment</param>
        /// <returns>A comment object <see cref="Dtos.Comments"/> in HEDM format</returns>
        [HttpPut, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(BasePermissionCodes.UpdateComment)]
        [HeaderVersionRoute("/comments/{id}", 6, false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutCommentsV6", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.Comments>> PutCommentsAsync([FromRoute] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.Comments comment)
        {
            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (comment == null)
            {
                return CreateHttpResponseException(new IntegrationApiException("Null comment argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(comment.Id))
            {
                comment.Id = id.ToLowerInvariant();
            }
            else if ((string.Equals(id, Guid.Empty.ToString())) || (string.Equals(comment.Id, Guid.Empty.ToString())))
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID empty",
                    IntegrationApiUtility.GetDefaultApiError("GUID must be specified.")));
            }
            else if (id.ToLowerInvariant() != comment.Id.ToLowerInvariant())
            {
                return CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                _commentsService.ValidatePermissions(GetPermissionsMetaData());
                //get Data Privacy List
                var dpList = await _commentsService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _commentsService.ImportExtendedEthosData(await ExtractExtendedData(await _commentsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic
                var commentReturn = await _commentsService.PutCommentAsync(id,
                    await PerformPartialPayloadMerge(comment, async () => await _commentsService.GetCommentByIdAsync(id),
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _commentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { id }));

                return commentReturn;
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
        /// Delete (DELETE) a comment
        /// </summary>
        /// <param name="id">GUID to desired comment</param>
        [HttpDelete, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), PermissionsFilter(BasePermissionCodes.DeleteComment)]
        [Route("/comments/{id}", Name = "DeleteCommentByGuid", Order = -10)]
        public async Task<IActionResult> DeleteCommentByGuidAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                _commentsService.ValidatePermissions(GetPermissionsMetaData());
                await _commentsService.DeleteCommentByIdAsync(id);
                // On delete, just return nothing.
                return NoContent();
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
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }
    }
}
