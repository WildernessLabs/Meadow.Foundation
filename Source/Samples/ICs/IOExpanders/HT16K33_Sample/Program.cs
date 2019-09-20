using Meadow;

namespace BasicHT16K33_Sample
{
    class Program
    {
        static IApp app;
        public static void Main(string[] args)
        {
            // instantiate and run new meadow app
            app = new HT16K33App();
        }
    }
}
