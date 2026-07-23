// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Contracts;

namespace Cratis.Prologue.Interpretation.for_Naming;

public class when_building_names : Specification
{
    [Fact] void should_pascalize_a_snake_case_column() => Naming.Pascalize("author_id").ShouldEqual("AuthorId");
    [Fact] void should_singularize_a_plural_table() => Naming.Singularize("Authors").ShouldEqual("Author");
    [Fact] void should_singularize_an_ies_plural() => Naming.Singularize("Categories").ShouldEqual("Category");
    [Fact] void should_name_an_insert_event_as_created() => Naming.EventName("Author", ChangeOperation.Insert).ShouldEqual("AuthorCreated");
    [Fact] void should_name_an_update_event_as_updated() => Naming.EventName("Author", ChangeOperation.Update).ShouldEqual("AuthorUpdated");
    [Fact] void should_name_a_post_command_as_create() => Naming.CommandName("POST", "Author").ShouldEqual("CreateAuthor");
    [Fact] void should_name_a_delete_command_as_delete() => Naming.CommandName("DELETE", "Author").ShouldEqual("DeleteAuthor");
    [Fact] void should_infer_guid_for_an_id_column() => Naming.InferType("AuthorId").ShouldEqual("Guid");
    [Fact] void should_infer_datetimeoffset_for_a_date_column() => Naming.InferType("CreatedAt").ShouldEqual("DateTimeOffset");
    [Fact] void should_infer_string_by_default() => Naming.InferType("Name").ShouldEqual("string");
}
#endif
