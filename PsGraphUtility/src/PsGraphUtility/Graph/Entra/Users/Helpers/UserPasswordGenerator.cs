// src/PsGraphUtility/Graph/Users/Helpers/UserPasswordGenerator.cs
using System;
using System.Security.Cryptography;
using System.Text;

namespace PsGraphUtility.Graph.Entra.Users.Helpers;

public static class UserPasswordGenerator
{
    private static readonly char[] Vowels = "aeiou".ToCharArray();
    private static readonly char[] Consonants = "bcdfghjklmnpqrstvwxyz".ToCharArray();
    private static readonly char[] LowerLetters = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
    private static readonly char[] Digits = "0123456789".ToCharArray();

    public static string GeneratePassword()
    {
        // Pattern: Uppercase consonant + vowel + lowercase + 5 digits (e.g. Kop99123 style)
        Span<byte> bytes = stackalloc byte[8];
        RandomNumberGenerator.Fill(bytes);

        char upperConsonant = char.ToUpper(Consonants[bytes[0] % Consonants.Length]);
        char vowel = Vowels[bytes[1] % Vowels.Length];
        char lower = LowerLetters[bytes[2] % LowerLetters.Length];

        var sb = new StringBuilder();
        sb.Append(upperConsonant);
        sb.Append(vowel);
        sb.Append(lower);

        for (int i = 3; i < 8; i++)
            sb.Append(Digits[bytes[i] % Digits.Length]);

        return sb.ToString();
    }
}
