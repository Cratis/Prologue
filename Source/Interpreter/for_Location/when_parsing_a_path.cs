// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Interpreter.for_Location;

public class when_parsing_a_path : Specification
{
    (string Module, string Feature, string Resource, string Action) _collection;
    (string Module, string Feature, string Resource, string Action) _actionOnCollection;
    (string Module, string Feature, string Resource, string Action) _nestedCollection;
    (string Module, string Feature, string Resource, string Action) _actionAfterId;
    (string Module, string Feature, string Resource, string Action) _withIdSegment;
    (string Module, string Feature, string Resource, string Action) _template;

    void Because()
    {
        _collection = Location.FromPath("/api/catalog/books");
        _actionOnCollection = Location.FromPath("/api/authors/register");
        _nestedCollection = Location.FromPath("/api/catalog/books/9780135000000/tags");
        _actionAfterId = Location.FromPath("/api/inventory/9780135000000/lost");
        _withIdSegment = Location.FromPath("/api/authors/8f3c2b1a-0000-0000-0000-000000000000");
        _template = Location.FromPath("/api/authors/{id}");
    }

    [Fact] void should_use_the_first_segment_as_the_module() => _collection.Module.ShouldEqual("Catalog");
    [Fact] void should_use_the_first_collection_as_the_feature() => _collection.Feature.ShouldEqual("Books");
    [Fact] void should_singularize_the_last_collection_into_the_resource() => _collection.Resource.ShouldEqual("Book");
    [Fact] void should_have_no_action_for_a_plain_collection() => _collection.Action.ShouldEqual(string.Empty);

    [Fact] void should_treat_a_trailing_verb_as_the_action() => _actionOnCollection.Action.ShouldEqual("Register");
    [Fact] void should_keep_the_resource_for_an_action_on_a_collection() => _actionOnCollection.Resource.ShouldEqual("Author");

    [Fact] void should_group_a_nested_collection_under_the_outer_feature() => _nestedCollection.Feature.ShouldEqual("Books");
    [Fact] void should_use_the_nested_collection_as_the_resource() => _nestedCollection.Resource.ShouldEqual("Tag");

    [Fact] void should_treat_a_verb_after_an_id_as_the_action() => _actionAfterId.Action.ShouldEqual("Lost");
    [Fact] void should_fall_back_to_the_module_resource_without_a_collection() => _actionAfterId.Resource.ShouldEqual("Inventory");

    [Fact] void should_ignore_guid_id_segments() => _withIdSegment.Module.ShouldEqual("Authors");
    [Fact] void should_have_no_action_for_a_guid_id() => _withIdSegment.Action.ShouldEqual(string.Empty);

    [Fact] void should_ignore_template_placeholder_segments() => _template.Resource.ShouldEqual("Author");
    [Fact] void should_have_no_action_for_a_template_placeholder() => _template.Action.ShouldEqual(string.Empty);
}
#endif
