using System;

namespace MAS.Core
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