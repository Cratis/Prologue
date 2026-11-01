// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Workbench.Discovery.Agents.for_AnalysisOutputParser.given;

public class shown_files : Specification
{
    protected static readonly IReadOnlyDictionary<string, int> _files = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {
        ["Loans/LoansController.cs"] = 120,
        ["Jobs/OverdueSweep.cs"] = 40
    };
}
#endif
