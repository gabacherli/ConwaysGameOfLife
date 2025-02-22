using System.Security.Cryptography;

namespace GameOfLife.API.Helpers
{
    public static class BoardHelpers
    {
        public static byte[] ConvertToBinary(List<List<bool>> state)
        {
            return state.SelectMany(row => row.Select(cell => (byte)(cell ? 1 : 0))).ToArray();
        }

        public static byte[] ComputeStateHash(byte[] stateBinary)
        {
            return SHA256.HashData(stateBinary);
        }
    }
}
