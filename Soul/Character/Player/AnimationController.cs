using System.Collections;
using UnityEngine;

public class AnimationController : AnimatorManager
{

    private const string ONE_HAND_DOWN = "OneHandDown";
    private const string ONE_HAND_UP = "OneHandUp";
    private const string TWO_HAND_DOWN = "TwoHandDown";
    private const string TWO_HAND_UP = "TwoHandUp";
    private const string SHIELD = "Shield";
    private const string TWOHANDSHEILD = "TwoHandShield";
    private const string BOW = "Bow";
    private const string Potion = "Potion";

    public int currentLayerIndex;

    IEnumerator _shieldBlend;

    private void Awake() {
        _animator = GetComponent<Animator>();
        _animator.applyRootMotion = false;
        _animator.SetBool("JumpEnd", true);
        currentLayerIndex = 0;
    }

    // private void Start()
    // {
    //     _animator = GetComponent<Animator>();
    //     _animator.applyRootMotion = false;
    //     _animator.SetBool("JumpEnd", true);
    // }

    public void SetFloat(string name, float value)
    {
        _animator.SetFloat(name, value);
    }

    public void SetBool(string name, bool value)
    {
        _animator.SetBool(name, value);
    }

    public void SetTrigger(string name)
    {
        _animator.SetTrigger(name);
    }

    public bool GetBool(string name)
    {
        return _animator.GetBool(name);
    }

    public void PlayTargetAnimation(string targetAnim, bool isInteracting)
    {
        _animator.applyRootMotion = isInteracting;
        _animator.SetBool("IsInteracting", isInteracting);

        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(currentLayerIndex);
        bool isHitAnim = targetAnim == "Hit_F_1";
        if (stateInfo.IsName(targetAnim) && isHitAnim)
        {
            // 이미 같은 애니메이션이 재생 중이면 0프레임부터 강제 재생
            _animator.Play(targetAnim, currentLayerIndex, 0);
        }
        else
        {
            _animator.CrossFade(targetAnim, 0.2f, currentLayerIndex);
        }
    }

    public void SetAnim()
    {
        _animator.applyRootMotion = true;
        _animator.SetBool("IsInteracting", true);
    }

    public bool GetInteractingState()
    {
        return _animator.GetBool("IsInteracting");
    }

    public Vector3 GetAnimatorDeltaPosition()
    {
        return _animator.deltaPosition;
    }

    public void ShieldAnim(bool isOn)
    {
        if (isOn)
        {
            if (_shieldBlend != null)
            {
                StopCoroutine(_shieldBlend);
            }
            _shieldBlend = ShieldBlend(true);
            StartCoroutine(_shieldBlend);
        }
        else
        {
            if (_shieldBlend != null)
            {
                StopCoroutine(_shieldBlend);
            }

            _shieldBlend = ShieldBlend(false);
            StartCoroutine(_shieldBlend);
        }
    }

    public void TwoHandShieldAnim(bool isOn)
    {
        if (isOn)
        {
            if (_shieldBlend != null)
            {
                StopCoroutine(_shieldBlend);
            }
            _shieldBlend = TwoHandShieldBlend(true);
            StartCoroutine(_shieldBlend);
        }
        else
        {
            if (_shieldBlend != null)
            {
                StopCoroutine(_shieldBlend);
            }

            _shieldBlend = TwoHandShieldBlend(false);
            StartCoroutine(_shieldBlend);
        }
    }

    IEnumerator ShieldBlend(bool isOn)
    {
        if (isOn)
        {
            while (_animator.GetLayerWeight(_animator.GetLayerIndex(SHIELD)) < 1)
            {
                _animator.SetLayerWeight(_animator.GetLayerIndex(SHIELD), _animator.GetLayerWeight(_animator.GetLayerIndex(SHIELD)) + Time.deltaTime * 5);

                if (_animator.GetLayerWeight(_animator.GetLayerIndex(SHIELD)) > 0.95f)
                {
                    _animator.SetLayerWeight(_animator.GetLayerIndex(SHIELD), 1);
                }
                yield return null;
            }
        }
        else
        {
            while (_animator.GetLayerWeight(_animator.GetLayerIndex(SHIELD)) > 0)
            {
                _animator.SetLayerWeight(_animator.GetLayerIndex(SHIELD), _animator.GetLayerWeight(_animator.GetLayerIndex(SHIELD)) - Time.deltaTime * 5);

                if (_animator.GetLayerWeight(_animator.GetLayerIndex(SHIELD)) < 0.05f)
                {
                    _animator.SetLayerWeight(_animator.GetLayerIndex(SHIELD), 0);
                }
                yield return null;
            }
        }
    }

