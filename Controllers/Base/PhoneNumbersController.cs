// Copyright 2012-2024 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;


namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Address data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PhoneNumbersController : BaseCompressedApiController
    {
        private readonly IPhoneNumberRepository _phoneNumberRepository;
        private readonly IPhoneNumberService _phoneNumberService;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PhoneNumbersController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="phoneNumberRepository">Repository of type <see cref="IPhoneNumberRepository">IPhoneNumberRepository</see></param>        
        /// <param name="configurationRepository">Repository of type<see cref="IConfigurationRepository">IConfigurationRepository</see></param>   
        /// <param name="phoneNumberService">Service of type <see cref="IPhoneNumberService">IPhoneNumberService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PhoneNumbersController(IAdapterRegistry adapterRegistry, IPhoneNumberRepository phoneNumberRepository, IConfigurationRepository configurationRepository, IPhoneNumberService phoneNumberService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _phoneNumberRepository = phoneNumberRepository;
            _configurationRepository = configurationRepository;
            _phoneNumberService = phoneNumberService;
            this._logger = logger;
        }
        /// <summary>
        /// Get all current phone numbers for a person
        /// </summary>
        /// <param name="personId">Person to get phone numbers for</param>
        /// <returns>PhoneNumber Object <see cref="Ellucian.Colleague.Dtos.Base.PhoneNumber">PhoneNumber</see></returns>
        /// <accessComments>Authenticated users can retrieve their own phone numbers or users with the VIEW.PERSON.INFORMATION permission can retrieve phone numbers for others.</accessComments>
        [HttpGet]
        [HeaderVersionRoute("/phone-numbers/{personId}", 1, true, Name = "GetPhoneNumbersByPersonIdAsync")]
        public async Task<ActionResult<Ellucian.Colleague.Dtos.Base.PhoneNumber>> GetPersonPhonesAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                _logger.LogError("Invalid personId parameter");
                return CreateHttpResponseException("The personId is required.", HttpStatusCode.BadRequest);
            }
            try
            {
                var phoneNumberDtoCollection = await _phoneNumberService.GetPersonPhones2Async(personId);
                return phoneNumberDtoCollection;
            }
            catch (PermissionsException pex)
            {
                return CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                string message = "Session has expired while retrieving phone numbers";
                _logger.LogError(csee, message);
                return CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                return CreateHttpResponseException(e.Message);
            }
        }
        /// <summary>
        /// Get a list of phone numbers from a list of Person keys
        /// </summary>
        /// <param name="criteria">Selection Criteria including PersonIds list.</param>
        /// <returns>List of Phone Number Objects <see cref="Ellucian.Colleague.Dtos.Base.PhoneNumber">PhoneNumber</see></returns>
        /// <accessComments>
        /// Authenticated users can query their own phone numbers only. 
        /// Users with the QUERY.PHONE.NUMBERS permission or the VIEW.PERSON.INFORMATION permission can query phone numbers for others.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/phone-numbers", 1, true, Name = "GetPhoneNumbersByIdList")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Base.PhoneNumber>>> QueryPhoneNumbersAsync(PhoneNumberQueryCriteria criteria)
        {
            if (criteria == null || criteria.PersonIds == null || criteria.PersonIds.Count() == 0)
            {
                _logger.LogError("Invalid personIds parameter: null or empty.");
                return CreateHttpResponseException("No person IDs provided.", HttpStatusCode.BadRequest);
            }
            try
            {
                var phoneDtoCollection = await _phoneNumberService.QueryPhoneNumbersAsync(criteria);
                return Ok(phoneDtoCollection);
            }
            catch (PermissionsException pex)
            {
                var permissionsMessage = "You do not have permission to query phone numbers.";
                _logger.LogError(pex, permissionsMessage);
                return CreateHttpResponseException(permissionsMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "QueryPhoneNumbers error");
                return CreateHttpResponseException(e.Message);
            }
        }

        /// <summary>
        /// Get a list of phone numbers from a list of Pilot Person keys
        /// </summary>
        /// <param name="criteria">Selection Criteria including PersonIds list.</param>
        /// <returns>List of Phone Number Objects <see cref="Ellucian.Colleague.Dtos.Base.PhoneNumber">PhoneNumber</see></returns>
        /// <accessComments>
        /// Authenticated users can query their own phone numbers only. Users with the QUERY.PHONE.NUMBERS permission can query phone numbers for anyone.
        /// </accessComments>
        [HttpPost]
        [HeaderVersionRoute("/qapi/phone-numbers", 1, false, RouteConstants.EllucianJsonPilotMediaTypeFormat, Name = "GetPilotPhoneNumbersByIdList")]
        public async Task<ActionResult<IEnumerable<Ellucian.Colleague.Dtos.Base.PilotPhoneNumber>>> QueryPilotPhoneNumbersAsync(PhoneNumberQueryCriteria criteria)
        {
            if (criteria == null || criteria.PersonIds == null || criteria.PersonIds.Count() == 0)
            {
                _logger.LogError("Invalid personIds parameter: null or empty.");
                return CreateHttpResponseException("No person IDs provided.", HttpStatusCode.BadRequest);
            }
            try
            {
                var pilotPhoneDtoCollection = await _phoneNumberService.QueryPilotPhoneNumbersAsync(criteria);
                return Ok(pilotPhoneDtoCollection);
            }
            catch (PermissionsException pex)
            {
                var permissionsMessage = "You do not have permission to query phone numbers.";
                _logger.LogError(pex, permissionsMessage);
                return CreateHttpResponseException(permissionsMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "QueryPhoneNumbers error");
                return CreateHttpResponseException(e.Message);
            }
        }
    }
}
