// Copyright 2023 Ellucian Company L.P. and its affiliates.
namespace Ellucian.Colleague.Api.Options
{
	/// <summary>
	/// Allows for programmatic setting of cookie policy.
	/// </summary>
	public class CookiePolicySettings
	{
		/// <summary>
		/// The cookie secure policy. Affects if cookies require HTTPS or only HTTP. Options: <see cref="CookieSecurePolicy"/>
		/// </summary>
		public CookieSecurePolicy CookieSecurePolicy { get; set; } = CookieSecurePolicy.SameAsRequest;
	}
}
