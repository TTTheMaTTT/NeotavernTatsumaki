// Recompile at 7/14/2021 11:23:44 AM
// Copyright (c) Pixel Crushers. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;
using UnityEngine.UI;
using TMPro;

namespace PixelCrushers.DialogueSystem
{

#if TMP_PRESENT

    /// <summary>
    /// This is manager of text effects, using Text Mesh Pro
    /// Types text, animates it using information from text tags.
    /// Note: Handles RPGMaker codes, but not two codes next to each other.
    /// Note: Uses code of TextMeshProTypewriterEffect.
    /// </summary>
    [AddComponentMenu( "" )] // Use wrapper.
    [DisallowMultipleComponent]
    public class TextEffectsManager : AbstractTypewriterEffect
    {

        [System.Serializable]
        public class AutoScrollSettings
        {
            [Tooltip( "Automatically scroll to bottom of scroll rect. Useful for long text. Works best with left justification." )]
            public bool autoScrollEnabled = false;
            public UnityEngine.UI.ScrollRect scrollRect = null;
            [Tooltip( "Optional. Add a UIScrollBarEnabler to main dialogue panel, assign UI elements, then assign it here to automatically enable scrollbar if content is taller than viewport." )]
            public UIScrollbarEnabler scrollbarEnabler = null;
        }

        /// <summary>
        /// Optional auto-scroll settings.
        /// </summary>
        public AutoScrollSettings autoScrollSettings = new AutoScrollSettings();

        public UnityEvent onBegin = new UnityEvent();
        public UnityEvent onCharacter = new UnityEvent();
        public UnityEvent onEnd = new UnityEvent();

        /// <summary>
        /// Indicates whether the effect is playing.
        /// </summary>
        /// <value><c>true</c> if this instance is playing; otherwise, <c>false</c>.</value>
        public override bool isPlaying { get { return typewriterCoroutine != null; } }

        /// @cond FOR_V1_COMPATIBILITY
        public bool IsPlaying { get { return isPlaying; } }
        /// @endcond

        protected const string RPGMakerCodeQuarterPause = @"\,";
        protected const string RPGMakerCodeFullPause = @"\.";
        protected const string RPGMakerCodeSkipToEnd = @"\^";
        protected const string RPGMakerCodeInstantOpen = @"\>";
        protected const string RPGMakerCodeInstantClose = @"\<";

        protected enum RPGMakerTokenType
        {
            None,
            QuarterPause,
            FullPause,
            SkipToEnd,
            InstantOpen,
            InstantClose
        }

        protected Dictionary<int, List<RPGMakerTokenType>> rpgMakerTokens = new Dictionary<int, List<RPGMakerTokenType>>();

        protected TMPro.TMP_Text m_textComponent = null;
        protected TMPro.TMP_Text textComponent
        {
            get {
                if( m_textComponent == null ) m_textComponent = GetComponent<TMPro.TMP_Text>();
                return m_textComponent;
            }
        }

        protected TMPro.TextMeshProUGUI m_textGUI = null;
        protected TMPro.TextMeshProUGUI textGUI
        {
            get {
                if( m_textGUI == null ) m_textGUI = GetComponent<TMPro.TextMeshProUGUI>();
                return m_textGUI;
            }
        }

        protected LayoutElement m_layoutElement = null;
        protected LayoutElement layoutElement
        {
            get {
                if( m_layoutElement == null ) {
                    m_layoutElement = GetComponent<LayoutElement>();
                    if( m_layoutElement == null ) m_layoutElement = gameObject.AddComponent<LayoutElement>();
                }
                return m_layoutElement;
            }
        }

        protected AudioSource runtimeAudioSource
        {
            get {
                if( audioSource == null ) audioSource = GetComponent<AudioSource>();
                if( audioSource == null && (audioClip != null) ) {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                    audioSource.panStereo = 0;
                }
                return audioSource;
            }
        }

        protected TextEffectsParser m_textEffectsParser;
        protected TextEffectsParser textEffectsParser{
            get {
                if( m_textEffectsParser == null ) {
                    m_textEffectsParser = new TextEffectsParser();
                }
                return m_textEffectsParser;
            }
        }

