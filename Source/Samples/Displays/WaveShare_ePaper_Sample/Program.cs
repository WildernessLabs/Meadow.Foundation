using Meadow;

namespace WaveShare_ePaper_Sample
{
    class Program
    {
        static IApp app;
        public static void Main(string[] args)
        {
            // instantiate and run new meadow app
            app = new WaveShareEPaperApp();
        }
    }
}