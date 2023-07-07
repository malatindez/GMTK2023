using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Events;

public class PlayerDetector : MonoBehaviour
{
    
    public GameObject Player;
    public float PlayerVisibilityWidth;
    public float Distance;
    public float Range;
    public int RayCount = 3;

    /// <summary>
    /// Called once on player detection.
    /// </summary>
    public UnityEvent OnDetection;
    /// <summary>
    /// Called as long as the enemy sees the player.
    /// </summary>
    public UnityEvent OnSeeing;
    
    private int wallLayer;
    private bool isAlerted = false;
    private bool isSeeing = false;

    private Vector3 lastPositionPlayerSeen = new Vector3();

    public bool IsAlerted
    {
        get => isAlerted;
        set => isAlerted = value;
    }

    public bool IsSeeing => isSeeing;

    public Vector3 LastPositionPlayerSeen
    {
        set => lastPositionPlayerSeen = value;
    }

    void Start()
    {
        wallLayer = LayerMask.NameToLayer("Wall");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.matrix = this.transform.localToWorldMatrix;

        Vector3 rangeLine = new Vector3(0, 0, Mathf.Cos(Range * Mathf.Deg2Rad));

        rangeLine.x = Mathf.Sin(Range * Mathf.Deg2Rad);
        Gizmos.DrawLine(Vector3.zero, rangeLine * Distance);

        rangeLine.x = -Mathf.Sin(Range * Mathf.Deg2Rad);
        Gizmos.DrawLine(Vector3.zero, rangeLine * Distance);
     
        Gizmos.color = isAlerted ? Color.yellow : Color.gray;
        Gizmos.color = isSeeing ? Color.red : Gizmos.color;
        Gizmos.DrawCube(Vector3.up * 2, new Vector3(0.1f, 0.1f, 0.1f));
    }

    void FixedUpdate()
    {
        if (Player == null)
        {
            return;
        }

        Vector3 distance = Player.transform.position - this.transform.position;
        var perpendicular = Vector3.Cross(distance, Vector3.up).normalized;

        isSeeing = false;

        if (distance.magnitude > Distance)
        {
            return;
        }

        if (!IsPlayerInRange(perpendicular))
        {
            return;
        }
        
        if (isPlayerBehindWall(perpendicular))
        {
           return;
        }
        
        if (!isSeeing)
        {
            OnDetection?.Invoke();
        }

        OnSeeing?.Invoke();
            
        isAlerted = true;
        isSeeing = true;

        lastPositionPlayerSeen = Player.transform.position;

    }

    [SuppressMessage("ReSharper", "Unity.InefficientPropertyAccess")]
    private bool IsPlayerInRange(Vector3 perpendicular)
    {

        for (int ind = 0; ind < RayCount; ind++)
        {
            float rangeOffset = PlayerVisibilityWidth / (RayCount - 1) * ind - PlayerVisibilityWidth / 2f;
            Vector3 offset = perpendicular * rangeOffset;
            
            float angle = Vector3.Angle(
                transform.TransformVector(Vector3.forward),  
                Player.transform.position - transform.position + offset
            );

            if (angle <= Range)
            {
                return true;
            }
        }

        return false;

    }
    
    private bool isPlayerBehindWall(Vector3 perpendicular)
    {
        for (int ind = 0; ind < RayCount; ind++)
        {
            float rangeOffset = PlayerVisibilityWidth / (RayCount - 1) * ind - PlayerVisibilityWidth / 2f;
            Vector3 offset = perpendicular * rangeOffset;

            
            bool result = Physics.Linecast(transform.position, Player.transform.position + offset, ~wallLayer);
            Debug.DrawLine(transform.position, Player.transform.position + offset);
            
            if (result)
            {
                return true;
            }
        }

        return false;
    }
}
