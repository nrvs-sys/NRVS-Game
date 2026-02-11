#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityEngine.UIElements;
using UnityAtoms.Editor;

namespace UnityAtoms.BaseAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `GameMode`. Inherits from `AtomEventEditor&lt;GameMode, GameModeEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomEditor(typeof(GameModeEvent))]
    public sealed class GameModeEventEditor : AtomEventEditor<GameMode, GameModeEvent> { }
}
#endif
