// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Repositories.Cloning;

/// <summary>
/// Represents the exception that is thrown when the secret holding a GitHub App's private key does not hold a private RSA key in PEM form.
/// </summary>
public class InvalidPrivateKey() : Exception("The private key is not a PEM-encoded RSA private key");
