// Copyright 2023 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ellucian.Colleague.Api.Models
{
    /// <summary>
    /// API settings profile model
    /// </summary>
    public class ApiSettingsProfileModel
    {
        /// <summary>
        /// Gets or sets the current profile name.
        /// </summary>
        public string CurrentProfileName { get; set; }

        /// <summary>
        /// Gets or sets the new profile name.
        /// </summary>
        public string NewProfileName { get; set; }

        /// <summary>
        /// Gets or sets a list of all existing profile names.
        /// </summary>
        public IEnumerable<SelectListItem> ExistingProfileNames { get; set; }

        /// <summary>
        /// Gets or set the selected <see cref="SelectListItem"/>.
        /// </summary>
        public SelectListItem SelectedExistingProfileName { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ApiSettingsProfileModel()
        {
            ExistingProfileNames = new List<SelectListItem>();
            SelectedExistingProfileName = new SelectListItem();
        }
    }
}
