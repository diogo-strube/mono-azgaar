using System;

namespace MonoAzgaar
{
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            using (var game = new MonoAzgaar(args))
                game.Run();
        }
    }
}
