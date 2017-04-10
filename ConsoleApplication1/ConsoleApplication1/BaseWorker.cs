using System;

namespace ConsoleApplication1
{
    public class BaseWorker
    {
        Scene _scene;

        public BaseWorker(Scene scene)
        {
            if(scene == null)
                throw new ArgumentNullException(nameof(scene));

            _scene = scene;
        }
    }
}