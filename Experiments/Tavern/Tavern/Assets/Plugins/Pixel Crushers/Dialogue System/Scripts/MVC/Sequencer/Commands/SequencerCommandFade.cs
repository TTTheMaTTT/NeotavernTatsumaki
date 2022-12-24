// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{

    /// <summary>
    /// Implements sequencer command: "Fade(in|out, [, duration[, webcolor[, sortOrder]]])".
    /// 
    /// Arguments:
    /// -# in or out.
    /// -# (Optional) Duration in seconds. Default: 1.
    /// -# (Optional) Web color in "\#rrggbb" format. Default: Black.
    /// -# (Optional) Sort order of render. Default: 32766.
    /// </summary>
    [AddComponentMenu("")] // Hide from menu.
    public class SequencerCommandFade : SequencerCommand
    {

        private const float SmoothMoveCutoff = 0.05f;
        private const int DefaultFaderCanvasSortOrder = 32766;

        private string direction;
        private float duration;
        private Color color;
        private int sortOrder;
        private bool fadeIn;
        private bool stay;
        private bool unstay;
        float startTime;
        float endTime;

        private static Canvas faderCanvas = null;
        private static UnityEngine.UI.Image faderImage = null;

        public void Awake()
        {
            // Get the values of the parameters:
            direction = GetParameter(0);
            duration = GetParameterAsFloat(1, 1);
            color = Tools.WebColor(GetParameter(2, "#000000"));
            sortOrder = GetParameterAsInt( 3, DefaultFaderCanvasSortOrder );
            bool canOverrideSortOrder = true;
            if (DialogueDebug.logInfo) Debug.Log(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}: Sequencer: Fade({1}, {2}, {3}, {4})", 
                                                                                            new System.Object[] { DialogueDebug.Prefix, direction, duration, color, sortOrder }));

            stay = string.Equals(direction, "stay", System.StringComparison.OrdinalIgnoreCase);
            unstay = string.Equals(direction, "unstay", System.StringComparison.OrdinalIgnoreCase);
            fadeIn = unstay || string.Equals(direction, "in", System.StringComparison.OrdinalIgnoreCase);

            if (unstay && faderImage != null && Mathf.Approximately(0, faderImage.color.a))
            {
                Stop(); // Image is already invisible, so no need to fade in.
            }
            else if (duration > SmoothMoveCutoff)
            {

                // Create fader canvas and image:
                if (faderCanvas == null)
                {
                    faderCanvas = new GameObject("Canvas (Fader)", typeof(Canvas)).GetComponent<Canvas>();
                    var parent = DialogueManager.instance.GetComponentInChildren<Canvas>().transform;
                    if( parent == null ) {
                        parent = DialogueManager.instance.transform;
                        faderCanvas.transform.SetParent( parent );
                        faderCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                        canOverrideSortOrder = false;
                    } else {
                        faderCanvas.transform.SetParent( parent );
                        faderCanvas.overrideSorting = true;
                    }
                }
                if (faderImage == null)
                {
                    faderImage = new GameObject("Fader Image", typeof(UnityEngine.UI.Image)).GetComponent<UnityEngine.UI.Image>();
                    faderImage.transform.SetParent(faderCanvas.transform, false);
                    faderImage.rectTransform.anchorMin = Vector2.zero;
                    faderImage.rectTransform.anchorMax = Vector2.one;
                    faderImage.sprite = null;
                }
                if( canOverrideSortOrder ) {
                    faderCanvas.sortingOrder = sortOrder;
                }
                faderCanvas.gameObject.SetActive(true);
                faderImage.gameObject.SetActive(true);

                // Set up duration:
                startTime = DialogueTime.time;
                endTime = startTime + duration;

                faderImage.color = new Color(color.r, color.g, color.b, fadeIn ? 1 : 0);

            }
            else
            {

                Stop();
            }
        }

        public void Update()
        {
            // Keep smoothing for the specified duration:
            if ((DialogueTime.time < endTime) && (faderImage != null))
            {
                float elapsed = (DialogueTime.time - startTime) / duration;
                float alpha = fadeIn ? (1 - elapsed) : elapsed;
                faderImage.color = new Color(color.r, color.g, color.b, alpha);
            }
            else
            {
                Stop();
            }
        }

        public void OnDestroy()
        {
            if (faderCanvas != null)
            {
                faderCanvas.gameObject.SetActive(stay);
            }
            if (faderImage != null)
            {
                faderImage.gameObject.SetActive(stay);
                faderImage.color = new Color(color.r, color.g, color.b, fadeIn ? 0 : 1);
            }
        }

    }

}
