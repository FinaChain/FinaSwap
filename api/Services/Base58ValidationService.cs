using System.Numerics;
using System.Security.Cryptography;

namespace FinaSwap.Api.Services;

public class Base58ValidationService : IBase58ValidationService
{
    private const int CheckSumSize = 4;
    private const string Digits = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

    public bool IsValid(string input)
    {
        try
        {
            var decoded = Decode(input);

            return decoded.Length > 0;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool IsValidPlain(string input)
    {
        try
        {
            var decoded = DecodePlain(input);

            return decoded.Length > 0;
        }
        catch (Exception)
        {
            return false;
        }
    }

    internal static string Encode(byte[] data)
    {
        return EncodePlain(_AddCheckSum(data));
    }

    internal static string EncodePlain(byte[] data)
    {
        var intData = data.Aggregate<byte, BigInteger>(0, (current, t) => current * 256 + t);
        var result = string.Empty;
        while (intData > 0)
        {
            var remainder = (int)(intData % 58);
            intData /= 58;
            result = Digits[remainder] + result;
        }

        for (var i = 0; i < data.Length && data[i] == 0; i++)
        {
            result = '1' + result;
        }

        return result;
    }

    internal static byte[] Decode(string data)
    {
        var dataWithCheckSum = DecodePlain(data);
        var dataWithoutCheckSum = _VerifyAndRemoveCheckSum(dataWithCheckSum);

        if (dataWithoutCheckSum == null)
        {
            throw new FormatException("Base58 checksum is invalid");
        }

        return dataWithoutCheckSum;
    }

    internal static byte[] DecodePlain(string data)
    {
        BigInteger intData = 0;
        for (var i = 0; i < data.Length; i++)
        {
            var digit = Digits.IndexOf(data[i]);

            if (digit < 0)
            {
                throw new FormatException($"Invalid Base58 character `{data[i]}` at position {i}");
            }

            intData = intData * 58 + digit;
        }

        var leadingZeroCount = data.TakeWhile(c => c == '1').Count();
        var leadingZeros = Enumerable.Repeat((byte)0, leadingZeroCount);
        var bytesWithoutLeadingZeros =
            intData.ToByteArray()
                .Reverse()
                .SkipWhile(b => b == 0);
        var result = leadingZeros.Concat(bytesWithoutLeadingZeros).ToArray();

        return result;
    }

    private static byte[] _AddCheckSum(byte[] data)
    {
        var checkSum = _GetCheckSum(data);
        var dataWithCheckSum = ConcatArrays(data, checkSum);

        return dataWithCheckSum;
    }

    private static byte[] _VerifyAndRemoveCheckSum(byte[] data)
    {
        var result = SubArray(data, 0, data.Length - CheckSumSize);
        var givenCheckSum = SubArray(data, data.Length - CheckSumSize);
        var correctCheckSum = _GetCheckSum(result);

        return givenCheckSum.SequenceEqual(correctCheckSum) ? result : Array.Empty<byte>();
    }

    private static byte[] _GetCheckSum(byte[] data)
    {
        var sha256 = SHA256.Create();
        var hash1 = sha256.ComputeHash(data);
        var hash2 = sha256.ComputeHash(hash1);

        var result = new byte[CheckSumSize];
        Buffer.BlockCopy(hash2, 0, result, 0, result.Length);

        return result;
    }

    internal static T[] ConcatArrays<T>(T[] arr1, T[] arr2)
    {
        var result = new T[arr1.Length + arr2.Length];
        Buffer.BlockCopy(arr1, 0, result, 0, arr1.Length);
        Buffer.BlockCopy(arr2, 0, result, arr1.Length, arr2.Length);

        return result;
    }

    internal static T[] SubArray<T>(T[] arr, int start, int length)
    {
        var result = new T[length];
        Buffer.BlockCopy(arr, start, result, 0, length);

        return result;
    }

    internal static T[] SubArray<T>(T[] arr, int start)
    {
        return SubArray(arr, start, arr.Length - start);
    }
}