        protected TextEffectsFactory m_textEffectsFactory;
        protected TextEffectsFactory textEffectsFactory
        {
            get {
                if( m_textEffectsFactory == null ) {
                    m_textEffectsFactory = new TextEffectsFactory();
                }
                return m_textEffectsFactory;
            }
        }

        protected Vector3[] originVertices;
        protected Color[] colorsWithoutVisibilityMask = new Color[0];
        protected Color[] typedColors = new Color[0];
        protected int maxVisibleCharacters;

        protected List<ITextEffect> effects = new List<ITextEffect>();

        protected string unprocessedText;

        protected bool started = false;
        protected int charactersTyped = 0;
        protected Coroutine typewriterCoroutine = null;
        protected MonoBehaviour coroutineController = null;

        public override void Awake()
        {

            if (removeDuplicateTypewriterEffects) RemoveIfDuplicate();
        }

        protected void RemoveIfDuplicate()
        {
            var effects = GetComponents<TextEffectsManager>();
            if (effects.Length > 1)
            {
                var keep = effects[0];
                for (int i = 1; i < effects.Length; i++)
                {
                    if (effects[i].GetInstanceID() < keep.GetInstanceID())
                    {
                        keep = effects[i];
                    }
                }
                for (int i = 0; i < effects.Length; i++)
                {
                    if (effects[i] != keep)
                    {
                        Destroy(effects[i]);
                    }
                }
            }
        }

        public override void Start()
        {
            if (!IsPlaying && playOnEnable)
            {
                LaunchEffects( textComponent.text, 0 );
            }
            started = true;
        }

