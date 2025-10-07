namespace ModManager;

public static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(defaultValue: false);
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.Run(new GUI());
    }
}
