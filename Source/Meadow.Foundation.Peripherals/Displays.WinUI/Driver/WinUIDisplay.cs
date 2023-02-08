using Microsoft.UI;
using System.Numerics;
using Windows.ApplicationModel.Core;
using Windows.UI.Composition;
using Windows.UI.Core;

namespace Meadow.WinUI
{
    class WinUIDisplay : IFrameworkView, IFrameworkViewSource
    {
        public IFrameworkView CreateView()
        {
            return this;
        }
        public void SetWindow(CoreWindow window)
        {
            this.compositor = new Compositor();
            this.compositionTarget = this.compositor.CreateTargetForCurrentView();

            // Make a visual which is a container to put other visuals into.
            ContainerVisual container = this.compositor.CreateContainerVisual();
            this.compositionTarget.Root = container;

            // Make a visual which paints itself with a brush.
            SpriteVisual visual = this.compositor.CreateSpriteVisual();

            // Tell it where it is.
            visual.Size = new Vector2(100, 100);
            visual.Offset = new Vector3(10, 10, 0);

            // Tell it how to paint itself
            visual.Brush = this.compositor.CreateColorBrush(Colors.Red);

            // Put it into the container.
            container.Children.InsertAtTop(visual);
        }
        public void Run()
        {
            CoreWindow window = CoreWindow.GetForCurrentThread();
            window.Activate();
            window.Dispatcher.ProcessEvents(CoreProcessEventsOption.ProcessUntilQuit);
        }
        public void Initialize(CoreApplicationView applicationView)
        {
        }
        public void Load(string entryPoint)
        {
        }
        public void Uninitialize()
        {
        }
        CompositionTarget compositionTarget;
        Compositor compositor;

    }
}