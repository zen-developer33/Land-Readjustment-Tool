using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Infrastructure.Logging;
using Microsoft.EntityFrameworkCore.Storage;

public class ProjectSession : IDisposable
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _activeTransaction;
    private bool _disposed = false;

    public string ProjectFilePath { get; }
    public string ProjectFolderPath { get; }

    /// <summary>
    /// Logger injected from outside.
    /// ProjectSession does not know or care
    /// whether it is FileLogger, ConsoleLogger,
    /// CompositeLogger or a test mock.
    /// </summary>
    public IAppLogger Logger { get; }

    /// <summary>
    /// Receives all dependencies through constructor.
    /// Nothing is created inside this class.
    /// 
    /// This is constructor injection — the correct
    /// way to implement dependency injection.
    /// 
    /// The caller decides:
    /// → which logger implementation to use
    /// → which context to use
    /// This class just uses what it receives.
    /// </summary>
    public ProjectSession(
        string projectFilePath,
        AppDbContext context,
        IAppLogger logger)
    {
        ProjectFilePath = projectFilePath;

        ProjectFolderPath =
            Path.GetDirectoryName(projectFilePath)
            ?? throw new ArgumentException(
                "Invalid project file path.",
                nameof(projectFilePath));

        // Receive dependencies — do not create them
        _context = context;
        Logger = logger;

        Logger.LogInfo(
            $"Session opened: {projectFilePath}");
    }

    public AppDbContext GetContext()
    {
        ObjectDisposedException
            .ThrowIf(_disposed, nameof(ProjectSession));
        return _context;
    }

    public void BeginUserTransaction()
    {
        ObjectDisposedException
            .ThrowIf(_disposed, nameof(ProjectSession));

        if (_activeTransaction != null) return;

        _activeTransaction = _context.Database
            .BeginTransaction();

        Logger.LogInfo("User transaction started.");
    }

    public void CommitUserTransaction()
    {
        ObjectDisposedException
            .ThrowIf(_disposed, nameof(ProjectSession));

        if (_activeTransaction == null)
            return;

        _activeTransaction.Commit();
        _activeTransaction.Dispose();
        _activeTransaction = null;

        Logger.LogInfo("User transaction committed.");

        BeginUserTransaction();
    }

    public async Task CommitUserTransactionAsync()
    {
        ObjectDisposedException
            .ThrowIf(_disposed, nameof(ProjectSession));

        if (_activeTransaction == null)
            return;

        await _activeTransaction.CommitAsync();
        await _activeTransaction.DisposeAsync();
        _activeTransaction = null;

        Logger.LogInfo("User transaction committed.");

        BeginUserTransaction();
    }

    public void RollbackUserTransaction()
    {
        ObjectDisposedException
            .ThrowIf(_disposed, nameof(ProjectSession));

        if (_activeTransaction == null)
            return;

        _activeTransaction.Rollback();
        _activeTransaction.Dispose();
        _activeTransaction = null;

        _context.ChangeTracker.Clear();
        Logger.LogInfo("User transaction rolled back.");

        BeginUserTransaction();
    }

    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            if (_activeTransaction != null)
            {
                _activeTransaction.Rollback();
                _activeTransaction.Dispose();
                _activeTransaction = null;
            }
        }
        catch
        {
            // Ignore rollback errors during dispose.
        }

        Logger.LogInfo(
            $"Session closing: {ProjectFilePath}");
        _context?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}