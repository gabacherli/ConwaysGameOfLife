namespace GameOfLife.API.Models
{
    public enum EndReason
    {
        Stable = 1,
        Loop = 2,
        MaxIterationsReached = 3
    }
}
