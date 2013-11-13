using System;

namespace PrisonStep
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (PrisonGame game = new PrisonGame())
            {
                game.Run();
            }
        }
    }
}

