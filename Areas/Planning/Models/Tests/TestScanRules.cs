// Copyright 2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Api.Areas.Planning.Models.Tests
{
    /// <summary>
    /// Test scan rules model.
    /// </summary>
    public class TestScanRules
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// gets or sets the number of supported rules.
        /// </summary>
        public int Supported { get; set; }

        /// <summary>
        /// Gets or sets the total number of rules.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// List of rules that will not be supported.
        /// </summary>
        public List<Rule> NotSupportedNames = new List<Rule>();

        /// <summary>
        /// Gets or sets the log output.
        /// </summary>
        public string Log { get; set; }
    }
}
