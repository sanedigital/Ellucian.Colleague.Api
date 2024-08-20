// Copyright 2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Ellucian.Colleague.Api.Models
{
    /// <summary>
    /// Test login model.
    /// </summary>
    public class TestLogin
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        [Required]
        [DisplayName("User ID")]
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [DisplayName("Password")]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the base API URL.
        /// </summary>
        [DisplayName("Colleague Web API Base URL")]
        public string BaseUrl { get; set; }
    }
}
