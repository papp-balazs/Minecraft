namespace Minecraft;

/// <summary>
/// The class containing the program entry point.
/// </summary>
public static class Program
{
    #region Public Static Methods

    /// <summary>
    /// The program entry point.
    /// </summary>
    public static void Main()
    {
        using var game = new Game();
        game.Init();
        game.Run();
    }

    #endregion
}