    IEnumerator TwoHandShieldBlend(bool isOn)
    {
        if (isOn)
        {
            while (_animator.GetLayerWeight(_animator.GetLayerIndex(TWOHANDSHEILD)) < 1)
            {
                _animator.SetLayerWeight(_animator.GetLayerIndex(TWOHANDSHEILD), _animator.GetLayerWeight(_animator.GetLayerIndex(TWOHANDSHEILD)) + Time.deltaTime * 5);
                if (_animator.GetLayerWeight(_animator.GetLayerIndex(TWOHANDSHEILD)) > 0.95f)
                {
                    _animator.SetLayerWeight(_animator.GetLayerIndex(TWOHANDSHEILD), 1);
                }
                yield return null;
            }
        }
        else
        {
            while (_animator.GetLayerWeight(_animator.GetLayerIndex(TWOHANDSHEILD)) > 0)
            {
                _animator.SetLayerWeight(_animator.GetLayerIndex(TWOHANDSHEILD), _animator.GetLayerWeight(_animator.GetLayerIndex(TWOHANDSHEILD)) - Time.deltaTime * 5);
                if (_animator.GetLayerWeight(_animator.GetLayerIndex(TWOHANDSHEILD)) < 0.05f)
                {
                    _animator.SetLayerWeight(_animator.GetLayerIndex(TWOHANDSHEILD), 0);
                }
                yield return null;
            }
        }
    }

    public void ChangeWeaponLayer(WeaponType weaponType, bool isTwoHanded, bool isShield)
    {
        currentLayerIndex = 0;
        _animator.SetLayerWeight(_animator.GetLayerIndex(ONE_HAND_DOWN), 0);
        _animator.SetLayerWeight(_animator.GetLayerIndex(ONE_HAND_UP), 0);
        _animator.SetLayerWeight(_animator.GetLayerIndex(TWO_HAND_DOWN), 0);
        _animator.SetLayerWeight(_animator.GetLayerIndex(TWO_HAND_UP), 0);
        _animator.SetLayerWeight(_animator.GetLayerIndex(BOW), 0);
        // _animator.SetLayerWeight(_animator.GetLayerIndex(SHIELD), 0);
        if (isTwoHanded)
        {
            switch (weaponType)
            {
                case WeaponType.sword:
                    _animator.SetLayerWeight(_animator.GetLayerIndex(TWO_HAND_DOWN), 1);
                    currentLayerIndex = _animator.GetLayerIndex(TWO_HAND_DOWN);
                    break;
                case WeaponType.heavySword:
                    _animator.SetLayerWeight(_animator.GetLayerIndex(TWO_HAND_UP), 1);
                    currentLayerIndex = _animator.GetLayerIndex(TWO_HAND_UP);
                    break;
                case WeaponType.bow:
                    _animator.SetLayerWeight(_animator.GetLayerIndex(BOW), 1);
                    currentLayerIndex = _animator.GetLayerIndex(BOW);
                    break;
                case WeaponType.none:
                    break;
            }
        }
        else
        {
            // if (isShield)
            // {
            //     // _animator.SetLayerWeight(_animator.GetLayerIndex(SHIELD), 1);
            // }
            // else
            // {
            switch (weaponType)
            {
                case WeaponType.sword:
                    _animator.SetLayerWeight(_animator.GetLayerIndex(ONE_HAND_DOWN), 1);
                    currentLayerIndex = _animator.GetLayerIndex(ONE_HAND_DOWN);
                    break;
                case WeaponType.heavySword:
                    _animator.SetLayerWeight(_animator.GetLayerIndex(ONE_HAND_UP), 1);
                    currentLayerIndex = _animator.GetLayerIndex(ONE_HAND_UP);
                    break;
                case WeaponType.bow:
                    break;
                case WeaponType.none:
                    break;
            }
            // }
        }
    }

    public void PotionAnim(bool isOn)
    {
        if (isOn)
        {
            _animator.SetLayerWeight(_animator.GetLayerIndex(Potion), 1);
        }
        else
        {
            _animator.SetLayerWeight(_animator.GetLayerIndex(Potion), 0);
        }
    }
}
