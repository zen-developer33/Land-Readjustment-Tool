using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Infrastructure.Logging;

public class ProjectSession : IDisposable
{
    private readonly AppDbContext _dbcontext;
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
    public ProjectSession(string projectFilePath,AppDbContext context,IAppLogger logger)
    {
        ProjectFilePath = projectFilePath;

        ProjectFolderPath =
            Path.GetDirectoryName(projectFilePath)
            ?? throw new ArgumentException(
                "Invalid project file path.",
                nameof(projectFilePath));

        // Receive dependencies — do not create them
        _dbcontext = context;
        Logger = logger;

        Logger.LogInfo(
            $"Session opened: {projectFilePath}");
    }

    public AppDbContext GetDbContext()
    {
        ObjectDisposedException
            .ThrowIf(_disposed, nameof(ProjectSession));
        return _dbcontext;
    }

    public void Dispose()
    {
        if (_disposed) return;
        Logger.LogInfo(
            $"Session closing: {ProjectFilePath}");
        _dbcontext?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}