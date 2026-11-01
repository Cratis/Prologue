// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Microsoft.Extensions.Primitives;

namespace Cratis.Prologue.Extractor.Shadowing.for_ShadowHeaders;

public class when_selecting_headers : Specification
{
    IReadOnlyList<KeyValuePair<string, string[]>> _selected;
    IReadOnlyList<KeyValuePair<string, string[]>> _withForwarded;

    static readonly KeyValuePair<string, StringValues>[] _headers =
    [
        new("Content-Type", "application/json"),
        new("Accept", new StringValues(["application/json", "text/plain"])),
        new("X-Correlation", "abc"),
        new("traceparent", "00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01"),
        new("Connection", "keep-alive"),
        new("Transfer-Encoding", "chunked"),
        new("Upgrade", "websocket"),
        new("Host", "legacy.example"),
        new("Content-Length", "42"),
        new("Authorization", "Bearer value"),
        new("Cookie", "session=value"),
        new("X-Api-Key", "value"),
        new("X-Auth-Token", "value"),
        new("User-Agent", "legacy-client/1.0"),
        new("Accept-Language", "nb-NO"),
        new("tracestate", "vendor=value"),
        new("baggage", "tenant=a"),
        new("X-Forwarded-For", "10.0.0.7"),
        new("X-Tenant", "contoso"),
        new("X-Prologue-Shadow", "false")
    ];

    void Because()
    {
        _selected = ShadowHeaders.Select(_headers, new HashSet<string>());
        _withForwarded = ShadowHeaders.Select(_headers, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "authorization", "Connection", "x-tenant" });
    }

    bool Has(IReadOnlyList<KeyValuePair<string, string[]>> headers, string name) => headers.Any(header => string.Equals(header.Key, name, StringComparison.OrdinalIgnoreCase));

    [Fact] void should_copy_the_content_type() => Has(_selected, "Content-Type").ShouldBeTrue();
    [Fact] void should_copy_every_value_of_a_header() => _selected.Single(header => header.Key == "Accept").Value.ShouldContainOnly(["application/json", "text/plain"]);
    [Fact] void should_copy_the_headers_on_the_allowlist() => new[] { "Content-Type", "Accept", "Accept-Language", "User-Agent", "tracestate", "baggage" }.All(name => Has(_selected, name)).ShouldBeTrue();
    [Fact] void should_drop_a_header_that_is_not_on_the_allowlist() => new[] { "X-Correlation", "X-Forwarded-For", "X-Tenant" }.Any(name => Has(_selected, name)).ShouldBeFalse();
    [Fact] void should_forward_a_header_a_rule_lists_even_when_it_is_not_a_credential() => Has(_withForwarded, "X-Tenant").ShouldBeTrue();
    [Fact] void should_not_let_the_request_set_the_marker() => _selected.Single(header => header.Key == ShadowHeaders.MarkerHeader).Value.ShouldContainOnly(["true"]);
    [Fact] void should_copy_the_trace_so_the_two_systems_can_be_followed_together() => Has(_selected, "traceparent").ShouldBeTrue();
    [Fact] void should_drop_hop_by_hop_headers() => new[] { "Connection", "Transfer-Encoding", "Upgrade" }.Any(name => Has(_selected, name)).ShouldBeFalse();
    [Fact] void should_drop_host_and_content_length() => (Has(_selected, "Host") || Has(_selected, "Content-Length")).ShouldBeFalse();
    [Fact] void should_drop_credentials_by_default() => new[] { "Authorization", "Cookie", "X-Api-Key", "X-Auth-Token" }.Any(name => Has(_selected, name)).ShouldBeFalse();
    [Fact] void should_tell_the_shadow_system_the_request_is_a_copy() => _selected.Single(header => header.Key == ShadowHeaders.MarkerHeader).Value.ShouldContainOnly(["true"]);
    [Fact] void should_forward_a_credential_a_rule_lists() => Has(_withForwarded, "Authorization").ShouldBeTrue();
    [Fact] void should_still_drop_the_other_credentials() => (Has(_withForwarded, "Cookie") || Has(_withForwarded, "X-Api-Key")).ShouldBeFalse();
    [Fact] void should_never_forward_a_hop_by_hop_header_even_when_listed() => Has(_withForwarded, "Connection").ShouldBeFalse();
}
#endif
