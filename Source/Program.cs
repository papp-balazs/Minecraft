namespace Minecraft;

/// <summary>
/// The class containing the program entry point.
/// </summary>
public static class Program
{
    public const int ExitSuccess = 0;
    public const int ExitFail = 1;

    /// <summary>
    /// The program entry point.
    /// </summary>
    public static int Main()
    {
        using var game = new Game();

        try
        {
            game.Initialize();
            game.Run();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("Fatal error: {0}", e);
            return ExitFail;
        }
        finally
        {
            game.Dispose();
        }

        return ExitSuccess;
    }
};