﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TinyHelpers.EntityFrameworkCore.Extensions
{
    public static class PropertyBuilderExtensions
    {
        private static readonly JsonSerializerOptions defaultJsonSerializerOptions;

        static PropertyBuilderExtensions()
        {
            defaultJsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            defaultJsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        }

        public static PropertyBuilder<T?> HasJsonConversion<T>(this PropertyBuilder<T?> propertyBuilder, JsonSerializerOptions? jsonSerializerOptions = null)
        {
            var options = jsonSerializerOptions ?? defaultJsonSerializerOptions;

            var converter = new ValueConverter<T?, string?>
            (
                v => JsonSerializer.Serialize(v, options),
                v => v == null ? default : JsonSerializer.Deserialize<T>(v, options)
            );

            var comparer = new ValueComparer<T?>
            (
                (l, r) => JsonSerializer.Serialize(l, options) == JsonSerializer.Serialize(r, options),
                v => v == null ? 0 : JsonSerializer.Serialize(v, options).GetHashCode(),
                v => v == null ? default : JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(v, options), options)
            );

            propertyBuilder.HasConversion(converter);
            propertyBuilder.Metadata.SetValueConverter(converter);
            propertyBuilder.Metadata.SetValueComparer(comparer);
            propertyBuilder.HasColumnType("nvarchar(MAX)");

            return propertyBuilder;
        }

        public static PropertyBuilder<IEnumerable<string>> HasArrayConversion(this PropertyBuilder<IEnumerable<string>> propertyBuilder, string separator = ";")
        {
            var converter = new ValueConverter<IEnumerable<string>, string>
            (
                v => string.Join(separator, v),
                v => v != null ? v.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>()
            );

            var comparer = new ValueComparer<IEnumerable<string>>
            (
                (l, r) => string.Join(separator, l) == string.Join(separator, r),
                v => v == null ? 0 : v.GetHashCode(),
                v => v
            );

            propertyBuilder.HasConversion(converter);
            propertyBuilder.Metadata.SetValueConverter(converter);
            propertyBuilder.Metadata.SetValueComparer(comparer);
            propertyBuilder.HasColumnType("nvarchar(MAX)");

            return propertyBuilder;
        }

    }
}
