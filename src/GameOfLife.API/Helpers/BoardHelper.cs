using GameOfLife.API.Models;
using System.Security.Cryptography;

namespace GameOfLife.API.Helpers
{
    public static class BoardHelper
    {
        /// <summary>
        /// Converts the board state to a binary representation.
        /// </summary>
        /// <param name="state">The current state of the board.</param>
        /// <returns>A byte array representing the binary state of the board.</returns>
        public static byte[] ConvertToBinary(List<List<bool>> state)
        {
            return state.SelectMany(row => row.Select(cell => (byte)(cell ? 1 : 0))).ToArray();
        }

        /// <summary>
        /// Computes the SHA256 hash of the given binary state.
        /// </summary>
        /// <param name="stateBinary">The binary state of the board.</param>
        /// <returns>A byte array representing the hash of the state.</returns>
        public static byte[] ComputeStateHash(byte[] stateBinary)
        {
            return SHA256.HashData(stateBinary);
        }

        /// <summary>
        /// Computes the next iteration of the board state.
        /// </summary>
        /// <param name="board">The current board.</param>
        /// <param name="stateHash">The hash of the next iteration state.</param>
        /// <returns>The next iteration of the board state.</returns>
        public static List<List<bool>> GetNextIteration(Board board, out byte[] stateHash)
        {
            var nextIteration = new List<List<bool>>();

            for (int row = 0; row < board.Rows; row++)
            {
                nextIteration.Add(new List<bool>());
                for (int col = 0; col < board.Columns; col++)
                {
                    int liveNeighbors = CountLiveNeighbors(board.State, row, col, board.Rows, board.Columns);
                    bool isAlive = board.State[row][col];

                    if (isAlive)
                    {
                        nextIteration[row].Add(liveNeighbors is 2 or 3);
                    }
                    else
                    {
                        nextIteration[row].Add(liveNeighbors is 3);
                    }
                }
            }

            stateHash = ComputeStateHash(ConvertToBinary(nextIteration));

            return nextIteration;
        }

        /// <summary>
        /// Counts the number of live neighbors for a given cell.
        /// </summary>
        /// <param name="state">The current state of the board.</param>
        /// <param name="row">The row index of the cell.</param>
        /// <param name="col">The column index of the cell.</param>
        /// <param name="rows">The total number of rows in the board.</param>
        /// <param name="columns">The total number of columns in the board.</param>
        /// <returns>The number of live neighbors.</returns>
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

        /// <summary>
        /// Converts a binary state back to a board state.
        /// </summary>
        /// <param name="binaryState">The binary state of the board.</param>
        /// <param name="rows">The total number of rows in the board.</param>
        /// <param name="columns">The total number of columns in the board.</param>
        /// <returns>The board state as a list of lists of booleans.</returns>
        /// <exception cref="ArgumentException">Thrown when the binary state does not match the expected dimensions.</exception>
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

        /// <summary>
        /// Computes the board state after a given number of iterations.
        /// </summary>
        /// <param name="board">The current board.</param>
        /// <param name="iterations">The number of iterations to compute.</param>
        /// <param name="stateHash">The hash of the final state.</param>
        /// <returns>The board state after the given number of iterations.</returns>
        public static List<List<bool>> GetBoardAfterNIterations(Board board, int iterations, out byte[] stateHash)
        {
            for (int i = 0; i < iterations; i++)
            {
                board.State = GetNextIteration(board, out _);
            }

            stateHash = ComputeStateHash(ConvertToBinary(board.State));

            return board.State;
        }

        /// <summary>
        /// Computes the stable or final iteration of the board state.
        /// </summary>
        /// <param name="board">The current board.</param>
        /// <param name="maxIterations">The maximum number of iterations to compute.</param>
        /// <param name="currentHash">The hash of the final state.</param>
        /// <param name="endReason">The reason for ending the iterations (Stable, Loop, or MaxIterationsReached).</param>
        /// <returns>The board state after the final iteration.</returns>
        public static (List<List<bool>>, int) GetStableOrFinalIteration(Board board, int maxIterations, out byte[] currentHash, out EndReason endReason)
        {
            var iterationIndex = 0;
            currentHash = board.StateHash;
            var hashes = new HashSet<byte[]>();

            while (iterationIndex < maxIterations)
            {
                if (hashes.Contains(currentHash))
                {
                    endReason = EndReason.Loop;
                    return (board.State, iterationIndex);
                }

                hashes.Add(currentHash);
                _ = GetNextIteration(board, out var nextIterationHash);

                if (currentHash == nextIterationHash)
                {
                    endReason = EndReason.Stable;
                    return (board.State, iterationIndex);
                }

                currentHash = nextIterationHash;
                iterationIndex++;
            }

            endReason = EndReason.MaxIterationsReached;
            return (board.State, iterationIndex);
        }
    }
}