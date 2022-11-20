using UnityEngine;

namespace _Project.Codebase
{
    public interface ICollectable
    {
        public void Collect();
        public Vector2 Velocity { get; set; }
        public Vector2 Position { get; set; }
    }
}