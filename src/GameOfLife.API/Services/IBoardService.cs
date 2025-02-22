﻿using GameOfLife.API.Models;

namespace GameOfLife.API.Services
{
    public interface IBoardService
    {
        Task<Guid> InsertBoardAsync(Board board);
    }
}