// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Interpreter.Contracts;
using Cratis.Screenplay.Printing;

namespace Cratis.Prologue.Screenplay.for_ScreenplayGenerator.given;

public class a_library_extraction_result : Specification
{
    protected ScreenplayGenerator _generator;
    protected ExtractionResult _result;

    void Establish()
    {
        _generator = new ScreenplayGenerator(new ScreenplayPrinter());
        _result = new ExtractionResult(
            Guid.NewGuid(),
            [
                new ExtractedModule(
                    "Library",
                    [
                        new ExtractedFeature(
                            "Authors",
                            [],
                            [
                                new ExtractedSlice(
                                    "Register",
                                    ExtractedSliceType.StateChange,
                                    [
                                        new ExtractedCommand(
                                            "RegisterAuthor",
                                            [
                                                new ExtractedProperty("AuthorId", "Guid", IsRequired: true),
                                                new ExtractedProperty("Name", "string", IsRequired: true, MaxLength: 200),
                                                new ExtractedProperty("Email", "string", IsRequired: true, MaxLength: 320),
                                                new ExtractedProperty("BornOn", "DateOnly"),
                                                new ExtractedProperty("RegisteredAt", "DateTimeOffset"),
                                                new ExtractedProperty("BookCount", "int"),
                                                new ExtractedProperty("Royalty", "decimal"),
                                                new ExtractedProperty("IsActive", "bool")
                                            ],
                                            [
                                                new ExtractedValidationRule("Name", ExtractedValidationRuleKind.Required, string.Empty, "Name is required"),
                                                new ExtractedValidationRule("Name", ExtractedValidationRuleKind.MaxLength, "200", "Name must be at most 200 characters"),
                                                new ExtractedValidationRule("Email", ExtractedValidationRuleKind.MinLength, "3", string.Empty),
                                                new ExtractedValidationRule("Email", ExtractedValidationRuleKind.Pattern, "^.+@.+$", "Must be a valid email address")
                                            ],
                                            "Registers an author with a unique email address")
                                    ],
                                    [
                                        new ExtractedEvent(
                                            "AuthorRegistered",
                                            [
                                                new ExtractedProperty("AuthorId", "Guid", IsRequired: true),
                                                new ExtractedProperty("Name", "string", IsRequired: true, MaxLength: 200),
                                                new ExtractedProperty("Email", "string", IsRequired: true, MaxLength: 320)
                                            ])
                                    ],
                                    [],
                                    [],
                                    [new ExtractedConstraint("UniqueEmail", "email", "AuthorRegistered")],
                                    "Registers an author in the catalog"),
                                new ExtractedSlice(
                                    "AllAuthors",
                                    ExtractedSliceType.StateView,
                                    [],
                                    [],
                                    [
                                        new ExtractedReadModel(
                                            "Author",
                                            [
                                                new ExtractedProperty("AuthorId", "Guid", IsRequired: true),
                                                new ExtractedProperty("Name", "string", IsRequired: true, MaxLength: 200),
                                                new ExtractedProperty("Email", "string", IsRequired: true, MaxLength: 320)
                                            ])
                                    ],
                                    [new ExtractedProjection("AuthorProjection", ["AuthorRegistered"])],
                                    [],
                                    "Lists the registered authors")
                            ],
                            "The author lifecycle")
                    ],
                    "Everything about the library")
            ],
            "LibrarySystem");
    }
}
#endif
