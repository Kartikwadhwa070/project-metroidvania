using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camerafollow : MonoBehaviour
{
    [SerializeField] private float followSpeed = 0.1f;
    [SerializeField] private Transform player; // Assign this in the Inspector.

    private void Update()
    {
        if (player != null)
        {
            Vector3 targetPosition = player.position;
            targetPosition.z = transform.position.z; // Preserve camera's Z position.
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed);
        }
        else
        {
            Debug.LogWarning("Player is not assigned in CameraFollow!");
        }
    }
}
