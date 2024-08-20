// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Citizen Type data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PersonAgreementsController : BaseCompressedApiController
    {
        private readonly IPersonAgreementsService _personAgreementsService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PersonAgreementsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="personAgreementsService">Service of type <see cref="IPersonAgreementsService">IPersonAgreementsService</see></param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PersonAgreementsController(IAdapterRegistry adapterRegistry, IPersonAgreementsService personAgreementsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _personAgreementsService = personAgreementsService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieve person agreements using person agreement query criteria
        /// </summary>
        /// <param name="criteria">Query criteria for retrieving person agreements</param>
        /// <returns>Collection of person agreements for a given person</returns>
        /// <accessComments>Authenticated users can only retrieve their own person agreement information.</accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/person-agreements", 1, true, Name = "QueryPersonAgreementsByPostAsync")]
        public async Task<ActionResult<IEnumerable<PersonAgreement>>> QueryPersonAgreementsByPostAsync([FromBody] PersonAgreementQueryCriteria criteria)
        {
            if (criteria == null || string.IsNullOrEmpty(criteria.PersonId))
            {
                throw new ArgumentNullException("id", "A person ID is required to retrieve person agreements by person ID.");
            }
            try
            {                
                return Ok(await _personAgreementsService.GetPersonAgreementsByPersonIdAsync(criteria.PersonId));
            }
            catch (PermissionsException pe)
            {
                string message = "Users can only retrieve their own person agreement information.";
                _logger.LogError(pe, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                string message = string.Format("Person agreements data for person {0} could not be retrieved.", criteria.PersonId);
                _logger.LogError(e, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Updates a <see cref="PersonAgreement">person agreement</see>. Users can only update the status and the date and time that action was taken on the person agreement.
        /// The person agreement description and text must exactly match the information stored in the database of record; if the description or text does not match then the update will be rejected.
        /// </summary>
        /// <param name="agreement">The <see cref="PersonAgreement">person agreement</see> to update</param>
        /// <returns>An updated <see cref="PersonAgreement">person agreement</see></returns>
        /// <accessComments>Authenticated users can only update their own person agreements.</accessComments>
        [HttpPut]
        [HeaderVersionRoute("/person-agreements", 1, true, Name = "UpdatePersonAgreementAsync")]
        public async Task<ActionResult<PersonAgreement>> UpdatePersonAgreementAsync([FromBody] PersonAgreement agreement)
        {
            if (agreement == null)
            {
                throw new ArgumentNullException("agreement", "A person agreement is required when updating a person agreement.");
            }
            try
            {
                return await _personAgreementsService.UpdatePersonAgreementAsync(agreement);
            }
            catch (PermissionsException pe)
            {
                string message = "Users can only update their own person agreement information.";
                _logger.LogError(pe, message);
                return CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                string message = string.Format("An error occurred while updating person agreement {0}.", agreement.Id);
                _logger.LogError(e, message);
                return CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }
    }
}
