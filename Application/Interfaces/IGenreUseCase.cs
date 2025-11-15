using Application.DTOs;
using Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IGenreUseCase
    {
        Task<OpResult> AddGenre(AddGenreDTO genreDTO);
    }
}
