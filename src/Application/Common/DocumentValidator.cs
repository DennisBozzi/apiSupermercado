using ApiSupermercado.Domain.Entities;

namespace ApiSupermercado.Application.Common;

public static class DocumentValidator
{
    public static bool IsValid(string? document, DocumentType type) => type switch
    {
        DocumentType.Cpf => IsValidCpf(document),
        DocumentType.Cnpj => IsValidCnpj(document),
        _ => false,
    };

    public static string Normalize(string document) => new(document.Where(char.IsDigit).ToArray());

    public static bool IsValidCpf(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;
        var cpf = Normalize(input);
        if (cpf.Length != 11) return false;
        if (cpf.Distinct().Count() == 1) return false;

        int Sum(int len)
        {
            var s = 0;
            for (var i = 0; i < len; i++) s += (cpf[i] - '0') * (len + 1 - i);
            return s;
        }

        int Digit(int sum)
        {
            var r = sum % 11;
            return r < 2 ? 0 : 11 - r;
        }

        var d1 = Digit(Sum(9));
        var d2 = Digit(Sum(10));
        return cpf[9] - '0' == d1 && cpf[10] - '0' == d2;
    }

    public static bool IsValidCnpj(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;
        var cnpj = Normalize(input);
        if (cnpj.Length != 14) return false;
        if (cnpj.Distinct().Count() == 1) return false;

        int[] m1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] m2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

        int Sum(int[] m, int len)
        {
            var s = 0;
            for (var i = 0; i < len; i++) s += (cnpj[i] - '0') * m[i];
            return s;
        }

        int Digit(int sum)
        {
            var r = sum % 11;
            return r < 2 ? 0 : 11 - r;
        }

        var d1 = Digit(Sum(m1, 12));
        var d2 = Digit(Sum(m2, 13));
        return cnpj[12] - '0' == d1 && cnpj[13] - '0' == d2;
    }
}
