using System;

namespace sistdev
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (TPSGame game = new TPSGame())
            {
                game.Run();
            }
        }
    }
}

