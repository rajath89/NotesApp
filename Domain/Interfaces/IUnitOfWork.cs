namespace Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IWorkspaceRepository WorkspaceRepository { get; }
    INoteRepository NoteRepository { get; }
    Task<int> SaveChangesAsync();
}