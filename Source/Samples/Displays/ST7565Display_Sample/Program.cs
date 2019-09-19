using Meadow;
using TftSpiDisplay_Sample;

namespace BasicST7565Display_Sample
{
    class Program
    {
        static IApp app;
        public static void Main(string[] args)
        {
            // instantiate and run new meadow app
            app = new ST7565DisplayApp();
        }
    }
}
