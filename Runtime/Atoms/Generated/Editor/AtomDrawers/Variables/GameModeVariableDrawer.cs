#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.BaseAtoms.Editor
{
    /// <summary>
    /// Variable property drawer of type `GameMode`. Inherits from `AtomDrawer&lt;GameModeVariable&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(GameModeVariable))]
    public class GameModeVariableDrawer : VariableDrawer<GameModeVariable> { }
}
#endif
