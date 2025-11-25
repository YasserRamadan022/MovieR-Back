using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Core.Domain.Common;
using Core.Ports;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class SearchUseCase : ISearchUseCase
    {
        private readonly ISearchRepository _searchRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<SearchUseCase> _logger;
        public SearchUseCase(ISearchRepository searchRepository, ILogger<SearchUseCase> logger, IMapper mapper)
        {
            _searchRepository = searchRepository;
            _logger = logger;
            _mapper = mapper;
        }
        public async Task<OpResult> SearchMoviesAsync(string query, int page = 1, int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            _logger.LogInformation("Search request: Query='{Query}', Page={Page}, PageSize={PageSize}",
                query, page, pageSize);

            var result = await _searchRepository.SearchMoviesAsync(query, page, pageSize);
            var moviesList = _mapper.Map<List<MoviesDTO>>(result.Data);

            _logger.LogInformation("Search completed: {Count} results found", result.TotalCount);

            return new OpResult() { Success = true, Message = "Data retrieved successfully", StatusCode = 200, Data = moviesList };
        }
    }
}
