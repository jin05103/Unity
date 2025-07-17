using System.Collections;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class CameraFollow : NetworkBehaviour
{
    CinemachineCamera cinemachineCamera;
    public bool isFollowing = false;

    public bool isDead = false;
    private int spectateIndex = -1;

    public override void OnNetworkSpawn()
    {
        if (IsLocalPlayer)
        {
            StartCoroutine(followPlayer());
        }
    }

    private void Update()
    {
        if (!IsLocalPlayer) return;

        if (isDead && Input.GetKeyDown(KeyCode.Space))
        {
            int count = PlayerController.AlivePlayers.Count;
            if (count == 0) return;

            spectateIndex++;
            if (spectateIndex >= count)
                spectateIndex = 0;

            int checkedCount = 0;
            while (checkedCount < count)
            {
                var player = PlayerController.AlivePlayers[spectateIndex];
                if (player != null && player != this.GetComponent<PlayerController>())
                {
                    // 카메라 타겟 변경
                    if (cinemachineCamera == null)
                        cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();

                    cinemachineCamera.Follow = player.transform;
                    Debug.Log($"관전 대상 변경: {player.name}");
                    break;
                }
                // 다음 인덱스로 순환
                spectateIndex = (spectateIndex + 1) % count;
                checkedCount++;
            }
        }
    }

    IEnumerator followPlayer()
    {
        while (!isFollowing)
        {
            if (IsOwner)
            {
                var vcam = FindFirstObjectByType<CinemachineCamera>();
                if (vcam != null)
                {
                    vcam.Follow = transform;
                    cinemachineCamera = vcam;
                    isFollowing = true;
                    break;
                }
            }
            yield return new WaitForSeconds(0.2f);
        }
    }
}
