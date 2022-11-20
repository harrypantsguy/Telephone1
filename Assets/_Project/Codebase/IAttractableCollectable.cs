using System.Collections.Generic;
using UnityEngine;

namespace _Project.Codebase
{
    public interface IAttractableCollectable : ICollectable
    {
        public static List<IAttractableCollectable> attractableCollectables = new List<IAttractableCollectable>();
        public float PickUpDistance { get; set; }
        public float AttractionDistance { get; set; }
    }
}