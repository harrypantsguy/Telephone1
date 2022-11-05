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

        public bool IsActive { get; private set; }
        public void SetActive(bool state)
        {
            gameObject.SetActive(state);
            
            if (!state)
                RemoveAllChildAsteroids();

            IsActive = state;
        }

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

        public void RemoveAllChildAsteroids()
        {
            for (var i = DirectChildrenAsteroids.Count - 1; i >= 0; i--)
            {
                Asteroid child = DirectChildrenAsteroids[i];
                child.SetParent(null);
                RemoveAsteroidAsChild(child);    
                child.RemoveAllChildAsteroids();
            }
        }
    }
}