        public void Update()
        {
            if( !paused ) {
                bool changed = false;
                // text effects management
                if( effects != null && effects.Count > 0 ) {
                    Vector3[] vertices = textGUI.mesh.vertices;
                    TMP_TextInfo textInfo = textGUI.textInfo;
                    foreach( ITextEffect effect in effects ) {
                        effect.UpdateEffect( textInfo, colorsWithoutVisibilityMask, vertices, Array.AsReadOnly( originVertices ) );
                    }
                    textGUI.mesh.vertices = vertices;
                    changed = true;
                }
                if( IsPlaying ) {
                    changed = true;
                }
                if( changed ) {
                    ChangeCharactersColor( maxVisibleCharacters );
                    textGUI.canvasRenderer.SetMesh( textGUI.mesh );
                }
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            if (!IsPlaying && playOnEnable && started)
            {
                LaunchEffects( textComponent.text, 0 );
            }
        }

        public override void OnDisable()
        {
            base.OnEnable();
            Stop();
        }

        /// <summary>
        /// Pauses the typewriting effect.
        /// </summary>
        public void Pause()
        {
            paused = true;
        }

        /// <summary>
        /// Unpauses the effect. The text will resume at the point where it
        /// was paused; it won't try to catch up to make up for the pause.
        /// </summary>
        public void Unpause()
        {
            paused = false;
        }

        public void Rewind()
        {
            charactersTyped = 0;
        }

        /// <summary>
        /// Starts typing, optionally from a starting index. Characters before the 
        /// starting index will appear immediately.
        /// </summary>
        /// <param name="text">Text to type.</param>
        /// <param name="fromIndex">Index of text string, to start typing from.</param>
        public override void StartTyping(string text, int fromIndex = 0)
        {
            LaunchEffects( text, fromIndex );
        }

        public override void StopTyping()
        {
            Stop();
        }

        /// <summary>
        /// Play typewriter on text immediately.
        /// </summary>
        /// <param name="text"></param>
        public virtual void PlayText(string text, int fromIndex = 0)
        {
            LaunchEffects( text, fromIndex );
        }

        protected void LaunchEffects( string text, int fromIndex = 0 )
        {
            unprocessedText = text;

            StopTypewriterCoroutine();
            textComponent.text = text;
            ProcessRPGMakerCodes();
            ProcessTags();
            textComponent.maxVisibleCharacters = text.Length;
            textComponent.ForceMeshUpdate();
            ManageTextEffects();
            StartTypewriterCoroutine( fromIndex ); 
        }

        protected virtual void ManageTextEffects()
        {
            textEffectsParser.Parse( StripRPGMakerCodes( unprocessedText ), out var textWithoutEffects, out var textWithoutTags, out var effectsParamsList );
            Assert.IsTrue( textWithoutEffects.Length == textComponent.text.Length );

            textGUI.ForceMeshUpdate();
            originVertices = textGUI.mesh.vertices;
            colorsWithoutVisibilityMask = textGUI.mesh.colors;
            typedColors = textGUI.mesh.colors;
            Assert.IsTrue( textWithoutTags.Length == textGUI.textInfo.characterCount );

            // effects initialization
            effects = new List<ITextEffect>();
            foreach( TextEffectParams effectParams in effectsParamsList ) {
                effects.Add( textEffectsFactory.CreateEffect( effectParams ) );
                effects[effects.Count - 1].Initialize( effectParams );
            }

            Vector3[] vertices = textGUI.mesh.vertices;

            TMP_TextInfo textInfo = textGUI.textInfo;
            // text effects start
            foreach( ITextEffect effect in effects ) {
                effect.StartEffect( textInfo, colorsWithoutVisibilityMask, vertices, Array.AsReadOnly( originVertices ) );
            }
            ChangeCharactersColor( maxVisibleCharacters );
            textGUI.mesh.vertices = vertices;
            textGUI.canvasRenderer.SetMesh( textGUI.mesh );
        }

        protected readonly Color visibilityMask = new Color( 1f, 1f, 1f, 0f );

        protected void ChangeCharactersColor( int visibleCharactersCount )
        {
            Assert.IsTrue( colorsWithoutVisibilityMask.Length == typedColors.Length );
            TMP_TextInfo textInfo = textGUI.textInfo;
            for( int c = 0; c < textInfo.characterCount; c++ ) {
                var charInfo = textInfo.characterInfo[c];
                if( !charInfo.isVisible ) {
                    continue;
                }
                int vertexIndex = charInfo.vertexIndex;
                if( rightToLeft ? c >= textInfo.characterCount - visibleCharactersCount : c < visibleCharactersCount ) {
                    for( int j = 0; j < 4; j++ ) {
                        typedColors[vertexIndex + j] = colorsWithoutVisibilityMask[vertexIndex + j];
                    }
                } else {
                    for( int j = 0; j < 4; j++ ) {
                        typedColors[vertexIndex + j] = colorsWithoutVisibilityMask[vertexIndex + j] * visibilityMask;
                    }
                }
            }
            textGUI.mesh.colors = typedColors;
        }

        protected virtual void StartTypewriterCoroutine(int fromIndex)
        {
            if (coroutineController == null || !coroutineController.gameObject.activeInHierarchy)
            {
                // This MonoBehaviour might not be enabled yet, so use one that's guaranteed to be enabled:
                MonoBehaviour controller = GetComponentInParent<AbstractDialogueUI>();
                if (controller == null) controller = DialogueManager.instance;
                coroutineController = controller;
                if (coroutineController == null) coroutineController = this;
            }
            typewriterCoroutine = coroutineController.StartCoroutine( Play( fromIndex));
        }

        public virtual IEnumerator OtherPlay( int fromIndex )
        {
            int count = 0;
            while( true ) {
                maxVisibleCharacters = count;
                count++;
                yield return DialogueTime.WaitForSeconds( quarterPauseDuration );
            }
        }

        /// <summary>
        /// Plays the typewriter effect.
        /// </summary>
        public virtual IEnumerator Play(int fromIndex)
        {
            if ( unprocessedText != null && ( textComponent != null) && (charactersPerSecond > 0))
            {
                if (waitOneFrameBeforeStarting) yield return null;
                fromIndex = StripRPGMakerCodes( Tools.StripTags( unprocessedText.Substring( 0, fromIndex ) ) ).Length;
                if (runtimeAudioSource != null) runtimeAudioSource.clip = audioClip;
                onBegin.Invoke();
                paused = false;
                float delay = 1 / charactersPerSecond;
                float lastTime = DialogueTime.time;
                float elapsed = fromIndex / charactersPerSecond;
                maxVisibleCharacters = fromIndex;
                yield return null;
                maxVisibleCharacters = fromIndex;
                TMPro.TMP_TextInfo textInfo = textComponent.textInfo;
                var parsedText = textComponent.GetParsedText();
                int totalVisibleCharacters = textInfo.characterCount; // Get # of Visible Character in text object
                charactersTyped = fromIndex;
                int skippedCharacters = 0;
                while (charactersTyped < totalVisibleCharacters)
                {
                    if (!paused)
                    {
                        var deltaTime = DialogueTime.time - lastTime;
                        elapsed += deltaTime;
                        var goal = (elapsed * charactersPerSecond) + skippedCharacters;
                        while (charactersTyped < goal)
                        {
                            if (rpgMakerTokens.ContainsKey(charactersTyped))
                            {
                                var tokens = rpgMakerTokens[charactersTyped];
                                for (int i = 0; i < tokens.Count; i++)
                                {
                                    var token = tokens[i];
                                    switch (token)
                                    {
                                        case RPGMakerTokenType.QuarterPause:
                                            yield return DialogueTime.WaitForSeconds(quarterPauseDuration);
                                            break;
                                        case RPGMakerTokenType.FullPause:
                                            yield return DialogueTime.WaitForSeconds(fullPauseDuration);
                                            break;
                                        case RPGMakerTokenType.SkipToEnd:
                                            charactersTyped = totalVisibleCharacters - 1;
                                            break;
                                        case RPGMakerTokenType.InstantOpen:
                                            var close = false;
                                            while (!close && charactersTyped < totalVisibleCharacters)
                                            {
                                                charactersTyped++;
                                                skippedCharacters++;
                                                if (rpgMakerTokens.ContainsKey(charactersTyped) && rpgMakerTokens[charactersTyped].Contains(RPGMakerTokenType.InstantClose))
                                                {
                                                    close = true;
                                                }
                                            }
                                            break;
                                    }
                                }
                            }
                            var typedCharacter = (0 <= charactersTyped && charactersTyped < parsedText.Length) ? parsedText[charactersTyped] : ' ';
                            if (charactersTyped < totalVisibleCharacters && !IsSilentCharacter(typedCharacter)) PlayCharacterAudio(typedCharacter);
                            onCharacter.Invoke();
                            charactersTyped++;
                            maxVisibleCharacters = charactersTyped;
                            if (IsFullPauseCharacter(typedCharacter)) yield return DialogueTime.WaitForSeconds(fullPauseDuration);
                            else if (IsQuarterPauseCharacter(typedCharacter)) yield return DialogueTime.WaitForSeconds(quarterPauseDuration);
                        }
                    }
                    maxVisibleCharacters = charactersTyped;
                    HandleAutoScroll();
                    //---Uncomment the line below to debug: 
                    //Debug.Log(textComponent.text.Substring(0, charactersTyped).Replace("<", "[").Replace(">", "]") + " (typed=" + charactersTyped + ")");
                    lastTime = DialogueTime.time;
                    var delayTime = DialogueTime.time + delay;
                    int delaySafeguard = 0;
                    while (DialogueTime.time < delayTime && delaySafeguard < 999)
                    {
                        delaySafeguard++;
                        yield return null;
                    }
                }
            }
            Stop();
        }

        protected void ProcessRPGMakerCodes()
        {
            rpgMakerTokens.Clear();
            var source = textComponent.text;
            var result = string.Empty;
            if (!source.Contains("\\")) return;
            source = Tools.StripTags(source);
            int safeguard = 0;
            while (!string.IsNullOrEmpty(source) && safeguard < 9999)
            {
                safeguard++;
                RPGMakerTokenType token;
                if (PeelRPGMakerTokenFromFront(ref source, out token))
                {
                    int i = result.Length;
                    if (!rpgMakerTokens.ContainsKey(i))
                    {
                        rpgMakerTokens.Add(i, new List<RPGMakerTokenType>());
                    }
                    rpgMakerTokens[i].Add(token);
                }
                else
                {
                    result += source[0];
                    source = source.Remove(0, 1);
                }
            }
            textComponent.text = Regex.Replace(textComponent.text, @"\\[\.\,\^\<\>]", string.Empty);
        }

        // Removes all non-text mesh pro tags.
        protected void ProcessTags()
        {
            string source = textComponent.text;
            string result = "";
            int index = 0;
            var tagMatches = Tools.GetTagMatches( source );
            foreach( Match match in tagMatches ) {
                if( Tools.IsTextMeshProTag( match.Value ) || Tools.IsRichTextCode( match.Value )) {
                    result += source.Substring( index, match.Index + match.Length - index );
                } else {
                    result += source.Substring( index, match.Index - index );
                }
                index = match.Index + match.Length;
            }
            result += source.Substring( index, source.Length - index );
            textComponent.text = Tools.UnescapeTagBrackets( result );
        }

        protected bool PeelRPGMakerTokenFromFront(ref string source, out RPGMakerTokenType token)
        {
            token = RPGMakerTokenType.None;
            if (string.IsNullOrEmpty(source) || source.Length < 2 || source[0] != '\\') return false;
            var s = source.Substring(0, 2);
            if (string.Equals(s, RPGMakerCodeQuarterPause))
            {
                token = RPGMakerTokenType.QuarterPause;
            }
            else if (string.Equals(s, RPGMakerCodeFullPause))
            {
                token = RPGMakerTokenType.FullPause;
            }
            else if (string.Equals(s, RPGMakerCodeSkipToEnd))
            {
                token = RPGMakerTokenType.SkipToEnd;
            }
            else if (string.Equals(s, RPGMakerCodeInstantOpen))
            {
                token = RPGMakerTokenType.InstantOpen;
            }
            else if (string.Equals(s, RPGMakerCodeInstantClose))
            {
                token = RPGMakerTokenType.InstantClose;
            }
            else
            {
                return false;
            }
            source = source.Remove(0, 2);
            return true;
        }

        protected void StopTypewriterCoroutine()
        {
            if (typewriterCoroutine == null) return;
            if (coroutineController == null)
            {
                StopCoroutine(typewriterCoroutine);
            }
            else
            {
                coroutineController.StopCoroutine(typewriterCoroutine);
            }
            typewriterCoroutine = null;
            coroutineController = null;
        }

        /// <summary>
        /// Stops the typewriting effect.
        /// </summary>
        public override void Stop()
        {
            if (isPlaying)
            {
                onEnd.Invoke();
                Sequencer.Message(SequencerMessages.Typed);
            }
            StopTypewriterCoroutine();
            if( textComponent != null ) {
                maxVisibleCharacters = textComponent.textInfo.characterCount;
                ChangeCharactersColor( maxVisibleCharacters );
                textGUI?.canvasRenderer.SetMesh( textGUI.mesh );
            } 
            HandleAutoScroll();
        }

        protected void HandleAutoScroll()
        {
            if (!autoScrollSettings.autoScrollEnabled) return;

            var layoutElement = textComponent.GetComponent<LayoutElement>();
            if (layoutElement == null) layoutElement = textComponent.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = textComponent.textBounds.size.y;
            if (autoScrollSettings.scrollRect != null)
            {
                autoScrollSettings.scrollRect.normalizedPosition = new Vector2(0, 0);
            }
            if (autoScrollSettings.scrollbarEnabler != null)
            {
                autoScrollSettings.scrollbarEnabler.CheckScrollbar();
            }
        }

    }

#else

    [AddComponentMenu("")] // Use wrapper.
    public class TextEffectsManager : AbstractTypewriterEffect
    {
        public override bool isPlaying { get { return false; } }
        public override void Awake() { }
        public override void Start() { }
        public override void StartTyping(string text, int fromIndex = 0) { }
        public override void Stop() { }
        public override void StopTyping() { }
    }

#endif
}
