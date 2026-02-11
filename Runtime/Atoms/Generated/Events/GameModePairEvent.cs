using UnityEngine;

namespace UnityAtoms.BaseAtoms
{
    /// <summary>
    /// Event of type `GameModePair`. Inherits from `AtomEvent&lt;GameModePair&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(menuName = "Unity Atoms/Events/GameModePair", fileName = "GameModePairEvent")]
    public sealed class GameModePairEvent : AtomEvent<GameModePair>
    {
    }
}
