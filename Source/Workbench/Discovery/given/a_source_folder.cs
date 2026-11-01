// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Discovery.Extraction;

namespace Cratis.Prologue.Workbench.Discovery.given;

/// <summary>
/// A temporary folder holding a small library system: one controller, some noise and a few folders that must be skipped.
/// </summary>
public class a_source_folder : Specification, IDisposable
{
    protected string _root;

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }

        GC.SuppressFinalize(this);
    }

    protected static void Write(string root, string relative, string content)
    {
        var path = Path.Combine(root, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }

    void Establish()
    {
        _root = Path.Combine(Path.GetTempPath(), $"prologue-discover-{Guid.NewGuid():N}");
        Write(_root, "Loans/LoansController.cs", """
            [ApiController]
            [Route("api/loans")]
            public class LoansController : ControllerBase
            {
                [HttpPost]
                public IActionResult LendCopy(LendRequest request)
                {
                    if (!_copies.Available(request.Book)) return Conflict();
                    return Ok();
                }

                [HttpPost("{id:int}/return")]
                public IActionResult ReturnLoan(int id) => Ok();
            }
            """);
        Write(_root, "Loans/LoanPolicy.cs", "public class LoanPolicy { }");
        Write(_root, "Program.cs", "var app = builder.Build();");
        Write(_root, "README.md", "# Library");
        Write(_root, "bin/Debug/Generated.cs", "public class Generated { }");
        Write(_root, "obj/Thing.cs", "public class Thing { }");
        Write(_root, "node_modules/lib/index.js", "module.exports = {};");
        Write(_root, ".git/hooks/pre-commit.py", "print('hi')");
        Write(_root, "Loans/Loan.g.cs", "public class Loan { }");
        Write(_root, "Data/Huge.cs", new string('x', (int)SourceFileScanner.MaxFileSize + 10));
    }
}
#endif
