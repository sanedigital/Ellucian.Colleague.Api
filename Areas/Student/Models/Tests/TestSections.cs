// Copyright 2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Ellucian.Colleague.Api.Areas.Student.Models.Tests
{
    /// <summary>
    /// Test sections model.
    /// </summary>
    public class TestSections
    {
        /// <summary>
        /// Gets or sets the section ids.
        /// </summary>
        [DisplayName("Section IDs, comma or space delimited")]
        [Required]
        public string SectionIds { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the cache should be used.
        /// </summary>
        [DisplayName("Use section cache")]
        public bool FromCache { get; set; }
    }
}
