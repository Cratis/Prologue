// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Library.Tests.given;

/// <summary>
/// The exception that is thrown when a <see cref="Frontend"/> the suite has no address for is asked to be driven.
/// </summary>
/// <param name="frontend">The <see cref="Frontend"/> that could not be resolved.</param>
public class UnknownFrontend(Frontend frontend)
    : Exception($"'{frontend}' is not a frontend this composition serves.");
