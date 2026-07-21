// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Library.Tests.given;

/// <summary>
/// The exception that is thrown when a spec asks for something the environment cannot give it — no container
/// runtime for the composition, or no browser to drive a frontend with. Whatever it says is about the machine the
/// specs are running on, never about the system under test.
/// </summary>
/// <param name="reason">What the environment could not provide.</param>
public class RunCannotBeHosted(string reason) : Exception(reason);
