// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;

namespace Cratis.Prologue.Workbench.Discovery.Agents;

/// <summary>
/// The prompts the agents are given. Each shows the files with numbered lines, so that a decision point can name its line, and
/// asks for JSON only.
/// </summary>
public static class AnalysisPrompts
{
    /// <summary>
    /// The Dramaturg's prompt: modules and capabilities.
    /// </summary>
    /// <param name="batch">The files to read.</param>
    /// <returns>The prompt.</returns>
    public static string Capabilities(AnalysisBatch batch) => Compose(
        "You map source code into the capabilities of an application. A capability is something a user or another system can ask the application to do, such as \"Lend a copy\" or \"Remove an author\"; a module groups related capabilities, such as \"Loans\".",
        "Include handlers that change state, scheduled jobs and message consumers, not plain reads. For a capability reached by an HTTP command, give the command as \"METHOD /path\" with the full route (a POST to a route with an id parameter is written like \"POST /api/loans/{id}/return\"). Leave \"commands\" empty when no HTTP command reaches it.",
        "{\"capabilities\":[{\"module\":\"Loans\",\"capability\":\"Return a copy\",\"commands\":[\"POST /api/loans/{id}/return\"],\"file\":\"Loans/LoansController.cs\"}]}",
        batch);

    /// <summary>
    /// The Prompter's prompt: decision points with file and line.
    /// </summary>
    /// <param name="batch">The files to read.</param>
    /// <returns>The prompt.</returns>
    public static string DecisionPoints(AnalysisBatch batch) => Compose(
        "You find the decision points in source code: the places where it decides, such as refusing a command, changing state, or running on a schedule. State each as a rule in one plain sentence, like \"Refuse a loan when no copy is available\".",
        "\"line\" is the line number shown before the code that decides. \"kind\" is \"refusal\" when the code rejects the command, \"stateChange\" when it changes state because the command was accepted, and \"schedule\" when it runs on a timer or as a background job. Give \"command\" as \"METHOD /path\" when the code is reached by an HTTP command, otherwise leave it empty.",
        "{\"decisionPoints\":[{\"rule\":\"Refuse a loan when no copy is available\",\"file\":\"Loans/LoansController.cs\",\"line\":71,\"command\":\"POST /api/loans\",\"kind\":\"refusal\"}]}",
        batch);

    /// <summary>
    /// The Usher's prompt: dependencies.
    /// </summary>
    /// <param name="batch">The files to read.</param>
    /// <returns>The prompt.</returns>
    public static string Dependencies(AnalysisBatch batch) => Compose(
        "You trace what the code depends on: calls to other services over HTTP, databases it reads or writes, scheduled or background jobs it runs, and queues or buses it uses.",
        "\"target\" names the other service, database, queue or job. \"kind\" is \"http\", \"database\", \"job\" or \"queue\". \"detail\" says how it is used in a few words, such as \"POST /notify\" or \"writes loans and copies\".",
        "{\"dependencies\":[{\"target\":\"Notifications\",\"kind\":\"http\",\"detail\":\"POST /notify\"}]}",
        batch);

    /// <summary>
    /// Renders files with numbered lines.
    /// </summary>
    /// <param name="batch">The files.</param>
    /// <returns>The text shown to the agent.</returns>
    public static string Render(AnalysisBatch batch)
    {
        var builder = new StringBuilder();
        foreach (var file in batch.Files)
        {
            builder.Append("=== ").Append(file.Path.Value).Append(" ===\n");
            var number = 1;
            foreach (var line in file.Content.Split('\n'))
            {
                builder.Append(number++).Append(": ").Append(line.TrimEnd('\r')).Append('\n');
            }
        }

        return builder.ToString();
    }

    static string Compose(string task, string rules, string example, AnalysisBatch batch) =>
        $"{task}\n\n{rules}\n\nAnswer with JSON only, in exactly this shape, and an empty list when you find nothing. \"file\" is the path shown in the header of each file below. Do not invent files or lines.\n{example}\n\nThe source:\n{Render(batch)}";
}
