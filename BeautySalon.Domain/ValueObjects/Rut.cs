using System.Text.RegularExpressions;

namespace BeautySalon.Domain.ValueObjects;

// Normalizes a Chilean RUT and validates its check digit (modulo 11) so an
// invalid RUT can never exist as a constructed value.
public sealed partial record Rut
{
    public string Value { get; }

    private Rut(string normalizedValue)
    {
        Value = normalizedValue;
    }

    public static Rut Create(string rawValue)
    {
        if (!TryCreate(rawValue, out var rut, out var error))
            throw new FormatException(error);

        return rut!;
    }

    public static bool TryCreate(string? rawValue, out Rut? rut, out string? error)
    {
        rut = null;
        error = null;

        if (string.IsNullOrWhiteSpace(rawValue))
        {
            error = "El RUT no puede estar vacío.";
            return false;
        }

        var cleaned = CleanupRegex().Replace(rawValue, string.Empty).ToUpperInvariant();
        if (cleaned.Length < 2)
        {
            error = "El RUT no tiene un formato válido.";
            return false;
        }

        var body = cleaned[..^1];
        var checkDigit = cleaned[^1];

        if (!body.All(char.IsDigit))
        {
            error = "El RUT no tiene un formato válido.";
            return false;
        }

        if (ComputeCheckDigit(body) != checkDigit)
        {
            error = "El dígito verificador del RUT no es válido.";
            return false;
        }

        rut = new Rut($"{body}-{checkDigit}");
        return true;
    }

    private static char ComputeCheckDigit(string body)
    {
        var sum = 0;
        var factor = 2;

        for (var i = body.Length - 1; i >= 0; i--)
        {
            sum += (body[i] - '0') * factor;
            factor = factor == 7 ? 2 : factor + 1;
        }

        var remainder = 11 - (sum % 11);

        return remainder switch
        {
            11 => '0',
            10 => 'K',
            _ => (char)('0' + remainder)
        };
    }

    [GeneratedRegex(@"[.\s-]")]
    private static partial Regex CleanupRegex();

    public override string ToString() => Value;
}
