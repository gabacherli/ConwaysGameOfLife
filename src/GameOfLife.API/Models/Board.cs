using GameOfLife.API.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GameOfLife.API.Models
{
    public class Board
    {
        [JsonIgnore]
        public Guid Id { get; set; }
        [Required, Range(1, 200, ErrorMessage = $"{nameof(Rows)} must be a positive integer between 1 and 200.")]
        public int Rows { get; set; }
        [Required, Range(1, 200, ErrorMessage = $"{nameof(Columns)} must be a positive integer between 1 and 200.")]
        public int Columns { get; set; }
        [Required, ValidateState]
        public List<List<bool>> State { get; set; } = new();
        [JsonIgnore]
        public byte[] StateBinary => BoardHelpers.ConvertToBinary(State);
        [JsonIgnore]
        public byte[] StateHash => BoardHelpers.ComputeStateHash(StateBinary);

        /// <summary>
        /// Custom validation attribute for board state validation.
        /// </summary>
        public class ValidateState : ValidationAttribute
        {
            protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
            {
                var board = (Board)validationContext.ObjectInstance;

                if (board.State is null)
                {
                    return new ValidationResult("Board state cannot be null.");
                }

                if (board.State.Count != board.Rows)
                {
                    return new ValidationResult($"Row count mismatch: Expected {board.Rows}, but got {board.State.Count}.");
                }

                // Find rows with incorrect column count
                var invalidRows = board.State
                    .Select((row, index) => (index, row.Count))
                    .Where(x => x.Count != board.Columns)
                    .ToList();

                if (invalidRows.Any())
                {
                    var invalidRowIndexes = string.Join(", ", invalidRows.Select(x => $"Row {x.index} (Expected {board.Columns}, Found {x.Count})"));
                    return new ValidationResult($"Board state column count mismatch in {invalidRows.Count} row(s): {invalidRowIndexes}.");
                }

                return ValidationResult.Success;
            }
        }
    }
}
