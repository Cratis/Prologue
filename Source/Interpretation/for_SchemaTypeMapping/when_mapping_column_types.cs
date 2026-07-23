// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Interpretation.for_SchemaTypeMapping;

public class when_mapping_column_types : Specification
{
    [Fact] void should_map_varchar_to_string() => SchemaTypeMapping.TypeFor("varchar").ShouldEqual("string");
    [Fact] void should_map_nvarchar_to_string() => SchemaTypeMapping.TypeFor("nvarchar").ShouldEqual("string");
    [Fact] void should_map_text_to_string() => SchemaTypeMapping.TypeFor("text").ShouldEqual("string");
    [Fact] void should_map_character_varying_to_string() => SchemaTypeMapping.TypeFor("character varying").ShouldEqual("string");
    [Fact] void should_map_uniqueidentifier_to_guid() => SchemaTypeMapping.TypeFor("uniqueidentifier").ShouldEqual("Guid");
    [Fact] void should_map_uuid_to_guid() => SchemaTypeMapping.TypeFor("uuid").ShouldEqual("Guid");
    [Fact] void should_map_int_to_int() => SchemaTypeMapping.TypeFor("int").ShouldEqual("int");
    [Fact] void should_map_integer_to_int() => SchemaTypeMapping.TypeFor("integer").ShouldEqual("int");
    [Fact] void should_map_bigint_to_long() => SchemaTypeMapping.TypeFor("bigint").ShouldEqual("long");
    [Fact] void should_map_bit_to_bool() => SchemaTypeMapping.TypeFor("bit").ShouldEqual("bool");
    [Fact] void should_map_boolean_to_bool() => SchemaTypeMapping.TypeFor("boolean").ShouldEqual("bool");
    [Fact] void should_map_datetime_to_datetimeoffset() => SchemaTypeMapping.TypeFor("datetime").ShouldEqual("DateTimeOffset");
    [Fact] void should_map_datetime2_to_datetimeoffset() => SchemaTypeMapping.TypeFor("datetime2").ShouldEqual("DateTimeOffset");
    [Fact] void should_map_timestamp_to_datetimeoffset() => SchemaTypeMapping.TypeFor("timestamp").ShouldEqual("DateTimeOffset");
    [Fact] void should_map_timestamptz_to_datetimeoffset() => SchemaTypeMapping.TypeFor("timestamptz").ShouldEqual("DateTimeOffset");
    [Fact] void should_map_timestamp_with_time_zone_to_datetimeoffset() => SchemaTypeMapping.TypeFor("timestamp with time zone").ShouldEqual("DateTimeOffset");
    [Fact] void should_map_date_to_dateonly() => SchemaTypeMapping.TypeFor("date").ShouldEqual("DateOnly");
    [Fact] void should_map_decimal_to_decimal() => SchemaTypeMapping.TypeFor("decimal").ShouldEqual("decimal");
    [Fact] void should_map_numeric_to_decimal() => SchemaTypeMapping.TypeFor("numeric").ShouldEqual("decimal");
    [Fact] void should_map_money_to_decimal() => SchemaTypeMapping.TypeFor("money").ShouldEqual("decimal");
    [Fact] void should_ignore_casing() => SchemaTypeMapping.TypeFor("NVARCHAR").ShouldEqual("string");
    [Fact] void should_not_map_an_unknown_type() => SchemaTypeMapping.TypeFor("geometry").ShouldBeNull();
}
#endif
