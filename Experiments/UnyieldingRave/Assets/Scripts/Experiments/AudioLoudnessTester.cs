using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioLoudnessTester : MonoBehaviour
{
    [SerializeField]
    private AudioSource audioSource;

    private float updateStep = 0.01f;
    private int sampleDataLength = 1024;
    private float currentUpdateTime = 0f;

    private float clipLoudness;
    private float[] clipSampleData;

    [SerializeField]
    private float sizeFactor = 1f;
    [SerializeField]
    private float minSize = 0f;
    [SerializeField]
    private float maxSize = 500f;

    // Start is called before the first frame update
    void Start()
    {
        clipSampleData = new float[sampleDataLength];
    }

    // Update is called once per frame
    void Update()
    {
        currentUpdateTime += Time.deltaTime;
        if( currentUpdateTime >= updateStep ) {
            currentUpdateTime = 0f;
            audioSource.clip.GetData( clipSampleData, 
                Mathf.Clamp( audioSource.timeSamples - sampleDataLength / 2, 0, audioSource.timeSamples ));
            clipLoudness = 0f;
            foreach( var sample in clipSampleData ) {
                clipLoudness += Mathf.Abs( sample );
            }
            clipLoudness /= sampleDataLength;

            clipLoudness = Mathf.Clamp( clipLoudness * sizeFactor, minSize, maxSize );
            gameObject.transform.localScale = new Vector3( clipLoudness, clipLoudness, clipLoudness );
        }
    }
}
