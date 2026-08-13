using BeautySalon.Application.Common.Interfaces;

namespace BeautySalon.Application.Tests.Features.Auth;

// Deterministic stand-in for BCryptPasswordHasher, which lives in Infrastructure - a project
// this test suite intentionally doesn't reference (see TestDatabase). Hashing/verification
// isn't the business rule under test here, so an identity comparison is enough.
public sealed class FakePasswordHasher : IPasswordHasher
{
    public string Hash(string plainTextPassword) => plainTextPassword;
    public bool Verify(string plainTextPassword, string hash) => plainTextPassword == hash;
}
