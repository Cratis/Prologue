// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

global using System.Reactive.Subjects;
global using Cratis.Arc.Commands;
global using Cratis.Arc.Commands.ModelBound;
global using Cratis.Arc.Queries.ModelBound;
global using Cratis.Chronicle.Events;
global using Cratis.Chronicle.EventSequences;
global using Cratis.Chronicle.Keys;
global using Cratis.Chronicle.Projections.ModelBound;
global using Cratis.Chronicle.ReadModels;

#if DEBUG
global using Cratis.Arc.Chronicle.Testing.Commands;
global using Cratis.Arc.Testing.Commands;
global using Cratis.Chronicle.Testing;
global using Cratis.Chronicle.Testing.EventSequences;
global using Cratis.Chronicle.Testing.ReadModels;
#endif
