// Copyright 2023-2024 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.EUP;
using Ellucian.Colleague.Dtos.Filters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Text.RegularExpressions;

namespace Ellucian.Colleague.Api.Controllers.Base
{
	/// <summary>
	/// Provids access to user identity profile data.
	/// </summary>
	[Authorize]
	[LicenseProvider(typeof(EllucianLicenseProvider))]
	[EllucianLicenseModule(ModuleConstants.Base)]
	public class UserIdentityProfilesController : BaseCompressedApiController
	{
		private readonly IUserIdentityProfileService _userIdentityProfileService;
		private readonly ILogger _logger;
		private readonly Regex guidRegex = new("^[a-f0-9]{8}(?:-[a-f0-9]{4}){3}-[a-f0-9]{12}$");

		/// <summary>
		/// Initializes a new instance of the <see cref="UserIdentityProfilesController"/> class.
		/// </summary>
		/// <param name="userIdentityProfileService">User Identity Profile Service</param>
		/// <param name="logger"></param>
		/// <param name="actionContextAccessor"></param>
		/// <param name="apiSettings"></param>
		public UserIdentityProfilesController(IUserIdentityProfileService userIdentityProfileService, ILogger logger, IActionContextAccessor actionContextAccessor, ApiSettings apiSettings) : base(actionContextAccessor, apiSettings)
		{
			_userIdentityProfileService = userIdentityProfileService;
			_logger = logger;
		}

		/// <summary>
		/// Returns a list of user identity profiles base on person filter criteria.
		/// </summary>
		/// <param name="page">User identity profile page to retrieve.</param>
		/// <param name="personFilter">Person filter search criteria.</param>
		/// <returns>List of <see cref="UserIdentityProfile"/> objects for person filter.</returns>
		[HttpGet, PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyPerson })]
		[Metadata(HttpMethodPermission = BasePermissionCodes.ViewAnyPerson)]
		[ValidateQueryStringFilter(), TypeFilter(typeof(FilteringFilter), Arguments = new object[] { true })]
		[QueryStringFilterFilter("personFilter", typeof(PersonFilterFilter2))]
		[TypeFilter(typeof(PagingFilter), Arguments = new object[] { true, 100 }), ServiceFilter(typeof(EedmResponseFilter))]
		[HeaderVersionRoute("/user-identity-profiles", "2.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetUserIdentityProfiles", IsEedmSupported = true)]
		public async Task<IActionResult> GetUserIdentityProfilesAsync(Paging page, QueryStringFilter personFilter)
		{
			try
			{
				_userIdentityProfileService.ValidatePermissions(GetPermissionsMetaData());
				var bypassCache = Request.GetTypedHeaders().CacheControl?.NoCache ?? false;

				page ??= new Paging(100, 0);

				if (CheckForEmptyFilterParameters())
					return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("Invalid person filter parameter.")), HttpStatusCode.BadRequest);

				var personFilterObj = GetFilterObject<PersonFilterFilter2>(_logger, "personFilter");

				if (!string.IsNullOrWhiteSpace(personFilterObj.personFilter?.Id) && !guidRegex.IsMatch(personFilterObj.personFilter.Id))
					return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("Person filter id is not a valid guid.")), HttpStatusCode.BadRequest);

				var (userIdentityProfiles, count) = await _userIdentityProfileService.GetUserIdentityProfilesAsync(page.Limit, page.Offset, bypassCache, personFilterObj.personFilter?.Id);

				AddEthosContextProperties(
					await _userIdentityProfileService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
					await _userIdentityProfileService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), userIdentityProfiles.Select(i => i.Id)));

				return new PagedActionResult<IEnumerable<UserIdentityProfile>>(userIdentityProfiles, page, count, HttpStatusCode.OK, _apiSettings.IncludeLinkSelfHeaders);
			}
			catch (PermissionsException ex)
			{
				_logger.LogError(ex.ToString());
				return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex), HttpStatusCode.Forbidden);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.ToString());
				return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex), HttpStatusCode.InternalServerError);
			}
		}

		/// <summary>
		/// Returns the requested resource for given guid
		/// </summary>
		/// <param name="id">A global identifier of User Identity Profiles for use in all external references</param>
		/// <returns>A <see cref="UserIdentityProfile"/> objects for person with given guid</returns>
		[HttpGet, PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyPerson })]
		[Metadata(HttpMethodPermission = BasePermissionCodes.ViewAnyPerson)]
		[ServiceFilter(typeof(EedmResponseFilter))]
		[HeaderVersionRoute("/user-identity-profiles/{id}", "2.0.0", true, RouteConstants.HedtechIntegrationMediaTypeFormat, Name = "GetUserIdentityProfile", IsEedmSupported = true)]
		public async Task<ActionResult<UserIdentityProfile>> GetUserIdentityProfileAsync([FromRoute] string id)
		{
			_userIdentityProfileService.ValidatePermissions(GetPermissionsMetaData());
			try
			{
				if (string.IsNullOrWhiteSpace(id) || !guidRegex.IsMatch(id))
					return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("Invalid guid.")), HttpStatusCode.BadRequest);

				var bypassCache = Request.GetTypedHeaders().CacheControl?.NoCache ?? false;

				AddEthosContextProperties(
					await _userIdentityProfileService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
					await _userIdentityProfileService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string> { id }));

				var userIdentityProfile = await _userIdentityProfileService.GetUserIdentityProfileAsync(id);

				return userIdentityProfile;
			}
			catch (ArgumentOutOfRangeException ex)
			{
				_logger.LogError(ex.ToString());
				return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex), HttpStatusCode.NotFound);
			}
			catch (PermissionsException ex)
			{
				_logger.LogError(ex.ToString());
				return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex), HttpStatusCode.Forbidden);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.ToString());
				return CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex), HttpStatusCode.InternalServerError);
			}
		}
	}
}
