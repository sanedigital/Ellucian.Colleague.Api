// Copyright 2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ellucian.Colleague.Api.Models
{
    /// <summary>
    /// API tests model.
    /// </summary>
    public class ApiTests
    {
        /// <summary>
        /// Gets or sets the lists of tests.
        /// </summary>
        public List<string> Tests { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ApiTests()
        {
            Tests = new List<string>();
        }
    }
}
