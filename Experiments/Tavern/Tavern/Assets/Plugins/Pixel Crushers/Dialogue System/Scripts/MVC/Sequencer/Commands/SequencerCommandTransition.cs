using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{

    /// <summary>
    /// Implements sequencer command: "Transition(assetPath[, duration])".
    /// 
    /// Arguments:
    /// -# assetPath - path in resources to asset with transition effect.
    /// -# (Optional) Duration in seconds. Default: 1.
    /// </summary>
    [AddComponentMenu( "" )] // Hide from menu.
    public class SequencerCommandTransition : SequencerCommand
    {
        private const float SmoothMoveCutoff = 0.05f;

        private string assetPath;
        private float duration;
        
        float startTime;
        float endTime;

        private static GameObject transitionObject = null;
        private static Transform parent = null;

        public void Awake()
        {
            // Get the values of the parameters:
            assetPath = GetParameter( 0 );
            duration = GetParameterAsFloat( 1, 1 );
            if( DialogueDebug.logInfo )
                Debug.Log( string.Format( System.Globalization.CultureInfo.InvariantCulture, "{0}: Sequencer: Transition({1}, {2})",
                                                                     new System.Object[] { DialogueDebug.Prefix, assetPath, duration } ) );

            if( duration > SmoothMoveCutoff ) {
                // Create fader canvas and image:
                if( parent == null ) {
                    parent = DialogueManager.instance.GetComponentInChildren<Canvas>().transform;
                    if( parent == null ) {
                        parent = DialogueManager.instance.transform;
                    }
                }

                if( transitionObject == null ) {
                    transitionObject = GameObject.Instantiate( Resources.Load<GameObject>( assetPath ) );
                    transitionObject.transform.SetParent( parent, false );
                }
                transitionObject.SetActive( true );

                // Set up duration:
                startTime = DialogueTime.time;
                endTime = startTime + duration;
            } else {
                Stop();
            }
        }


        public void Update()
        {
            if( (DialogueTime.time >= endTime) /*|| (transitionObject == null)*/ ) {
                Stop();
            }
        }


        public void OnDestroy()
        {
            if( transitionObject != null ) {
                Destroy( transitionObject );
                transitionObject = null;
            }
        }

    }
}