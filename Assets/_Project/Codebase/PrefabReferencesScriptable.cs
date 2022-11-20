using UnityEngine;

namespace _Project.Codebase
{
    [CreateAssetMenu(fileName = "PrefabReferencesScriptable", menuName = "PrefabReferencesScriptable", order = 0)]
    public class PrefabReferencesScriptable : ScriptableObject
    {
        [field: SerializeField] public GameObject DistanceCollectable { get; private set; }
    }
}