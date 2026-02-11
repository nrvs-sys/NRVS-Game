using UnityEngine;

namespace UnityAtoms.BaseAtoms
{
    /// <summary>
    /// Event of type `GameMode`. Inherits from `AtomEvent&lt;GameMode&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(menuName = "Unity Atoms/Events/GameMode", fileName = "GameModeEvent")]
    public sealed class GameModeEvent : AtomEvent<GameMode>
    {
    }
}
