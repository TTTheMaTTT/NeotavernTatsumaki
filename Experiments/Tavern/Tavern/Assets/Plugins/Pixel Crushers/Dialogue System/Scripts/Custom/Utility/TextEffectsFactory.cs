using UnityEngine.Assertions;

namespace PixelCrushers.DialogueSystem
{
    /// <summary>
    /// Creates ibject with the interface ITextEffect which corresponds to passed params
    /// </summary>
    public class TextEffectsFactory
    {
        public ITextEffect CreateEffect( TextEffectParams effectParams )
        {
            switch( effectParams.EffectType ) {
                case TextEffectType.Color_Blink:
                    return new ColorBlinkEffect();
                case TextEffectType.Color_Transition:
                    return new ColorTransitionEffect();
                case TextEffectType.Color_Rainbow:
                    return new ColorRainbowEffect();
                case TextEffectType.Animation_Wave:
                    return new WaveEffect();
                case TextEffectType.Animation_Wobble:
                    return new WobbleEffect();
                case TextEffectType.Animation_Shake:
                    return new ShakeEffect();
                case TextEffectType.Animation_SizeTransition:
                    return new SizeTransitionEffect();
                default:
                    Assert.IsTrue( false );
                    return null;
            }
        }
    }
}
