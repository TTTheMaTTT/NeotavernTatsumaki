// Объекты, которые можно перетаскивать
public interface IDraggable
{
    void StartDrag();
    void StopDrag();
    void StartRotate();
    void StopRotate();

    bool HaveMomentumOnDetach();
    bool ImitateInertia();
}
