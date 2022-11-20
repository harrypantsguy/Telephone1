using System.Collections.Generic;

namespace _Project.Codebase
{
    public interface ICollector
    {
        public static List<ICollector> collectors = new List<ICollector>();

        public void CollectCollectable(ICollectable collectable);
    }
}