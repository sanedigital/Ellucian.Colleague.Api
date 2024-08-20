// Copyright 2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Api.Models
{
    /// <summary>
    /// API photo settings model.
    /// </summary>
    public class PhotoSettingsModel
    {
        /// <summary>
        /// Gets or sets the base photo URL.
        /// </summary>
        public string BasePhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets a list of supported image types.
        /// </summary>
        public List<KeyValuePair<string, string>> ImageTypes { get; set; }

        /// <summary>
        /// Gets or sets the selected image type.
        /// </summary>
        public KeyValuePair<string, string> SelectedImageType { get; set; }

        /// <summary>
        /// Gets or sets the image extension.
        /// </summary>
        public string ImageExtension { get; set; }

        /// <summary>
        /// Gets or sets a list of custom headers.
        /// </summary>
        public List<KeyValuePair<string, string>> CustomHeaders { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PhotoSettingsModel()
        {
            BasePhotoUrl = string.Empty;
            ImageTypes = new List<KeyValuePair<string, string>>();
            CustomHeaders = new List<KeyValuePair<string, string>>();
            ImageTypes.Add(new KeyValuePair<string, string>("GIF", "gif"));
            ImageTypes.Add(new KeyValuePair<string, string>("JPEG", "jpg"));
            ImageTypes.Add(new KeyValuePair<string, string>("PNG", "png"));
            ImageTypes.Add(new KeyValuePair<string, string>("TIFF", "tiff"));
            SelectedImageType = ImageTypes[0];
        }

        /// <summary>
        /// Populates the BasePhotoUrl and ImageExtension properties.
        /// </summary>
        /// <param name="formattedUrl"></param>
        public void ParseFormattedUrl(string formattedUrl)
        {
            string idPlaceholder = "{0}";
            if (!string.IsNullOrEmpty(formattedUrl) && formattedUrl.Contains(idPlaceholder))
            {
                BasePhotoUrl = formattedUrl.Substring(0, (formattedUrl.IndexOf(idPlaceholder)));
                try
                {
                    var t = formattedUrl.Substring((formattedUrl.IndexOf(idPlaceholder) + idPlaceholder.Length));
                    ImageExtension = t.TrimStart('.');
                }
                catch (Exception)
                {
                    ImageExtension = "";
                }
            }

        }

        /// <summary>
        /// Builds the full image URL using the BasePhotoUrl and ImageExtension properties.
        /// </summary>
        /// <returns></returns>
        public string GetFormattedUrl()
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(BasePhotoUrl))
            {
                result = BasePhotoUrl + "{0}";
                if (!string.IsNullOrEmpty(ImageExtension))
                {
                    result += string.Format(".{0}", ImageExtension);
                }
            }

            return result;
        }
    }
}
