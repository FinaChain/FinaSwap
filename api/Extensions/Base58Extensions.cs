using System.Numerics;
using System.Security.Cryptography;

namespace FinaSwap.Api.Extensions
{
    public static class Base58Extensions
    {
        public static string Base58Encode(this byte[] data)
        {
            var digits = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

            BigInteger intData = 0;
            for (var i = 0; i < data.Length; i++)
            {
                intData = intData * 256 + data[i];
            }

            var result = "";
            while (intData > 0)
            {
                var remainder = (int)(intData % 58);
                intData /= 58;
                result = digits[remainder] + result;
            }

            for (var i = 0; i < data.Length && data[i] == 0; i++)
            {
                result = '1' + result;
            }
            return result;
        }

        public static string Base58EncodeWithChecksum(this byte[] data)
        {
            return AddCheckSum(data).Base58Encode();
        }

        public const int CheckSumSizeInBytes = 4;

        public static byte[] AddCheckSum(byte[] data)
        {
            var checksum = GetCheckSum(data);

            var dataWithCheckSum = new byte[data.Length + checksum.Length];

            Buffer.BlockCopy(data, 0, dataWithCheckSum, 0, data.Length);
            Buffer.BlockCopy(checksum, 0, dataWithCheckSum, data.Length, checksum.Length);

            return dataWithCheckSum;
        }

        private static byte[] GetCheckSum(byte[] data)
        {
            using var sha256 = SHA256.Create();
            var hash1 = sha256.ComputeHash(data);
            var hash2 = sha256.ComputeHash(hash1);

            var result = new byte[CheckSumSizeInBytes];
            Buffer.BlockCopy(hash2, 0, result, 0, result.Length);

            return result;
        }

        public static byte[] Base58Decode(this string source)
        {
            var digits = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
            BigInteger intData = 0;
            for (var i = 0; i < source.Length; i++)
            {
                var digit = digits.IndexOf(source[i]);
                if (digit < 0)
                    throw new FormatException($"Invalid Base58 character `{source[i]}` at position {i}");
                intData = intData * 58 + digit;
            }

            var leadingZeroCount = source.TakeWhile(c => c == '1').Count();
            var leadingZeros = Enumerable.Repeat((byte)0, leadingZeroCount);
            var bytesWithoutLeadingZeros =
                intData.ToByteArray()
                    .Reverse()
                    .SkipWhile(b => b == 0);
            var result = leadingZeros.Concat(bytesWithoutLeadingZeros).ToArray();
            return result;
        }

        public static byte[] Base58DecodeWithCheckSum(this string source)
        {
            var dataWithCheckSum = Base58Decode(source);
            var dataWithoutCheckSum = VerifyAndRemoveCheckSum(dataWithCheckSum);
            if (dataWithoutCheckSum == null)
                throw new FormatException("Base58 checksum is invalid");
            return dataWithoutCheckSum;
        }

        private static byte[] VerifyAndRemoveCheckSum(byte[] data)
        {
            var result = SubArray(data, 0, data.Length - CheckSumSizeInBytes);
            var givenCheckSum = SubArray(data, data.Length - CheckSumSizeInBytes);
            var correctCheckSum = GetCheckSum(result);
            return givenCheckSum.SequenceEqual(correctCheckSum) ? result : Array.Empty<byte>();
        }

        private static T[] SubArray<T>(T[] arr, int start, int length)
        {
            var result = new T[length];
            Buffer.BlockCopy(arr, start, result, 0, length);
            return result;
        }

        private static T[] SubArray<T>(T[] arr, int start)
        {
            return SubArray(arr, start, arr.Length - start);
        }
    }
}
