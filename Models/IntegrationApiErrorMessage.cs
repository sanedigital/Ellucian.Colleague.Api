// Copyright 2014-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace Ellucian.Colleague.Api.Models
{
    /// <summary>
    /// A standard error message for the Integration API
    /// </summary>
    public class IntegrationApiErrorMessage
    {
        /// <summary>
        /// Standard error code, as defined in the CDM
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Standard error description, corresponding to the Code
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// HTTP status code for this error
        /// </summary>
        public HttpStatusCode ReturnCode { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="code">Error code</param>
        /// <param name="description">Error description</param>
        /// <param name="returnCode">HTTP status code</param>
        public IntegrationApiErrorMessage(string code, string description, HttpStatusCode returnCode)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code");
            }
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException("description");
            }
            if (!HttpStatusCode.IsDefined(typeof(HttpStatusCode), returnCode))
            {
                throw new ArgumentOutOfRangeException("returnCode");
            }

            Code = code.TrimEnd();
            Description = description.TrimEnd();
            ReturnCode = returnCode;
        }

        /// <summary>
        /// Overload constructor
        /// </summary>
        /// <param name="code">Error code</param>
        /// <param name="description">Error description</param>
        /// <param name="returnCode">Integer value of HTTP return code</param>
        public IntegrationApiErrorMessage(string code, string description, int returnCode)
            : this(code, description, (HttpStatusCode)returnCode)
        {
        }
    }
}
