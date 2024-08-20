// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Ellucian.Colleague.Api.Models
{
    /// <summary>
    /// Object that reflects the data in a single resource file. 
    /// Includes an object to retain the original value and to keep track of changes made to the entry.
    /// </summary>
    [Serializable]
    public class ResourceFileEntryModel
    {
        /// <summary>
        /// Gets or sets the value of the key of the resource file entry.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        [Required]
        public String Key { get; set; }
        /// <summary>
        /// Gets or sets the value of the resource file entry.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        [Required]
        public String Value { get; set; }
        /// <summary>
        /// Gets or sets the comment of the resource file entry.
        /// </summary>
        /// <value>
        /// The comment.
        /// </value>
        public String Comment { get; set; }
        /// <summary>
        /// Gets or sets the original value to keep track of modified values.
        /// </summary>
        /// <value>
        /// The original value.
        /// </value>
        public String OriginalValue { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceFileEntryModel"/> class.
        /// </summary>
        public ResourceFileEntryModel() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceFileEntryModel"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public ResourceFileEntryModel(String key, String value) { Key = key; Value = value; Comment = String.Empty; }
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceFileEntryModel"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="comment">The comment.</param>
        public ResourceFileEntryModel(String key, String value, String comment) { Key = key; Value = value; Comment = comment; }
    }
}
