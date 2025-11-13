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
    public class ActorUseCase: IActorUseCase
    {
        private readonly IGenericRepository<Actor> _actorRepository;
        private readonly ILogger<ActorUseCase> _logger;
        private readonly IMapper _mapper;
        public ActorUseCase(IGenericRepository<Actor> actorRepository, ILogger<ActorUseCase> logger, IMapper mapper)
        {
            _actorRepository = actorRepository ?? throw new ArgumentNullException(nameof(actorRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<OpResult> AddActor(AddActorDTO actorDTO)
        {
            if(actorDTO == null)
            {
                _logger.LogWarning("Attempted to use null actor dto");
                return new OpResult() { Success = false, Message = "Invalid request data", StatusCode = 400 };
            }

            try
            {
                var newActor = _mapper.Map<Actor>(actorDTO);
                var result = await _actorRepository.AddAsync(newActor);

                return new OpResult() { Success = true, Message = "Actor added successfully", StatusCode = 201 };
            }
            catch (RepositoryException ex)
            {
                if (ex.Message.Contains("already exists"))
                {
                    return new OpResult() { Success = false, Message = "This actor already exists", StatusCode = 409 };
                }
                return new OpResult() { Success = false, Message = "Something went wrong", StatusCode = 500 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while adding actor");
                return new OpResult() { Success = false, Message = "Something went wrong", StatusCode = 500 };
            }
        }
    }
}
