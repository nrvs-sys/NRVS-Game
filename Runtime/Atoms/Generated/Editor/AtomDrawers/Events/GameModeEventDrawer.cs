#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.BaseAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `GameMode`. Inherits from `AtomDrawer&lt;GameModeEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(GameModeEvent))]
    public class GameModeEventDrawer : AtomDrawer<GameModeEvent> { }
}
#endif
