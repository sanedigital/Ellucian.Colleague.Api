// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Ellucian.Colleague.Api.Models
{
    /// <summary>
    /// Object that reflects a resource file
    /// </summary>
    [Serializable]
    public class ResourceFileModel
    {
        /// <summary>
        /// Gets or sets the full path of the resource file.
        /// </summary>
        /// <value>
        /// The path of the resource file.
        /// </value>
        public string ResourceFilePath { get; set; }
        /// <summary>
        /// Gets the name of the resource file from the resource file path
        /// </summary>
        /// <value>
        /// The name of the resource file.
        /// </value>
        public string ResourceFileName { get { return Path.GetFileName(ResourceFilePath); } }
        /// <summary>
        /// Gets or sets the resource file entries.
        /// </summary>
        /// <value>
        /// The resource file entries.
        /// </value>
        public List<ResourceFileEntryModel> ResourceFileEntries { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceFileModel"/> class.
        /// </summary>
        public ResourceFileModel()
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceFileModel"/> class.
        /// </summary>
        /// <param name="path">Path of the resource file</param>
        public ResourceFileModel(string path)
        {
            ResourceFilePath = path;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceFileModel"/> class.
        /// </summary>
        /// <param name="path">The path of the resource file.</param>
        /// <param name="resItems">The resource items within the resource file.</param>
        public ResourceFileModel(string path, List<ResourceFileEntryModel> resItems)
        {
            ResourceFilePath = path;
            ResourceFileEntries = resItems;
        }
    }
}
