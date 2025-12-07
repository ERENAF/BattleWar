using SeaWar.forms;
namespace SeaWar
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            GameForm p2 = new GameForm();
            p2.Show();
            Application.Run(new GameForm());
            
        }
    }
}