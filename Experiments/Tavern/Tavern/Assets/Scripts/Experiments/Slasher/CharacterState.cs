namespace Experiments.Slasher
{
    public enum CharacterState
    {
        None,// Дефолтное состояние (перемещение)
        Attacking,// Атака
        PoiseBreak,// Сбитие баланса, при получении урона
        Dead// Смерть, конечное состояние всех вещей
    }
}