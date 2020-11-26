using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPropDetection : IState<EnemyPropController>
{
    private float m_TimeSinceEnteredState = 0f;
    private float m_ForwardSpeed = 0f;
    private readonly float m_MaxForwardSpeed = 0.4f;
    private float m_WiggleAngle = 0f;
    private float m_DeltaAngle = 0f;
    private float m_LastStep = 0f;


    
    public void EnterState(EnemyPropController owner)
    {
        m_TimeSinceEnteredState = Time.time;
        m_ForwardSpeed = 0;
        m_WiggleAngle = 0f;
        m_LastStep = 0f;
        m_DeltaAngle = Mathf.Asin(Mathf.Sin(Time.deltaTime));
        owner.RigidBodyComponent.useGravity = false;
        owner.RigidBodyComponent.isKinematic = true;
        if (AudioManager.instance && owner.DetectionClip)
            AudioManager.instance.PlayClipAtPoint(owner.DetectionClip, owner.transform.position, TargetAudioMixer.PropGeneric);
        owner.m_Material.SetColor("_EmissiveColor", Color.white * owner.m_MaximumEmissionIntensity);
        //Spawn visual for detection
    }

    public void ExitState(EnemyPropController owner)
    {
        //Remove any visuals
    }

    public void UpdateState(EnemyPropController owner)
    {
        //WakeUp(owner);
        if(m_TimeSinceEnteredState + owner.DetectionAnimationTime > Time.time)
        {
            WakeUp(owner);
        }
        else 
        {
            owner.SetState(owner.FollowState);
        }
    }

    private void WakeUp(EnemyPropController owner)
    {
        CrawlTowardsTarget(owner);
        if (RiseTowardsHeight(owner) || RotateTowards(owner))
        {
            owner.SetState(owner.FollowState);
        }
    }

    private bool RiseTowardsHeight(EnemyPropController owner)
    {
        if(owner.AttackPattern == MimicAttackPattern.ChestJump)
        {
            return true;
        }

        if (Physics.Raycast(owner.transform.position, Vector3.down, out RaycastHit hitInfo)) {

            float targetHeight = owner.TargetObject.transform.position.y + owner.HoverHeight;
            targetHeight = Mathf.Clamp
                           (
                               targetHeight,
                               hitInfo.point.y,
                               owner.TargetObject.transform.position.y + owner.HoverHeight
                           );

            float step = (Time.time - m_TimeSinceEnteredState) / owner.DetectionAnimationTime;

            if (Mathf.Abs((targetHeight - owner.transform.position.y)) < Mathf.Abs(step - m_LastStep))
            {
                owner.transform.position = new Vector3(owner.transform.position.x,
                                                        targetHeight,
                                                        owner.transform.position.z);

                return true;
            }
            m_LastStep = step;

            owner.transform.position = new Vector3(owner.transform.position.x, 
                                                    Mathf.SmoothStep(owner.transform.position.y, targetHeight, step), 
                                                    owner.transform.position.z);

        }
        return false;
    }

    private void CrawlTowardsTarget(EnemyPropController owner)
    {
        owner.transform.position += ((owner.TargetObject.transform.position - owner.transform.position).normalized).Flattened() 
                                        * m_ForwardSpeed * Time.deltaTime;
        m_ForwardSpeed = Mathf.Clamp(m_ForwardSpeed + Time.deltaTime, 0f, m_MaxForwardSpeed);
    }

    private bool RotateTowards(EnemyPropController owner)
    {

        Quaternion targetRotation = Quaternion.LookRotation
                                    (
                                        owner.TargetObject.transform.position - owner.transform.position,
                                        Vector3.Cross
                                        (
                                            Vector3.Cross
                                            (
                                                owner.TargetObject.transform.position - owner.transform.position,
                                                Vector3.up
                                            ),
                                            owner.TargetObject.transform.position - owner.transform.position
                                        )
                                    );


        owner.transform.rotation = SmoothRotation(owner.DetectionAnimationTime, owner.transform.rotation, targetRotation);

        return owner.transform.rotation == targetRotation;
    }

    private void Swing(EnemyPropController owner)
    {
        owner.transform.Translate(owner.transform.right * Mathf.Sin(m_WiggleAngle - m_DeltaAngle));
        m_WiggleAngle += Time.deltaTime;
    }

    private Quaternion SmoothRotation(float duration, Quaternion currentRotation, Quaternion targetRotation)
    {
        float step = (Time.time - m_TimeSinceEnteredState) / duration;
        step = Mathf.Clamp01(step);
        step = -2.0f * step * step * step + 3.1f * step * step;

        float angleBetween = Quaternion.Angle(currentRotation, targetRotation);

        return Quaternion.RotateTowards(currentRotation, targetRotation, angleBetween * step);
    }
}
