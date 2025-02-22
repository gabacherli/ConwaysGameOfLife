﻿namespace GameOfLife.API.Models
{
    public class FinalIterationResponse
    {
        public required Board Board { get; set; }
        public int Iterations { get; set; }
        public EndReason EndReason { get; set; }
    }
}
