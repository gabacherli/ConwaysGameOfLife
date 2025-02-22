using GameOfLife.API.Models;
using System.Security.Cryptography;

namespace GameOfLife.API.Helpers
{
    public static class BoardHelper
    {
        public static byte[] ConvertToBinary(List<List<bool>> state)
        {
            return state.SelectMany(row => row.Select(cell => (byte)(cell ? 1 : 0))).ToArray();
        }

        public static byte[] ComputeStateHash(byte[] stateBinary)
        {
            return SHA256.HashData(stateBinary);
        }

        public static List<List<bool>> GetNextTick(Board board, int rows, int columns, out byte[] stateHash)
        {
            var nextTick = new List<List<bool>>();

            for (int row = 0; row < rows; row++)
            {
                nextTick.Add(new List<bool>());
                for (int col = 0; col < columns; col++)
                {
                    int liveNeighbors = CountLiveNeighbors(board.State, row, col, rows, columns);
                    bool isAlive = board.State[row][col];

                    if (isAlive)
                    {
                        nextTick[row].Add(liveNeighbors is 2 or 3);
                    }
                    else
                    {
                        nextTick[row].Add(liveNeighbors is 3);
                    }
                }
            }

            stateHash = ComputeStateHash(ConvertToBinary(nextTick));

            return nextTick;
        }

        public static int CountLiveNeighbors(List<List<bool>> state, int row, int col, int rows, int columns)
        {
            int count = 0;
            int[] directions = { -1, 0, 1 };

            foreach (var dRow in directions)
            {
                foreach (var dCol in directions)
                {
                    if (dRow is 0 && dCol is 0) continue;

                    int newRow = row + dRow;
                    int newCol = col + dCol;

                    if (newRow >= 0 && newRow < rows && newCol >= 0 && newCol < columns && state[newRow][newCol])
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        public static List<List<bool>> ConvertFromBinary(byte[] binaryState, int rows, int columns)
        {
            if (binaryState is null || binaryState.Length != rows * columns)
            {
                throw new ArgumentException("Binary state does not match expected dimensions.");
            }

            var state = new List<List<bool>>();
            int index = 0;

            for (int row = 0; row < rows; row++)
            {
                var rowList = new List<bool>();
                for (int col = 0; col < columns; col++)
                {
                    rowList.Add(binaryState[index++] is 1);
                }
                state.Add(rowList);
            }

            return state;
        }
    }
}
