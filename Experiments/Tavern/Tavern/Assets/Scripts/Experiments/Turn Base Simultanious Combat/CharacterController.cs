using UnityEngine;

namespace Experiments.TurnBaseSimultaniousCombat
{
    // ������� ����� ���� ����������, ������� ����������� GameController'��
    public class CharacterController : MonoBehaviour
    {

        public virtual void Analyze()
        {
        }

        public virtual void Action()
        {
        }

        protected virtual void Awake()
        {
            transform.position = GameControllerAbstract.Instance().GetPositionOnGrid( transform.position );
        }
    }
}