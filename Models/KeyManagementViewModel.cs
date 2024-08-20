// Copyright 2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Options;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Ellucian.Colleague.Api.Models
{
    /// <summary>
    /// Used for key management page
    /// </summary>
    public class KeyManagementViewModel
    {
        /// <summary>
        /// Available approaches for key management
        /// </summary>
        public IEnumerable<string> KeyStrategies { get; set; }
        /// <summary>
        /// The strategy for the key.
        /// </summary>
        public string KeyStrategy { get; set; }
        /// <summary>
        /// The path to the key repository. Ideally, a UNC path to support web farms.
        /// </summary>
        public string KeyPath { get; set; }
        /// <summary>
        /// The fixed key to use. Changing this means the application should re-protect the already protected values.
        /// </summary>
        public string FixedKey { get; set; }

        /// <summary>
        /// Used for validation or other issues.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// We need to redirect to a confirmation URL because the app will restart.
        /// </summary>
        public string ConfirmationUrl { get; set; }
    }
}
