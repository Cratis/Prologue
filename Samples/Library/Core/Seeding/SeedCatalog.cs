// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Library.Core.Seeding;

/// <summary>
/// The fixed starting collection the library is seeded with. Kept apart from the seeding logic so the data can be
/// read and changed without wading through the code that writes it.
/// </summary>
public static class SeedCatalog
{
    /// <summary>
    /// Gets the authors the library starts with.
    /// </summary>
    public static readonly (string FirstName, string LastName)[] Authors =
    [
        ("Ursula", "Le Guin"),
        ("Kazuo", "Ishiguro"),
        ("Toni", "Morrison"),
        ("Italo", "Calvino"),
        ("Chinua", "Achebe"),
        ("Octavia", "Butler"),
        ("Gabriel", "Garcia Marquez"),
        ("Virginia", "Woolf")
    ];

    /// <summary>
    /// Gets the members the library starts with.
    /// </summary>
    public static readonly (string FirstName, string LastName)[] Members =
    [
        ("Aina", "Berg"),
        ("Jonas", "Holm"),
        ("Sofie", "Lund"),
        ("Mikkel", "Dahl"),
        ("Elise", "Vik"),
        ("Anders", "Moen")
    ];

    /// <summary>
    /// Gets the titles the catalog starts with.
    /// </summary>
    public static readonly (string Isbn, string Title)[] Books =
    [
        ("9780135000000", "The Left Hand of Darkness"),
        ("9780135000001", "The Dispossessed"),
        ("9780135000002", "The Remains of the Day"),
        ("9780135000003", "Never Let Me Go"),
        ("9780135000004", "Beloved"),
        ("9780135000005", "Song of Solomon"),
        ("9780135000006", "Invisible Cities"),
        ("9780135000007", "If on a Winter's Night a Traveler"),
        ("9780135000008", "Things Fall Apart"),
        ("9780135000009", "Kindred"),
        ("9780135000010", "One Hundred Years of Solitude"),
        ("9780135000011", "Mrs Dalloway")
    ];

    /// <summary>
    /// Gets the tag sets applied across the seeded titles.
    /// </summary>
    public static readonly string[][] Tags =
    [
        ["science-fiction", "classic"],
        ["science-fiction"],
        ["literary", "booker-prize", "classic"],
        ["literary", "dystopian"],
        ["literary", "pulitzer"],
        ["literary"],
        ["experimental", "classic"],
        ["experimental"],
        ["postcolonial", "classic"],
        ["science-fiction", "historical"],
        ["magical-realism", "classic"],
        ["modernist"]
    ];
}
