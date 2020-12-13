using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Dialogue
{

    /// <summary>
    /// Окно, отображающее персонажей диалога
    /// </summary>
    public class CDialogueActorsPanel : MonoBehaviour
    {

        public void Initialize( XmlDocument iniDoc = null )
        {
            if( iniDoc != null ) {
                // Пытаемся прочесть из инишника параметры
                XmlNode actorsImagesPathNode = iniDoc.DocumentElement.SelectSingleNode( ActorsImagesPathKey );
                if( _actorsImagesPath.Length == 0 && actorsImagesPathNode != null ) {
                    _actorsImagesPath = actorsImagesPathNode.InnerText;
                }

                XmlNode actorDefaultScaleNode = iniDoc.DocumentElement.SelectSingleNode( ActorDefaultScaleKey );
                if( _actorDefaultScale < 0f && actorDefaultScaleNode != null ) {
                    Assert.IsTrue( actorsImagesPathNode.InnerText.ParseToFloat( out _actorDefaultScale ) );
                }
            }
            _allActors = new List<CActorInfo>();
            _actorNameToIndex = new Dictionary<string, int>();
            _newActors = new HashSet<string>();
        }


        /// <summary>
        /// Вызывается при начале диалога
        /// </summary>
        public void OnStartDialogue()
        {
            _allActors.Clear();
            _actorNameToIndex.Clear();
            _leadActorIndex = -1;
            for( int i = transform.childCount; i >= 0; i-- ) {
                DestroyImmediate( transform.GetChild( i ).gameObject );
            }
        }

        /// <summary>
        /// Отобразить действующих лиц диалога
        /// </summary>
        /// <param name="arrangements">Какие изменения нужно произвести в расположении лиц</param>
        /// <param name="actorImages">Какие изображения должны быть у лиц диалога</param>
        /// <param name="leadingActor">Имя говорящего лица</param>
        public void ShowActors( CDialogueActorsArrangements arrangements, CDialogueActorImage[] actorImages, string leadingActor )
        {
            _newActors.Clear();
            // Сначала определим изменения в расположении лиц
            bool mustChangeArrangement = false;

            int prevMaxActorsOnScreen = _maxActorsOnScreen;
            _maxActorsOnScreen = arrangements.UseDefaultMaxActorsOnScreen ? DialogueStatementDefaultValues.MaxActorsOnScreen : arrangements.MaxActorsOnScreen;
            if( prevMaxActorsOnScreen != _maxActorsOnScreen ) {
                mustChangeArrangement = true;
            }

            // Рассмотрим все описания аранжировок, добавим и удалим лица при необходимости, определим, нужно ли менять расположение лиц диалога.
            foreach( var arrangement in arrangements.Arrangements ) {
                switch( arrangement.ArrangementAction ) {
                    case TDialogueActorArrangementAction.Appear:
                        AddActor( arrangement.ActorName );
                        mustChangeArrangement = true;
                        break;
                    case TDialogueActorArrangementAction.Leave:
                        DeleteActor( arrangement.ActorName );
                        mustChangeArrangement = true;
                        break;
                    case TDialogueActorArrangementAction.ChangePosition:
                        Assert.IsTrue( !_actorNameToIndex.ContainsKey( arrangement.ActorName ), "Arrangement with type 'changePosition' is applied to the present actor" );
                        CActorInfo actorInfo = _allActors[_actorNameToIndex[arrangement.ActorName]];
                        if( actorInfo.PositionType != arrangement.Position || 
                            (arrangement.Position == TDialogueActorArrangementPosition.Custom && arrangement.ExactPosistion != actorInfo.Position) ) 
                        {
                            mustChangeArrangement = true;
                        }
                        break;
                    default:
                        Assert.IsTrue( false );
                        break;
                }

                if( arrangement.ArrangementAction != TDialogueActorArrangementAction.Leave ) {
                    CActorInfo actorInfo = _allActors[_actorNameToIndex[arrangement.ActorName]];
                    actorInfo.PositionType = arrangement.Position;
                    if( arrangement.Position == TDialogueActorArrangementPosition.Custom ) {
                        actorInfo.Position = arrangement.ExactPosistion;
                    }
                }
            }

            // Учтём главное лицо (обеспечим, чтобы оно точно отобразилось)
            if( _actorNameToIndex.ContainsKey( leadingActor ) ) {
                MakeLeading( leadingActor );
            }

            // Изменим расположение всех актёров с учётом новой информации
            if( mustChangeArrangement ) {
                MakeArrangements();
            }
        }

        /// Добавление нового лица в диалог
        private void AddActor( string actorName )
        {
            Assert.IsTrue( !_actorNameToIndex.ContainsKey( actorName ), "Arrangement with type 'appear' is applied to the present actor" );
            CActorInfo actorInfo = new CActorInfo();
            actorInfo.ActorName = actorName;

            // Создаём объект с изображением диалогового лица
            var actorImage = new GameObject().AddComponent<Image>();
            actorImage.transform.SetParent( transform );
            actorInfo.ActorObject = actorImage.gameObject;

            _allActors.Add( actorInfo );
            _actorNameToIndex.Add( actorName, _allActors.Count );
            _newActors.Add( actorName );

            // Всегда пытаемся отобразить новое лицо, даже если оно ничего не сказало
            MakeLeading( actorName );
        }

        // Удаление действующего лица из диалога
        private void DeleteActor( string actorName )
        {
            Assert.IsTrue( _actorNameToIndex.ContainsKey( actorName ), "Arrangement with type 'leave' is applied to the absent actor" );
            int prevIndex = _allActors[_actorNameToIndex[actorName]].PrevActorIndex;
            int nextIndex = _allActors[_actorNameToIndex[actorName]].NextActorIndex;
            if( prevIndex != -1 ) {
                _allActors[prevIndex].NextActorIndex = nextIndex;
            }
            if( nextIndex != -1 ) {
                _allActors[nextIndex].PrevActorIndex = prevIndex;
            }
            _actorNameToIndex.Remove( actorName );
        }

        private void MakeLeading( string actorName )
        {
            Assert.IsTrue( _actorNameToIndex.ContainsKey( actorName ), "Trying to make leading absent actor" );
            int index = _actorNameToIndex[actorName];
            if( _leadActorIndex != -1 ) {
                _allActors[_leadActorIndex].PrevActorIndex = index;
            }
            _allActors[index].NextActorIndex = _leadActorIndex;
            _allActors[index].PrevActorIndex = -1;
            _leadActorIndex = index;
        }

        // Расположить изображения по назначенным позициям
        private void MakeArrangements()
        {
            // Определим, какие лица будут присутствовать на сцене
            HashSet<int> actorsIndexes = new HashSet<int>();
            int currentIndex = _leadActorIndex;
            while( currentIndex != -1 && actorsIndexes.Count. )
        }

        // Вспомогательная структура, хранящая информацию о лице
        private class CActorInfo
        {
            public string ActorName;
            public GameObject ActorObject;// Игровой объект, соответствующий лицу
            public TDialogueActorArrangementPosition PositionType;// Какой тип расположения у лица (в какой части экрана расположить)
            public Vector2 Position;// Где расположить
            // Индексы списка _allActors, которые показывают, между какими лицами находилось бы данное лицо, если бы их расположили в порядке давности самой последней реплики (начиная от самой недавней).
            // Нужно для отображения самых последних говорящих при большом кол-ве лиц диалога.
            public int PrevActorIndex;// Если -1, значит, данное лицо говорило последним
            public int NextActorIndex;// Если -1, значит, данное лицо мы не слышали дольше всех
        }

        private static string ActorsImagesPathKey = "actorsImagesFolder";
        private static string ActorDefaultScaleKey = "actorScale";

        [SerializeField]private string _actorsImagesPath;// Путь к папке с изображениями лиц диалога
        [SerializeField]private float _actorDefaultScale = -1f;// Насколько нужно изменить размер оригинальной картинки персонажа
        [SerializeField] private Vector2 _boundaries;// Ширина и высота ограничивающего прямоугольника
        [SerializeField] private float _transitionSpeed;// Скорость переходов (перемещений спрайтов)

        private int _maxActorsOnScreen;// Максимальное кол-во отображаемых лиц диалога
        private List<CActorInfo> _allActors;// Информация обо всех лицах диалога, что участвовали в нём 
        private Dictionary<string, int> _actorNameToIndex;// Отображение из имении диалога к его индексу в _allActors. Также указывает, какие лица в данный момент присутствуют в диалоге
        private HashSet<string> _newActors;// Вспомогательный список, содержащий новые лица диалога
        private int _leadActorIndex;// Индекс последнего говорившего

    }
}