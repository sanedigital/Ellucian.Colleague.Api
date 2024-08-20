// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web;


namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to organizational position relationships
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class OrganizationalPositionRelationshipsController : BaseCompressedApiController
    {
        private IOrganizationalPositionRelationshipService _organizationalPositionRelationshipService;

        private ILogger _logger;

        /// <summary>
        /// OrganizationalPositionRelationshipsController constructor
        /// </summary>
        public OrganizationalPositionRelationshipsController(IOrganizationalPositionRelationshipService organizationalPositionRelationshipService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _organizationalPositionRelationshipService = organizationalPositionRelationshipService;
            this._logger = logger;

        }

        /// <summary>
        /// Create organizational position relationship
        /// </summary>
        /// <param name="organizationalPositionRelationship">The organizational position relationship</param>
        /// <returns>The new organizational position relationship</returns>
        /// <accessComments>
        /// Users with the following permission codes can create organizational position relationships:
        /// 
        /// UPDATE.ORGANIZATIONAL.RELATIONSHIPS
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/organizational-position-relationships", 1, true, Name = "CreateOrganizationalPositionRelationship")]
        public async Task<ActionResult<Dtos.Base.OrganizationalPositionRelationship>> CreateOrganizationalPositionRelationshipAsync([FromBody] Dtos.Base.OrganizationalPositionRelationship organizationalPositionRelationship)
        {
            try
            {
                return await _organizationalPositionRelationshipService.AddAsync(organizationalPositionRelationship);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Delete an organizational position relationship
        /// </summary>
        /// <param name="id">Organizational position relationship ID to delete</param>
        /// <accessComments>
        /// Users with the following permission codes can delete organizational position relationships:
        /// 
        /// UPDATE.ORGANIZATIONAL.RELATIONSHIPS
        /// </accessComments>
        [HttpDelete]
        [HeaderVersionRoute("/organizational-position-relationships/{id}", 1, true, Name = "DeleteOrganizationalPositionRelationship")]
        public async Task<IActionResult> DeleteOrganizationalPositionRelationshipAsync(string id)
        {
            try
            {
                await _organizationalPositionRelationshipService.DeleteAsync(id);
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
