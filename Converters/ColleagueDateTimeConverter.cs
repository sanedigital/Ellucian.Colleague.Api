// Copyright 2014-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Api.Converters
{
    /// <summary>
    /// This converter overrides json.net's default IsoDateTimeConverter when
    /// de-serializing DateTimeOffset objects
    /// </summary>
    public class ColleagueDateTimeConverter : IsoDateTimeConverter
    {
        /// <summary>
        /// The default time zone identifier
        /// </summary>
        private readonly string _colleagueTimeZoneId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColleagueDateTimeConverter" /> class.
        /// </summary>
        /// <param name="colleagueTimeZoneId">The colleague time zone identifier.</param>
        public ColleagueDateTimeConverter(string colleagueTimeZoneId)
        {
            _colleagueTimeZoneId = colleagueTimeZoneId;
        }

        /// <summary>
        /// Reads the JSON representation of the object with special treatment for <see cref="DateTimeOffset"/> types
        /// </summary>
        /// <param name="reader">The Newtonsoft.Json.JsonReader to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // This method overrides ReadJson to handle the conversion of inbound datetime json strings that
            // do not contain an offset, or are not UTC time, to a DateTimeOffset object.
            // Normally, the API server's time zone's offset will be used, but we don't want that. 
            // With this custom converter, the Colleague time zone's offset will be used instead.

            // Only apply Colleague time zone offset if target deserialization type is DateTimeOffset
            if (objectType != typeof(DateTimeOffset) && objectType != typeof(DateTimeOffset?))
            {
                return base.ReadJson(reader, objectType, existingValue, serializer);
            }

            // If the reader is null or has no value, let the default ReadJson() handle it
            if (reader == null || reader.Value == null)
            {
                return base.ReadJson(reader, objectType, existingValue, serializer);
            }

            var dateText = reader.Value.ToString();
            if (objectType == typeof(DateTimeOffset?) && string.IsNullOrEmpty(dateText))
            {
                return null;
            }
            // Here the datetime string in dateText will be raw json format (yyyy-mm-ddThh:mm:ss-+hh:mm)
            // because we set DateParseHandling = None in the bootstrapper.
            if (dateText.IndexOfAny(new[] { 'Z', 'z', '+' }) == -1 && dateText.Count(c => c == '-') == 2)
            {
                // If this time is a local time (no 'Z' or offset time located) then apply
                // Colleague time zone's offset to it (we assume it's from the same time zone.)
                var dt = DateTime.Parse(dateText);
                var tz = TimeZoneInfo.FindSystemTimeZoneById(_colleagueTimeZoneId);
                var offset = tz.GetUtcOffset(dt);
                DateTimeOffset convertedDateTimeOffset = new DateTimeOffset(dt, offset);
                return convertedDateTimeOffset;
            }

            DateTimeOffset unmodifiedDateTimeOffset = DateTimeOffset.Parse(dateText);
            return unmodifiedDateTimeOffset;
        }

        /// <summary>
        /// Writes the JSON representation of the object with special treatment for <see cref="DateTimeOffset" /> types
        /// </summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // When Serializing DateTimeOffset or DateTimeOffset? objects, 
            // use the UTC DateTime in order to emit Zulu time strings.
            // (API guidelines mandate UTC time.)
            if (value is DateTimeOffset)
            {
                DateTime utcDateTime = ((DateTimeOffset)value).UtcDateTime;
                base.WriteJson(writer, utcDateTime, serializer);
            }
            else if (value is DateTimeOffset?)
            {
                DateTimeOffset? nullableDTO = (DateTimeOffset?)value;
                if (nullableDTO.HasValue)
                {
                    DateTime utcDateTime = nullableDTO.Value.UtcDateTime;
                    base.WriteJson(writer, utcDateTime, serializer);
                }
                else
                {
                    base.WriteJson(writer, value, serializer);
                }
            }
            else
            {
                base.WriteJson(writer, value, serializer);
            }
        }

    }
}
