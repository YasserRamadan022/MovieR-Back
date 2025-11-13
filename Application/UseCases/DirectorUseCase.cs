using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Core.Domain.Common;
using Core.Domain.Common.RepositoryException;
using Core.Domain.Entities;
using Core.Ports;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class DirectorUseCase: IDirectorUseCase
    {
        private readonly IGenericRepository<Director> _directorRepository;
        private readonly ILogger<DirectorUseCase> _logger;
        private readonly IMapper _mapper;

        public DirectorUseCase(IGenericRepository<Director> directorRepository, ILogger<DirectorUseCase> logger, IMapper mapper)
        {
            _directorRepository = directorRepository ?? throw new ArgumentNullException(nameof(directorRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<OpResult> AddDirector(AddDirectorDTO directorDTO)
        {
            if(directorDTO == null)
            {
                _logger.LogWarning("Attempted to use null director dto");
                return new OpResult() { Success = false, Message = "Invalid request data", StatusCode = 400 };
            }

            try
            {
                var newDirector = _mapper.Map<Director>(directorDTO);
                var result = await _directorRepository.AddAsync(newDirector);

                return new OpResult() { Success = true, Message = "Director added successfully", StatusCode = 201 };
            }
            catch (RepositoryException ex)
            {
                if (ex.Message.Contains("already exists"))
                {
                    return new OpResult() { Success = false, Message = "This director already exists", StatusCode = 409 };
                }
                return new OpResult() { Success = false, Message = "Something went wrong", StatusCode = 500 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while adding director");
                return new OpResult() { Success = false, Message = "Something went wrong", StatusCode = 500 };
            }
        }
    }
}
