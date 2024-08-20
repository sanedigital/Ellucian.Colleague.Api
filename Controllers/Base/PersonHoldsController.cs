// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.

using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Constraints;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Ellucian.Colleague.Api.Utility;
using System.Net.Http;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http;
using Ellucian.Web.Http.ModelBinding;

using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Web.Http.Configuration;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides person holds data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PersonHoldsController : BaseCompressedApiController
    {
        private readonly IPersonHoldsService _personHoldsService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Person holds constructor
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="personHoldsService"></param>
        /// <param name="logger"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiSettings"></param>
        public PersonHoldsController(IAdapterRegistry adapterRegistry, IPersonHoldsService personHoldsService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
        {
            _adapterRegistry = adapterRegistry;
            _personHoldsService = personHoldsService;
            this._logger = logger;
        }

        #region GET Methods
        /// <summary>
        /// Returns a list of all active restrictions recorded for any person in the database
        /// </summary>
        /// <returns></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, PermissionsFilter(new string[] { PersonHoldsPermissionCodes.ViewPersonHold, PersonHoldsPermissionCodes.CreateUpdatePersonHold } )] 
        [TypeFilter(typeof(PagingFilter), Arguments =new object[] { true, 200 }), ServiceFilter(typeof(EedmResponseFilter))]
        [ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
        [HeaderVersionRoute("/person-holds", "6.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetActivePersonHolds", IsEedmSupported = true)]
        public async Task<IActionResult> GetPersonsActiveHoldsAsync(Paging page)
        {
            try
            {
                _personHoldsService.ValidatePermissions(GetPermissionsMetaData());

                var bypassCache = false;
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


                var pageOfItems = await _personHoldsService.GetPersonHoldsAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(await _personHoldsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _personHoldsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedActionResult<IEnumerable<Dtos.PersonHold>>(pageOfItems.Item1, page, pageOfItems.Item2, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);

            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }
        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves all active person holds for hold id
        /// </summary>
        /// <returns>PersonHold object for a person.</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { PersonHoldsPermissionCodes.ViewPersonHold, PersonHoldsPermissionCodes.CreateUpdatePersonHold })]
        [HeaderVersionRoute("/person-holds/{id}", "6.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetActivePersonHoldById", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PersonHold>> GetPersonsActiveHoldAsync([FromRoute] string id)
        {
            try
            {
                _personHoldsService.ValidatePermissions(GetPermissionsMetaData()); 
                var bypassCache = false;
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }
            
                var personHold = await _personHoldsService.GetPersonHoldAsync(id);

                if (personHold != null)
                {

                    AddEthosContextProperties(await _personHoldsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _personHoldsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { personHold.Id }));
                }

                return personHold;
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException e)
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

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves all active person holds for person id
        /// </summary>
        /// <returns>PersonHold object for a person.</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(new string[] { PersonHoldsPermissionCodes.ViewPersonHold, PersonHoldsPermissionCodes.CreateUpdatePersonHold })]
        [ValidateQueryStringFilter(new string[] { "person" }, false, true)]
        [QueryStringConstraint(allowOtherKeys: true, "person")]
        [HeaderVersionRoute("/person-holds", "6.1.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "DefaultGetActivePersonHoldsByPersonId", IsEedmSupported = true, Order = -20)]
        public async Task<ActionResult<IEnumerable<Dtos.PersonHold>>> GetPersonsActiveHoldsByPersonIdAsync([FromQuery] string person)
        {
            try
            {
                _personHoldsService.ValidatePermissions(GetPermissionsMetaData());

                var bypassCache = false;
                if (Request.GetTypedHeaders().CacheControl != null)
                {
                    if (Request.GetTypedHeaders().CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }
                if (person == null)
                {
                    return new List<Dtos.PersonHold>();
                }

                var personHolds = await _personHoldsService.GetPersonHoldsAsync(person, bypassCache);

                AddEthosContextProperties(await _personHoldsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _personHoldsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              personHolds.Select(a => a.Id).ToList()));

                return Ok(personHolds);
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException e)
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

        #endregion

        #region PUT method
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="personHold"></param>
        /// <returns></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(PersonHoldsPermissionCodes.CreateUpdatePersonHold)]
        [HttpPut]
        [HeaderVersionRoute("/person-holds/{id}", "6.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PutActivePersonHoldV6", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PersonHold>> PutPersonHoldAsync([FromRoute] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.PersonHold personHold)
        {
            try
            {
                _personHoldsService.ValidatePermissions(GetPermissionsMetaData());
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("Person hold id cannot be null or empty");
                }

                if (personHold == null)
                {
                    throw new ArgumentNullException("personHold cannot be null or empty");
                }

                if (!id.Equals(personHold.Id, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("id does not match id in personHold.");
                }

                if (id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Nil GUID cannot be used in PUT operation.");
                }

                if (string.IsNullOrEmpty(personHold.Id))
                {
                    personHold.Id = id.ToUpperInvariant();
                }

                if (!string.IsNullOrEmpty(personHold.Reason))
                {
                    throw new IntegrationApiException("reason cannot be updated",
                                    IntegrationApiUtility.GetDefaultApiError("reason is not supported in Colleague."));
                }

                //get Data Privacy List
                var dpList = await _personHoldsService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _personHoldsService.ImportExtendedEthosData(await ExtractExtendedData(await _personHoldsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic
                var personHoldReturn = await _personHoldsService.UpdatePersonHoldAsync(id,
                    await PerformPartialPayloadMerge(personHold, async () => await _personHoldsService.GetPersonHoldAsync(id),
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _personHoldsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { id }));

                return personHoldReturn; 

            }
            catch (ArgumentNullException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
            catch (InvalidOperationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
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
        #endregion

        #region POST method
        /// <summary>
        /// Create new person hold
        /// </summary>
        /// <param name="personHold">personHold</param>
        /// <returns></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost, ServiceFilter(typeof(EedmResponseFilter)), PermissionsFilter(PersonHoldsPermissionCodes.CreateUpdatePersonHold)]
        [HttpPost]
        [HeaderVersionRoute("/person-holds", "6.1.0", false, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "PostActivePersonHoldV6", IsEedmSupported = true)]
        public async Task<ActionResult<Dtos.PersonHold>> PostPersonHoldAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.PersonHold personHold)
        {
             if (personHold == null)
             {
                return CreateHttpResponseException(new IntegrationApiException("Null PersonHolds argument",
                IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
             }
            try
            {
                if (!string.IsNullOrEmpty(personHold.Reason))
                {
                    throw new IntegrationApiException("reason cannot be updated",
                        IntegrationApiUtility.GetDefaultApiError("reason is not supported in Colleague."));
                }

                _personHoldsService.ValidatePermissions(GetPermissionsMetaData());
                if (string.IsNullOrEmpty(personHold.Id))
                {
                    throw new ArgumentNullException("Null personHold id", "Id is a required property.");
                }

                if (personHold.Id != Guid.Empty.ToString())
                {
                    throw new ArgumentNullException("personHoldsDto", "On a post you can not define a GUID");
                }
                //call import extend method that needs the extracted extension data and the config
                await _personHoldsService.ImportExtendedEthosData(await ExtractExtendedData(await _personHoldsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the person hold
                var personHoldCreate = await _personHoldsService.CreatePersonHoldAsync(personHold);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _personHoldsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _personHoldsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { personHoldCreate.Id }));

                return personHoldCreate;
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
            catch (ArgumentNullException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
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
        #endregion

        #region DELETE method
        /// <summary>
        /// Deletes person hold based on id
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpDelete, PermissionsFilter(PersonHoldsPermissionCodes.DeletePersonHold)]
        [Route("/person-holds/{id}", Name = "DeleteActivePersonHold", Order = -10)]
        public async Task<IActionResult> DeletePersonHoldAsync(string id)
        {
            try
            {
                _personHoldsService.ValidatePermissions(GetPermissionsMetaData());
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("Person hold id cannot be null or empty");
                }
                await _personHoldsService.DeletePersonHoldAsync(id);
                return NoContent();
            }
            catch (ArgumentNullException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (PermissionsException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (IntegrationApiException e)
            {
                _logger.LogError(e.ToString());
                return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
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
        #endregion
    }
}
