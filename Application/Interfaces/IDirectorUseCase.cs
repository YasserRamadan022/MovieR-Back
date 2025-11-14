using Application.DTOs;
using Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IDirectorUseCase
    {
        Task<OpResult> AddDirector(AddDirectorDTO directorDTO);
        Task<OpResult> GetDirectorMovies(int directorId, int pageNumber = 1, int pageSize = 10);
    }
}
