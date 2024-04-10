using InfimaGames.Animated.ModernGuns;
using UnityEngine;

public class JankReloadDoneStateMachineBehaviour : StateMachineBehaviour
{
    private Weapon _weapon;

    public override void OnStateExit(Animator animator, AnimatorStateInfo _, int __)
    {
        if (_weapon == null)
            _weapon = animator.GetComponent<Weapon>();

        _weapon.OnReloadDone?.Invoke();
    }
}
