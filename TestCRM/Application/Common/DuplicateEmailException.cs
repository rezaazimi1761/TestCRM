namespace TestCRM.Application.Common;

/// <summary>
/// Thrown when a Create command detects a duplicate email within the current tenant.
/// Controllers catch this and return 409 Conflict.
/// </summary>
public sealed class DuplicateEmailException(string message) : Exception(message);
