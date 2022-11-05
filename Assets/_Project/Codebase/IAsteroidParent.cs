using System.Collections.Generic;
using UnityEngine;

namespace _Project.Codebase
{
    public interface IAsteroidParent
    {
        public float Radius { get; }
        public Transform Transform { get; }
        public List<Asteroid> DirectChildrenAsteroids { get; }

        public void AddAsteroidAsChild(in Asteroid newChildAsteroid);
        public void RemoveAsteroidAsChild(in Asteroid childAsteroid);
        public void GetAllChildAsteroidsNonAlloc(in List<Asteroid> asteroids);
        public void RemoveAllChildAsteroids();
    }
}