// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Screenplay.for_ScreenplayGenerator;

public class when_generating_the_play_text : given.a_library_extraction_result
{
    string _play;

    void Because() => _play = _generator.Generate(_result);

    [Fact] void should_declare_the_module() => _play.ShouldContain("module Library");
    [Fact] void should_describe_the_module() => _play.ShouldContain("description \"Everything about the library\"");
    [Fact] void should_declare_the_feature() => _play.ShouldContain("feature Authors");
    [Fact] void should_describe_the_feature() => _play.ShouldContain("description \"The author lifecycle\"");
    [Fact] void should_declare_the_state_change_slice() => _play.ShouldContain("slice StateChange Register");
    [Fact] void should_describe_the_state_change_slice() => _play.ShouldContain("description \"Registers an author in the catalog\"");
    [Fact] void should_declare_the_state_view_slice() => _play.ShouldContain("slice StateView AllAuthors");
    [Fact] void should_declare_the_command() => _play.ShouldContain("command RegisterAuthor");
    [Fact] void should_describe_the_command_as_the_first_body_line() => _play.ShouldContain("command RegisterAuthor\n        description \"Registers an author with a unique email address\"");
    [Fact] void should_map_a_guid_property_to_uuid() => _play.ShouldContain("authorId Uuid");
    [Fact] void should_map_a_string_property_to_string() => _play.ShouldContain("name String");
    [Fact] void should_map_a_date_only_property_to_date() => _play.ShouldContain("bornOn Date");
    [Fact] void should_map_a_date_time_offset_property_to_date_time() => _play.ShouldContain("registeredAt DateTime");
    [Fact] void should_map_an_int_property_to_int() => _play.ShouldContain("bookCount Int");
    [Fact] void should_map_a_decimal_property_to_decimal() => _play.ShouldContain("royalty Decimal");
    [Fact] void should_map_a_bool_property_to_bool() => _play.ShouldContain("isActive Bool");
    [Fact] void should_emit_the_required_rule() => _play.ShouldContain("name not empty message \"Name is required\"");
    [Fact] void should_emit_the_max_length_rule() => _play.ShouldContain("name max 200 message \"Name must be at most 200 characters\"");
    [Fact] void should_emit_the_min_length_rule_without_a_message() => _play.ShouldContain("email min 3");
    [Fact] void should_emit_the_pattern_rule() => _play.ShouldContain("email matches \"^.+@.+$\" message \"Must be a valid email address\"");
    [Fact] void should_produce_the_event() => _play.ShouldContain("produces AuthorRegistered");
    [Fact] void should_map_the_produced_event_properties_by_identity() => _play.ShouldContain("authorId = authorId");
    [Fact] void should_declare_the_event() => _play.ShouldContain("event AuthorRegistered");
    [Fact] void should_declare_the_constraint() => _play.ShouldContain("constraint UniqueEmail");
    [Fact] void should_declare_the_unique_property() => _play.ShouldContain("unique email on AuthorRegistered");
    [Fact] void should_declare_the_query_for_the_read_model() => _play.ShouldContain("query AllAuthors => Author[]");
    [Fact] void should_declare_the_projection_targeting_the_read_model() => _play.ShouldContain("projection AuthorProjection => Author");
    [Fact] void should_subscribe_to_the_source_event() => _play.ShouldContain("from AuthorRegistered");
    [Fact] void should_map_the_read_model_properties_by_identity() => _play.ShouldContain("name = name");
}
#endif
