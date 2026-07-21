// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Library.Core.Database;
using Library.Core.Telemetry;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.Core.Members;

/// <summary>
/// Represents the details needed to register a member.
/// </summary>
/// <param name="FirstName">The member's first name.</param>
/// <param name="LastName">The member's last name.</param>
public record RegisterMember(string FirstName, string LastName);

/// <summary>
/// Represents a member as returned to callers.
/// </summary>
/// <param name="MemberId">The identifier of the member.</param>
/// <param name="FirstName">The member's first name.</param>
/// <param name="LastName">The member's last name.</param>
public record MemberDetails(int MemberId, string FirstName, string LastName);

/// <summary>
/// Exposes the people registered with the library.
/// </summary>
/// <param name="dbContext">The library database.</param>
[ApiController]
[Route("api/members")]
public class MembersController(LibraryDbContext dbContext) : ControllerBase
{
    /// <summary>
    /// Lists every registered member.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The registered members.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDetails>>> All(CancellationToken cancellationToken) =>
        await dbContext.Members
            .OrderBy(member => member.LastName).ThenBy(member => member.FirstName)
            .Select(member => new MemberDetails(member.MemberId, member.FirstName, member.LastName))
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Registers a member.
    /// </summary>
    /// <param name="command">The member to register.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The registered member.</returns>
    [HttpPost]
    public async Task<ActionResult<MemberDetails>> Register([FromBody] RegisterMember command, CancellationToken cancellationToken)
    {
        var member = new Member { FirstName = command.FirstName, LastName = command.LastName };

        dbContext.Members.Add(member);
        await dbContext.SaveChangesAsync(cancellationToken);

        LibraryTelemetry.Operations.Add(1, new KeyValuePair<string, object?>("operation", "register-member"));

        var details = new MemberDetails(member.MemberId, member.FirstName, member.LastName);
        return CreatedAtAction(nameof(All), new { id = member.MemberId }, details);
    }
}
