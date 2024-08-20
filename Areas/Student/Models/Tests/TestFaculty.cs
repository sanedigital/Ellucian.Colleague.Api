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
    /// Test faculty model
    /// </summary>
    public class TestFaculty
    {

        /// <summary>
        /// Gets or sets the faculty ids.
        /// </summary>
        [DisplayName("Faculty IDs, comma or space delimited")]
        [Required]
        public string FacultyIds { get; set; }
    }
}
