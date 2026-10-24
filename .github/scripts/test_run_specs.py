# Copyright (c) Cratis. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

"""Regression checks for the native executable's non-empty-result guard."""

import importlib.util
import subprocess
import unittest
from pathlib import Path
from unittest.mock import patch

SPEC = importlib.util.spec_from_file_location("run_specs", Path(__file__).with_name("run-specs.py"))
RUNNER = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(RUNNER)


class NativeResults(unittest.TestCase):
    def invoke(self, xml, status=0):
        reports = []

        def execute(command, check):
            self.assertFalse(check)
            self.assertEqual(command[:-2], [
                "dotnet", "run", "--project", "project with spaces.csproj",
                "--configuration", "Debug", "--no-build", "--no-restore", "--",
                "-trait-", "Category=Integration",
            ])
            self.assertEqual(command[-2], "-result-xml")
            report = Path(command[-1])
            reports.append(report)
            if xml is not None:
                report.write_text(xml, encoding="utf-8")
            return subprocess.CompletedProcess(command, status)

        with patch.object(RUNNER.subprocess, "run", side_effect=execute):
            result = RUNNER.run("project with spaces.csproj", ["-trait-", "Category=Integration"])
        self.assertFalse(reports[0].parent.exists())
        return result

    def test_nonempty_report_passes(self):
        self.assertEqual(self.invoke('<assemblies><assembly total="3"/></assemblies>'), 0)

    def test_multiple_assemblies_pass(self):
        self.assertEqual(self.invoke('<assemblies><assembly total="3"/><assembly total="2"/></assemblies>'), 0)

    def test_zero_selection_fails(self):
        self.assertEqual(self.invoke('<assemblies><assembly total="0"/></assemblies>'), 1)

    def test_missing_report_fails(self):
        self.assertEqual(self.invoke(None), 1)

    def test_malformed_report_fails(self):
        self.assertEqual(self.invoke('<assemblies>'), 1)

    def test_missing_assembly_fails(self):
        self.assertEqual(self.invoke('<assemblies/>'), 1)

    def test_wrong_root_fails(self):
        self.assertEqual(self.invoke('<results><assembly total="3"/></results>'), 1)

    def test_missing_total_fails(self):
        self.assertEqual(self.invoke('<assemblies><assembly/></assemblies>'), 1)

    def test_nonnumeric_total_fails(self):
        self.assertEqual(self.invoke('<assemblies><assembly total="invalid"/></assemblies>'), 1)

    def test_negative_total_fails(self):
        self.assertEqual(self.invoke('<assemblies><assembly total="-1"/></assemblies>'), 1)

    def test_failed_runner_status_is_preserved(self):
        self.assertEqual(self.invoke(None, status=17), 17)

    def test_failed_runner_cannot_be_masked_by_valid_report(self):
        self.assertEqual(self.invoke('<assemblies><assembly total="3"/></assemblies>', status=23), 23)


if __name__ == "__main__":
    unittest.main()
