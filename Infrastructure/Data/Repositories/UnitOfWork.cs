using Domain.Interfaces;

namespace Infrastructure.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;
    private IWorkspaceRepository _workspaceRepository;
    private INoteRepository _noteRepository;

    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IWorkspaceRepository WorkspaceRepository => 
        _workspaceRepository ??= new WorkspaceRepository(_dbContext);

    public INoteRepository NoteRepository => 
        _noteRepository ??= new NoteRepository(_dbContext);

    public async Task<int> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}