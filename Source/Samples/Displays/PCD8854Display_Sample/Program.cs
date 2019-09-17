using Meadow;

namespace BasicPCD8843Display_Sample
{
    class Program
    {
        static IApp app;
        public static void Main(string[] args)
        {
            // instantiate and run new meadow app
            app = new PCD8854DisplayApp();
        }
    }
}
