// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;


namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to organizational relationships
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class OrganizationalRelationshipsController : BaseCompressedApiController
    {
        private readonly IOrganizationalPersonPositionService _organizationalPersonPositionService;
        private readonly IOrganizationalRelationshipService _organizationalRelationshipService;
        private readonly ILogger _logger;

        /// <summary>
        /// OrganizationalRelationshipsController constructor
        /// </summary>
        /// <param name="organizationalPersonPositionService">Service of type <see cref="IOrganizationalPersonPositionService">IOrganizationalPersonPositionService</see></param>
        /// <param name="organizationalRelationshipService">Service of type <see cref="IOrganizationalRelationshipService">IOrganizationalRelationshipService</see></param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public OrganizationalRelationshipsController(IOrganizationalPersonPositionService organizationalPersonPositionService, IOrganizationalRelationshipService organizationalRelationshipService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _organizationalPersonPositionService = organizationalPersonPositionService;
            _organizationalRelationshipService = organizationalRelationshipService;
            this._logger = logger;
        }

        /// <summary>
        /// Create organizational relationship
        /// </summary>
        /// <param name="organizationalRelationship">The organizational relationship</param>
        /// <returns>The new organizational relationship</returns>
        /// <accessComments>
        /// Users with the following permission codes can create organizational relationships:
        /// 
        /// UPDATE.ORGANIZATIONAL.RELATIONSHIPS
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/organizational-relationships", 1, true, Name = "CreateOrganizationalRelationship")]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.Base.OrganizationalRelationship>> CreateOrganizationalRelationshipAsync([FromBody] Ellucian.Colleague.Dtos.Base.OrganizationalRelationship organizationalRelationship)
        {
            try
            {
                return await _organizationalRelationshipService.AddAsync(organizationalRelationship);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Update organizational relationship
        /// </summary>
        /// <param name="organizationalRelationship">The organizational relationship</param>
        /// <returns>The updated organizational relationship</returns>
        /// <accessComments>
        /// Users with the following permission codes can update organizational relationships:
        /// 
        /// UPDATE.ORGANIZATIONAL.RELATIONSHIPS
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/organizational-relationships/{id}", 1, true, Name = "UpdateOrganizationalRelationship")]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.Base.OrganizationalRelationship>> UpdateOrganizationalRelationshipAsync([FromBody] Ellucian.Colleague.Dtos.Base.OrganizationalRelationship organizationalRelationship)
        {
            try
            {
                return await _organizationalRelationshipService.UpdateAsync(organizationalRelationship);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Delete an organizational relationship
        /// </summary>
        /// <param name="id">Organizational relationship ID to delete</param>
        /// <accessComments>
        /// Users with the following permission codes can delete organizational relationships:
        /// 
        /// UPDATE.ORGANIZATIONAL.RELATIONSHIPS
        /// </accessComments>
        [HttpDelete]
        [HeaderVersionRoute("/organizational-relationships/{id}", 1, true, Name = "DeleteOrganizationalRelationship")]
        public async Task<IActionResult> DeleteOrganizationalRelationshipAsync(string id)
        {
            try
            {
                await _organizationalRelationshipService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }
    }
}
