using UnityEngine;

// The models ship without animations: this lowers the arms (instead of the T-pose)
// and swings legs and arms while walking. With a gun, the right arm aims forward.
// No need to touch it.
[RequireComponent(typeof(Animator))]
public class ProceduralWalk : MonoBehaviour
{
    [SerializeField] Transform body;              // the object that moves and turns (Player)
    [SerializeField] ClickToMove movement;        // where the speed comes from
    [SerializeField] float stepsPerSecond = 1.7f;
    [SerializeField] float legSwing = 32f;
    [SerializeField] float armSwing = 22f;

    Transform upperLegL, upperLegR, lowerLegL, lowerLegR, upperArmL, upperArmR;
    Quaternion restUpperLegL, restUpperLegR, restLowerLegL, restLowerLegR, restArmL, restArmR;
    Vector3 restLocalPosition;
    float phase, blend;

    public bool HoldsGun { get; set; }      // set by Loadout

    void Awake()
    {
        Cache();
        if (upperLegL == null) { enabled = false; return; }      // the model is not Humanoid
        ApplyRestPose();
        restUpperLegL = upperLegL.localRotation; restUpperLegR = upperLegR.localRotation;
        restLowerLegL = lowerLegL.localRotation; restLowerLegR = lowerLegR.localRotation;
        restArmL = upperArmL.localRotation; restArmR = upperArmR.localRotation;
        restLocalPosition = transform.localPosition;
    }

    void LateUpdate()
    {
        float target = movement != null ? movement.Speed01 : 0f;
        blend = Mathf.MoveTowards(blend, target, Time.deltaTime * 6f);
        phase += Time.deltaTime * stepsPerSecond * Mathf.PI * 2f * Mathf.Max(blend, 0.0001f);
        float s = Mathf.Sin(phase) * blend;
        Vector3 axis = body != null ? body.right : transform.right;

        Swing(upperLegL, restUpperLegL, -legSwing * s, axis);
        Swing(upperLegR, restUpperLegR, legSwing * s, axis);
        Swing(lowerLegL, restLowerLegL, legSwing * 1.2f * Mathf.Max(0f, s), axis);    // knee bends back
        Swing(lowerLegR, restLowerLegR, legSwing * 1.2f * Mathf.Max(0f, -s), axis);
        Swing(upperArmL, restArmL, armSwing * s, axis);
        if (HoldsGun) AimRightArm();
        else Swing(upperArmR, restArmR, -armSwing * s, axis);

        transform.localPosition = restLocalPosition + Vector3.up * (Mathf.Abs(s) * 0.05f);
    }

    // Right arm forward and a little down, so the gun in that hand points ahead.
    void AimRightArm()
    {
        upperArmR.localRotation = restArmR;
        Vector3 forward = body != null ? body.forward : transform.forward;
        Vector3 current = (lowerArmR.position - upperArmR.position).normalized;
        Vector3 desired = (forward * 0.85f + Vector3.down * 0.5f).normalized;
        upperArmR.rotation = Quaternion.FromToRotation(current, desired) * upperArmR.rotation;
    }

    static void Swing(Transform bone, Quaternion rest, float degrees, Vector3 worldAxis)
    {
        bone.localRotation = rest;
        bone.rotation = Quaternion.AngleAxis(degrees, worldAxis) * bone.rotation;
    }

    // Arms down, slightly away from the body. Can also be called from the editor.
    public void ApplyRestPose()
    {
        Cache();
        if (upperArmL == null || lowerArmL == null) return;
        LowerArm(upperArmL, LowerArmOf(upperArmL));
        LowerArm(upperArmR, LowerArmOf(upperArmR));
    }

    void LowerArm(Transform upper, Transform lower)
    {
        Vector3 current = (lower.position - upper.position).normalized;
        Vector3 outward = Vector3.ProjectOnPlane(current, Vector3.up).normalized;
        Vector3 desired = (Vector3.down + outward * 0.18f).normalized;
        upper.rotation = Quaternion.FromToRotation(current, desired) * upper.rotation;
    }

    Transform lowerArmL, lowerArmR;
    Transform LowerArmOf(Transform upper) => upper == upperArmL ? lowerArmL : lowerArmR;

    void Cache()
    {
        var animator = GetComponent<Animator>();
        upperLegL = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        upperLegR = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        lowerLegL = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        lowerLegR = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
        upperArmL = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        upperArmR = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        lowerArmL = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        lowerArmR = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
    }
}
