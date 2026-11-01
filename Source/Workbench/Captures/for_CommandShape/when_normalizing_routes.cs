// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG

namespace Cratis.Prologue.Workbench.Captures.for_CommandShape;

public class when_normalizing_routes : Specification
{
    [Fact] void should_replace_a_numeric_segment() => CommandShape.From("POST", "/api/loans/2649/return").Value.ShouldEqual("POST /api/loans/{id}/return");
    [Fact] void should_replace_an_isbn_like_segment() => CommandShape.From("POST", "/api/catalog/books/978-0-123456/tags").Value.ShouldEqual("POST /api/catalog/books/{id}/tags");
    [Fact] void should_replace_a_guid_segment() => CommandShape.From("DELETE", "/api/authors/3f2504e0-4f89-11d3-9a0c-0305e82c3301").Value.ShouldEqual("DELETE /api/authors/{id}");
    [Fact] void should_upper_case_the_method() => CommandShape.From("post", "/api/loans").Value.ShouldEqual("POST /api/loans");
    [Fact] void should_drop_the_query_string() => CommandShape.From("GET", "/api/loans/12?include=copies&page=2").Value.ShouldEqual("GET /api/loans/{id}");
    [Fact] void should_drop_a_trailing_slash() => CommandShape.From("POST", "/api/loans/").Value.ShouldEqual("POST /api/loans");
    [Fact] void should_keep_short_segments_with_a_digit() => CommandShape.From("GET", "/api/v1/loans").Value.ShouldEqual("GET /api/v1/loans");
    [Fact] void should_keep_words() => CommandShape.From("PUT", "/api/inventory/restock").Value.ShouldEqual("PUT /api/inventory/restock");
    [Fact] void should_keep_an_existing_placeholder() => CommandShape.From("POST", "/api/loans/{id}/return").Value.ShouldEqual("POST /api/loans/{id}/return");
    [Fact] void should_give_the_root_a_slash() => CommandShape.From("GET", string.Empty).Value.ShouldEqual("GET /");
    [Fact] void should_give_the_same_shape_to_different_identities() => CommandShape.From("POST", "/api/loans/1/return").ShouldEqual(CommandShape.From("POST", "/api/loans/99999/return"));
}
#endif
