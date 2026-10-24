#!/usr/bin/env python3
# Copyright (c) Cratis. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

"""Run one native Debug spec executable and reject missing or empty results."""

import subprocess
import sys
import tempfile
from pathlib import Path
from xml.etree import ElementTree


def result_count(report):
    root = ElementTree.parse(report).getroot()
    assemblies = root.findall("assembly")
    if root.tag != "assemblies" or not assemblies:
        raise ValueError("expected assemblies/assembly results")
    counts = [int(assembly.attrib["total"]) for assembly in assemblies]
    if any(count <= 0 for count in counts):
        raise ValueError("no tests selected")
    return sum(counts)


def run(project, arguments):
    with tempfile.TemporaryDirectory(prefix="prologue-specs-") as directory:
        report = Path(directory) / "results.xml"
        status = subprocess.run([
            "dotnet", "run", "--project", project, "--configuration", "Debug",
            "--no-build", "--no-restore", "--", *arguments, "-result-xml", str(report),
        ], check=False).returncode
        if status != 0:
            return status if status > 0 else 128 - status
        try:
            count = result_count(report)
        except (OSError, ElementTree.ParseError, KeyError, ValueError) as error:
            print(f"{project}: invalid native spec results: {error}", file=sys.stderr)
            return 1
        print(f"{project}: {count} tests selected")
        return 0


def main():
    if len(sys.argv) < 2:
        print("Usage: run-specs.py PROJECT [native runner options]", file=sys.stderr)
        return 2
    try:
        return run(sys.argv[1], sys.argv[2:])
    except OSError as error:
        print(f"Unable to run native specs: {error}", file=sys.stderr)
        return 2


if __name__ == "__main__":
    sys.exit(main())
