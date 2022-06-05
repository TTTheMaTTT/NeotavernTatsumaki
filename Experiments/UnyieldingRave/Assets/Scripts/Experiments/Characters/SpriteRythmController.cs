using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ������������ ����������� �������� ��� ������
public class SpriteRythmController : MonoBehaviour
{
    [SerializeField]
    private float bpm = 120;// ������� �����.

    private string beatSpeedParameterName = "BeatSpeed";

    private Animator animator;


    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        if( animator != null ) {
            animator.SetFloat( beatSpeedParameterName, bpm / 60f );
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
