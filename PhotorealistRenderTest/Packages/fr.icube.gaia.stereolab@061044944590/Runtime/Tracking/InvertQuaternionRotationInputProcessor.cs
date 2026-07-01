using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
/// <summary>
/// Pre-process the data of an InputAction to invert rotation eulerian-axes on a quaternion.
/// </summary>
public class InvertQuaternionRotationInputProcessor : InputProcessor<Quaternion>
{
    public bool invertX = true;
    public bool invertY = true;
    public bool invertZ = true;

    #if UNITY_EDITOR
    static InvertQuaternionRotationInputProcessor()
    {
        Initialize();
    }
    #endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        InputSystem.RegisterProcessor<InvertQuaternionRotationInputProcessor>();
    }

    /// <summary>
    /// Apply eulerian axes inversion without using the euler angles (avoid gimbal locks)
    /// </summary>
    public override Quaternion Process(Quaternion value, InputControl control)
    {
        Quaternion rotation = value;

        // Flip the axes of the direction vector of the quaternion
        if (invertX)
        {
            rotation.x *= -1;
        }
        if (invertY)
        {
            rotation.y *= -1;
        }
        if (invertZ)
        {
            rotation.z *= -1;
        }

        return rotation;
    }
}