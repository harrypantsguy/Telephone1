using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace _Project.Codebase
{
    public sealed class StasisBarrier : MonoBehaviour, IAsteroidParent
    {
        [field: SerializeField] public float Radius { get; private set; }
        public List<Asteroid> DirectChildrenAsteroids { get; } = new List<Asteroid>();
        public Transform Transform => transform;
        public int TotalNumChildAsteroids =>
            DirectChildrenAsteroids.Sum(childAsteroid => childAsteroid.TotalChildAsteroidCount) + DirectChildrenAsteroids.Count;
        
        private readonly List<Asteroid> _allChildAsteroids = new List<Asteroid>();

        [UsedImplicitly]
        private void Update()
        {
            _allChildAsteroids.Clear();
            GetAllChildAsteroidsNonAlloc(_allChildAsteroids);
        }

        public void AddAsteroidAsChild(in Asteroid newChildAsteroid)
        {
            DirectChildrenAsteroids.Add(newChildAsteroid);
            newChildAsteroid.SetParent(this);
        }

        public void RemoveAsteroidAsChild(in Asteroid childAsteroid)
        {
            DirectChildrenAsteroids.Remove(childAsteroid);
        }

        public void GetAllChildAsteroidsNonAlloc(in List<Asteroid> asteroids)
        {
            asteroids.AddRange(DirectChildrenAsteroids);
            foreach (var childAsteroid in DirectChildrenAsteroids)
                childAsteroid.GetAllChildAsteroidsNonAlloc(asteroids);
        }
    }
}