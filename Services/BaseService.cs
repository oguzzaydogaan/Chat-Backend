using AutoMapper;
using Repositories.Entities;
using Repositories.Repositories;

namespace Services
{
    public abstract class BaseService<TEntity, TDTO> where TEntity : BaseEntity where TDTO : class
    {
        protected readonly IMapper _mapper;
        protected readonly BaseRepository<TEntity> _repository;
        public BaseService(IMapper mapper, BaseRepository<TEntity> repository)
        {
            _mapper = mapper;
            _repository = repository;
        }
        public async Task<List<TDTO>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            var dtos = entities.Select(e => _mapper.Map<TDTO>(e)).ToList();
            return dtos;
        }

        public async Task<TDTO> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException("Entity not found");
            return _mapper.Map<TDTO>(entity);
        }

        public async Task<int> DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
            return id;
        }

        public async Task<TEntity> AddWithoutSaveAsync(TEntity entity)
        {
            return await _repository.AddWithoutSaveAsync(entity);
        }

        public async Task SaveChangesAsync()
        {
            await _repository.SaveChangesAsync();
        }
    }
}
