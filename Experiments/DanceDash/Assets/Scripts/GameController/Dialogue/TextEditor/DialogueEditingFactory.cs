using UnityEngine;
using UnityEngine.Assertions;

namespace Dialogue { 
    /// <summary>
    /// Создаёт объект с интерфейсом IDialogueEditing, соответствующий переданным параметрам
    /// </summary>
    public class CDialogueEditingFactory
    {
        public IDialogueEditing CreateEditing( CDialogueEditingParams editingParams )
        {
            switch( editingParams.EditingType ) {
                case TEditingType.Color:
                    return new CColorEditing();
                case TEditingType.Color_Blink:
                    return new CColorBlinkEditing();
                case TEditingType.Color_Transition:
                    return new CColorTransitionEditing();
                case TEditingType.Color_Rainbow:
                    return new CColorRainbowEditing();
                case TEditingType.Animation_Wave:
                    return new CWaveEditing();
                case TEditingType.Animation_Wobble:
                   return new CWobbleEditing();
                case TEditingType.Animation_Shake:
                    return new CShakeEditing();
                case TEditingType.Animation_SizeTransition:
                    return new CSizeTransitionEditing();
                default:
                    Assert.IsTrue( false );
                    return null;
            }
        }
    }
}
