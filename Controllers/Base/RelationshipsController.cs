// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using Ellucian.Web.Security;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to Relationship data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class RelationshipsController : BaseCompressedApiController
    {
        private IRelationshipService _relationshipService;
        private ILogger _logger;
        private const string permissionExceptionMessage = "User does not have permission to access the requested information";
        private const string unexpectedGenericErrorMessage = "Unexpected error occurred while processing the request.";
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
        private const string invalidArgumentException = "Invalid argument";
        /// <summary>
        /// Instantiates a RelationshipController
        /// </summary>
        /// <param name="relationshipService">a relationship service of type <see cref="IRelationshipService"/></param>
        /// <param name="logger">a logging service of type <see cref="ILogger"/></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public RelationshipsController(IRelationshipService relationshipService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _relationshipService = relationshipService;
            this._logger = logger;
        }

        /// <summary>
        /// Gets the primary relationships for a person or organization
        /// </summary>
        /// <param name="personId">The identifier of the person of interest</param>
        /// <returns>An enumeration of the person's primary relationship with other persons or organizations.</returns>
        /// <accessComments>
        /// Any logged in user can get their own primary relationships.
        /// A user with the permission ADD.ALL.HR.PROXY is considered as an admin and can access the primary relationship info of any employee.
        /// </accessComments>
        [HttpGet]
        [HeaderVersionRoute("/persons/{personId}/relationships", 1, true, Name = "GetPersonPrimaryRelationships")]
        public async Task<ActionResult<IEnumerable<Relationship>>> GetPersonPrimaryRelationshipsAsync(string personId)
        {
            try
            {
                return Ok(await _relationshipService.GetPersonPrimaryRelationshipsAsync(personId));
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (ArgumentNullException anex)
            {
                _logger.LogError(anex.ToString());
                return CreateHttpResponseException(invalidArgumentException, HttpStatusCode.BadRequest);
            }
            catch (PermissionsException pex)
            {
                _logger.LogError(pex.ToString());
                return CreateHttpResponseException(permissionExceptionMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.ToString());
                return CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Creates the given relationship type between the two given entities
        /// </summary>
        /// <param name="relationship">The <see cref="Relationship"/> to be created</param>
        /// <returns>The created <see cref="Relationship"/></returns>
        /// <accessComments>
        /// Only the current user can create their own relationship.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/persons/{personId}/relationships", 1, true, Name = "PostRelationships")]
        public async Task<ActionResult<Relationship>> PostRelationshipAsync([FromBody] Relationship relationship)
        {
            try 
            {
                return await _relationshipService.PostRelationshipAsync(relationship.OtherEntity, relationship.RelationshipType, relationship.PrimaryEntity);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.LogError(csse, csse.Message);
                return CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException)
            {
                return CreateHttpResponseException(permissionExceptionMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.ToString());
                return CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }
    }
}
