using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Experiments.SamuraiWay
{
    public class CharacterController : MonoBehaviour
    {

        [SerializeField] private float speed;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            float horizontalAxis = Input.GetAxis( "Horizontal" );
            if( horizontalAxis > 0f ) {
                transform.position += new Vector3( speed * Time.deltaTime, 0f );
            } else if( horizontalAxis < 0f ) {
                transform.position -= new Vector3( speed * Time.deltaTime, 0f );
            }
        }
    }

}