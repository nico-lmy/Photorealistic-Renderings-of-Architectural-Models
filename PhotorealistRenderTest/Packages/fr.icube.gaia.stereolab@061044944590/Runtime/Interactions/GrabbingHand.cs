using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using Stereolab.Attributes;

namespace Stereolab.Interactions
{
    [RequireComponent(typeof(Collider))]
    /// <summary>
    /// Simple interaction tool that allows to grab stuff based on a Collider.
    /// </summary>
    public class GrabbingHand : MonoBehaviour
    {
        /*
        Behaves as follow:

        Default state: "open", no input nor dragging anything, collider deactivated
        1. Input down: Activate the collider and start catching stuff (magnetic state) and dragging it around
        2. End of the magnetic state: We don't catch other stuff but keep dragging around what we have
        3. Some stuff might be unable to follow us, so we leave them behind (reactive gravity and stuff) but keep dragging the rest
        4. Input up: Release all the objects dragged and disable the collider.
        */

        /// <summary>
        /// Possible states of grabbing. Only of esthetic use.
        /// </summary>
        private enum State {
            Released,
            Magnetic,
            Dragging
        }

        [SerializeField] 
        private InputActionReference grabInputAction; 

        [Header("General")]

        /// <summary>
        /// Time during which the instance will catch any collision to drag them at the beginning of the grab.
        /// </summary>
        [SerializeField]
        [Tooltip("Time during which the instance will catch any collision to drag them at the beginning of the grab.")]
        private float magneticDuration = 0.2f;

        /// <summary>
        /// Is the instance currently catching objects ?
        /// </summary>
        private bool magneticState = false;

        /// <summary>
        /// Multiplier to the inertia and torque intensity passed to any object grabbed and released in movement.
        /// </summary>
        [SerializeField]
        [Tooltip("Multiplier to the inertia and torque intensity passed to any object grabbed and released in movement.")]
        private float forceFactor = 1f;

        /// <summary>
        /// Stores the force of the hand as the mean value of its movement between the last and current frame, and the value of the variable in the last update iteration.
        /// </summary>
        /// <remarks>
        /// The goal is to retain so force when the hand stop (inertia-like behavior).
        /// </remarks>
        private Vector3 movementInertia = Vector3.zero;

        /// <summary>
        /// Stores the torque of the hand as the mean value of its rotaion between the last and current frame, and the value of the variable in the last update iteration.
        /// </summary>
        /// <remarks>
        /// The goal is to retain so force when the hand stop (inertia-like behavior).
        /// </remarks>
        private Vector3 rotationInertia = Vector3.zero;

        /// <summary>
        /// List of other colliders currently grabbed and dragged.
        /// </summary>
        private List<Collider> grabbedColliders = new List<Collider>();

        /// <summary>
        /// Position of the instance at the last call of FixedUpdate.
        /// </summary>
        private Vector3 lastFramePosition = Vector3.zero;

        /// <summary>
        /// Rotation of the instance at the last call of FixedUpdate.
        /// </summary>
        private Quaternion lastFrameRotation = Quaternion.identity;

        /// <summary>
        /// Collider component attached to the GO.
        /// </summary>
        private Collider grabbingCollider;


        [Header("Debug")]
        /// <summary>
        /// Enable the debug gizmos for the hand.
        /// </summary>
        [SerializeField]
        [Tooltip("Enable the debug gizmos for the hand.")]
        private bool drawGizmos = true;

        /// <summary>
        /// Color of the debug gizmos.
        /// </summary>
        [SerializeField]
        [Tooltip("Color of the debug gizmos.")]
        private Color gizmosColor = Color.red;

        [Header("Editor handles")]
        /// <summary>
        /// Visual indicator showing the state of the instance in the editor.
        /// </summary>
        [SerializeField]
        [ReadOnly]
        private State state = State.Released;

        /// <summary>
        /// Editor field behaving like a button, that once pressed starts a grab sequence.
        /// </summary>
        /// <remarks>
        /// Should be not used in any other way than the one specified in summary or in any actual interaction.
        /// </remarks>
        [SerializeField]
        [Tooltip("Editor field behaving like a button, that once pressed starts a grab sequence.")]
        private bool grabDebug = false;
        
        /// <summary>
        /// Editor field behaving like a button, that once pressed stops a grab sequence.
        /// </summary>
        /// <remarks>
        /// Should be not used in any other way than the one specified in summary or in any actual interaction.
        /// </remarks>
        [SerializeField]
        [Tooltip("Editor field behaving like a button, that once pressed stops a grab sequence.")]
        private bool releaseDebug = false;

        private void OnDrawGizmos()
        {
            if (drawGizmos)
            {
                // Visually link all grabbed object to the instance
                Gizmos.color = gizmosColor;
                foreach (Collider collider in grabbedColliders)
                {
                    Gizmos.DrawLine(transform.position, collider.transform.position);
                    Gizmos.DrawSphere(collider.transform.position, 0.01f);
                }
            }
        }

        private void Awake()
        {
            // Retrieve the Collider component and deactivate it 
            grabbingCollider = GetComponent<Collider>();
            grabbingCollider.enabled = false;
        }

        private void Update()
        {
            // Allow to grab and release objects from the serialized fields of the editor
            if (grabDebug)
            {
                Grab();
                grabDebug = false;
            }

            if (releaseDebug)
            {
                Release();
                releaseDebug = false;
            }
        }

