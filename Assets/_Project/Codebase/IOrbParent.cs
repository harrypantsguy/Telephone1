using System.Collections.Generic;
using UnityEngine;

namespace _Project.Codebase
{
    public interface IOrbParent
    {
        public float Radius { get; }
        public Transform Transform { get; }
        public List<Orb> DirectChildrenOrbs { get; }

        public void AddOrbAsChild(in Orb newChildOrb);
        public void RemoveOrbAsChild(in Orb childOrb);
    }
}