        /// <summary>
        /// Do all the moving in the physics to be in sync with the TriggerEnter/Exit events
        /// </summary>
        private void FixedUpdate()
        {
            // Retrieve the position/rotation of the grabbing hand at the given frame
            Vector3 currentGrabPosition = transform.position;
            Quaternion currentGrabRotation = transform.rotation;

            // Get the translations of the grabbing hand for the given frame
            Vector3 frameGrabbingPosition = currentGrabPosition - lastFramePosition;
            lastFramePosition = currentGrabPosition;

            // Get the rotations of the grabbing hand for the given frame
            Quaternion frameGrabbingRotation = currentGrabRotation * Quaternion.Inverse(lastFrameRotation);
            lastFrameRotation = currentGrabRotation;

            // Move the colliders GOs to follow the hand that grabbed them
            foreach(Collider collider in grabbedColliders)
            {
                // Simple translation of the GO owning the collider
                collider.transform.Translate(frameGrabbingPosition, Space.World);

                // Rotation of the collider around the grab
                collider.transform.RotateAround(currentGrabPosition, Vector3.right, frameGrabbingRotation.eulerAngles.x);
                collider.transform.RotateAround(currentGrabPosition, Vector3.up, frameGrabbingRotation.eulerAngles.y);
                collider.transform.RotateAround(currentGrabPosition, Vector3.forward, frameGrabbingRotation.eulerAngles.z);
            }

            // Stores the movement/rotation for the frame as inertia
            movementInertia = (movementInertia + frameGrabbingPosition)/2f;
            rotationInertia = (rotationInertia + frameGrabbingRotation.eulerAngles)/2f;
        }

        private void OnEnable()
        {
            grabInputAction.action.performed += Grab;
            grabInputAction.action.canceled += Release;
        }

        private void OnDisable()
        {
            grabInputAction.action.performed -= Grab;
            grabInputAction.action.canceled -= Release;
        }

        private void SwitchGrabRelease(InputAction.CallbackContext _)
        {
            if (state == State.Released)
            {
                Grab();
            }
            else 
            {
                Release();
            }
        }

        private void Grab(InputAction.CallbackContext _) { Grab(); }
        private void Release(InputAction.CallbackContext _) { Release(); }

        /// <summary>
        /// Activate the collider, start grabbing and dragging objects
        /// </summary>
        public void Grab()
        {
            StartCoroutine(EnableMagneticState(magneticDuration));
        }

        /// <summary>
        /// Release the objects held and deactivate the collider.
        /// </summary>
        public void Release()
        {
            // Deactivate the collider component to prevent any OnTrigger event to happen again
            grabbingCollider.enabled = false;

            // Free the registered colliders, give them their physics back and apply the inertia
            foreach (Collider collider in grabbedColliders)
            {
                Rigidbody rig;
                if (collider.TryGetComponent<Rigidbody>(out rig))
                {
                    rig.useGravity = true;
                    rig.AddForce(movementInertia * forceFactor, ForceMode.Force);
                    rig.AddTorque(rotationInertia * forceFactor, ForceMode.Force);
                }
                collider.isTrigger = false;
            }
            grabbedColliders.Clear();

            // Update the informative editor field
            state = State.Released;
        }

        private void OnTriggerEnter(Collider collider)
        {
            // We don't want to keep grabbing everything we move into so we limit this behaviour to a "magnetic" state
            if (!magneticState)
            {
                return;
            }

            // Register the collider to the list of grabbed things
            grabbedColliders.Add(collider);
            Rigidbody collidedRigidBody;
            if (collider.TryGetComponent<Rigidbody>(out collidedRigidBody))
            {
                // Initialize the 'dragging around' variables
                collidedRigidBody.useGravity = false;
                collider.isTrigger = true;

                lastFramePosition = transform.position;
                lastFrameRotation = transform.rotation;
            }
        }

        /// <summary>
        /// Triggered by a dragged object leaving the collider mesh while the grab is being maintained.
        /// </summary>
        private void OnTriggerExit(Collider collider)
        {
            // Prevents an unknown collider to trigger the following
            if (!grabbedColliders.Contains(collider))
            {
                return;
            }

            // Give the physics back to the exiting collider
            Rigidbody rig;
            if (collider.TryGetComponent<Rigidbody>(out rig))
            {
                rig.useGravity = true;
            }
            collider.isTrigger = false;

            // Forget about it
            grabbedColliders.Remove(collider);
        }

        /// <summary>
        /// Enable the Collider component and its magnetic state for a given duration (in seconds)
        /// </summary>
        IEnumerator EnableMagneticState(float duration)
        {
            // Enable the Collider component and make it grab what it touches
            grabbingCollider.enabled = true;
            magneticState = true;

            // Update the editor field 
            state = State.Magnetic;

            // Wait for the specified duration
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.fixedDeltaTime;
                yield return null;
            } 

            // Leave the magnetic state. The grabbed colliders will still be moved around but no other will join the list
            magneticState = false;

            // Update the editor field
            // NB: The condition is here to avoid a wrong update if the grab is released before the end of the magnetic state
            if (state == State.Magnetic)
                state = State.Dragging;
        }
    }
}