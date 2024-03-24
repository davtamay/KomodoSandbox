using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Komodo.Runtime;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System;
using Komodo.Utilities;
using UnityEngine.XR;
using Unity.XR.CoreUtils;

namespace Komodo.IMPRESS
{
    public class WorldPulling : SingletonComponent<WorldPulling>, IUpdatable
    {
        public static WorldPulling Instance
        {
            get { return (WorldPulling)_Instance; }

            set { _Instance = value; }
        }

        public ControlerAndHandTracker controlerHandTracker;
        [Serializable]
        public struct UpdatingValue<T>
        {
            public UpdatingValue(T initial)
            {
                Initial = initial;

                Current = initial;
            }

            public T Initial { get; }

            public T Current { get; set; }
        }

       [ShowOnly] public bool worldPullingActivatedLeft;
        [ShowOnly] public bool worldPullingActivatedRight;

        public UnityEvent onBothHandsReleased;

        public Transform xrTransform;
        public XROrigin xrOrigin;
             

        public void SetWorldPullingLeft(bool isActivated)
        {
            worldPullingActivatedLeft = isActivated;

            if (worldPullingActivatedLeft == false && worldPullingActivatedRight == false)
                onBothHandsReleased.Invoke();

        }
        public void SetWorldPullingRight(bool isActivated)
        {
            worldPullingActivatedRight = isActivated;

            if (worldPullingActivatedLeft == false && worldPullingActivatedRight == false)
                onBothHandsReleased.Invoke();
        }
        public bool GetWorldPullingLeft()
        {
            return worldPullingActivatedLeft;
        }
        public bool SetWorldPullingRight()
        {
            return worldPullingActivatedRight;
        }



        // Assign a Komodo Player or Impress Player object so we can access its hands
        public Transform player;

        // Assign a playspace, so we can scale, rotate, and translate the avatar,
        // while still allowing the player to move around
        public Transform playspace;

        // Assign the player rig's hands so we can read their transform values
       private Transform[] hands = new Transform[2];

        // Connect this action as a callback in Unity.
        public Action onDoubleTriggerPress;

        // Connect this action as a callback in Unity.
        public Action onDoubleTriggerRelease;

        // Set the min scale for the avatar
        public float scaleMin = 0.1f;

        // Set the max scale for the avatar
        public float scaleMax = 12.0f;

        // This is taken from Google Tilt Brush and has a texture that shows the current scale
        public MeshRenderer animalRulerMesh;

        // This is also taken from Google Tilt Brush
        public Transform animalRuler;

        // Assign a prefab here to help the user feel oriented towards the physical world while world pulling
        public GameObject physicalFloor;

        // If you need to manage what's visible or not during world-pulling, 
        // set up a layer manager and assign it here
        public LayerVisibility layerManager;

        // Assign a prefab in the inspector that will represent the position, scale, 
        // and rotation of world-pulling objects
        public GameObject debugAxes;

        // Assign materials for each debug axis that gets created. See below to understand
        // How many materials you need.
        public Material[] materials;

        // Turn this on to show the various debug axes that are created.
        public bool showDebugAxes;

        private Transform initialPivotPointInPlayspace;

        private Transform currentPivotPointInPlayspace;

        private Vector3 copyOfInitialPivotPointPosition;

        private GameObject copyOfInitialPivotPointPositionAxes;

        private GameObject initialPlayspace;

        // This is the distance between the hands divided by the scale of the playspace
        [SerializeField]
        private UpdatingValue<float> handDistanceInPlayspace;

        // When world-pulling, this is a line segment to hint that the user should move their hands
        private LineRenderer handToHandLine;

        private GameObject initialPlayspaceAxes;

        private GameObject currentPlayspaceAxes;

        public Action<float> onChangeScale;

        public TeleportPlayer teleportPlayer;
        public void Awake()
        {
            teleportPlayer = player.GetComponent<TeleportPlayer>();
          //  var initManager = Instance;

            initialPivotPointInPlayspace = new GameObject("InitialPivotPoint").transform;

            initialPivotPointInPlayspace.parent = playspace;

            currentPivotPointInPlayspace = new GameObject("CurrentPivotPoint").transform;

            currentPivotPointInPlayspace.parent = playspace;

            if (!physicalFloor)
            {
                throw new UnassignedReferenceException("physicalFloorReference");
            }

            initialPlayspace = new GameObject();
        //}
        //public void Start()
        //{
            if (playspace == null)
            {
                playspace = GameObject.FindGameObjectWithTag("XRCamera").transform;
            }

            player = GameObject.FindGameObjectWithTag("Player").transform;

            if (!player)
            {
                throw new UnassignedReferenceException("player");
            }

            if (player.TryGetComponent(out PlayerReferences playerRefs))
            {
                hands[0] = playerRefs.handL;
                hands[1] = playerRefs.handR;

            }
            else
            {
                throw new MissingComponentException("PlayerReferences on player");
            }

            InitializeDebugAxes();

            handToHandLine = gameObject.GetComponent<LineRenderer>();

            if (!handToHandLine)
            {
                throw new MissingComponentException("LineRenderer on handToHandLine");
            }

            handToHandLine.enabled = false;

            if (!layerManager)
            {
                throw new UnassignedReferenceException("layerManager");
            }

            HidePhysicalFloor();

            if (!animalRulerMesh)
            {
                throw new UnassignedReferenceException("animalRulerMesh");
            }

            animalRulerMesh.material.SetTextureOffset("_MainTex", Vector2.zero);

            if (!animalRuler)
            {
                throw new UnassignedReferenceException("animalRuler");
            }

            //average human height, need to readjust ruller
            // CustomScalling(1.77f);
          // CustomScalling(1);

            if (!teleportPlayer)
            teleportPlayer = player.GetComponent<TeleportPlayer>();
            

         //   teleportPlayer.AdjustYAccordingToWorldPulling(playspace.localScale.x );

           //  playspace.position = new Vector3(playspace.position.x, 1.77f, playspace.position.z);
            // teleportPlayer.SetManualYOffset(1.77f);

            

           // onChangeScale.Invoke(5);



            animalRuler.gameObject.SetActive(false);

            onDoubleTriggerPress += StartWorldPulling;

            onDoubleTriggerRelease += StopWorldPulling;


          

        }

       public void Start(){

//DOES NOT WORK IN BUILD...
//yield return null;
//             playspace.position = new Vector3(playspace.position.x, 1.77f, playspace.position.z);

              if(!teleportPlayer)
            teleportPlayer = player.GetComponent<TeleportPlayer>();

            onChangeScale.Invoke(1);
            // teleportPlayer.SetManualYOffset(1.77f);
        }

        // It will feel like the player is pulling the world, but really they are pushing themselves
        // in the opposite direction, with an inverse rotation, and and inverse scale.
        [ContextMenu("Start World Pulling")]
        public void StartWorldPulling()
        {
            SetInitialValues();

            animalRuler.gameObject.SetActive(true);

            handToHandLine.enabled = true;

            ShowPhysicalFloor();

            layerManager.HideLayers();

            //register our update loop to be called
            if (GameStateManager.IsAlive)
            {
                GameStateManager.Instance.RegisterUpdatableObject(this);
            }
        }

        [ContextMenu("Stop World Pulling")]
        public void StopWorldPulling()
        {
            animalRuler.gameObject.SetActive(false);

            handToHandLine.enabled = false;

            HidePhysicalFloor();

            layerManager.ShowLayers();

            //deregister our update loop not be called when not in use
            if (GameStateManager.IsAlive)
            {
                GameStateManager.Instance.DeRegisterUpdatableObject(this);
            }

            //var dif = xrTransform.GetChild(0).position.y - xrTransform.position.y ;
            //xrOrigin.CameraYOffset =  dif;

                                                                          //    xrOrigin.CameraYOffset = 1.28f * xrTransform.localScale.y;//.GetChild(0).position.y;
                                                                          // Debug.Log("stopped world pulling"); //TODO Remove

            // teleportPlayer.SetManualYOffset(currentPivotPointInPlayspace.position.y);
        }

        // Stores transforms of gameObjects and variables used to compute scale, rotation, and translation
        protected void SetInitialValues ()
        {
            UpdateLocalPivotPoint(initialPivotPointInPlayspace, controlerHandTracker.lControler_PosData.pos, controlerHandTracker.rControler_PosData.pos);

            copyOfInitialPivotPointPosition = xrTransform.TransformPoint(Vector3.forward);//initialPivotPointInPlayspace.position;

            UpdateLocalPivotPoint(currentPivotPointInPlayspace, controlerHandTracker.lControler_PosData.pos, controlerHandTracker.rControler_PosData.pos);

            // Scale

            handDistanceInPlayspace = new UpdatingValue<float>(Vector3.Distance(controlerHandTracker.lControler_PosData.pos, controlerHandTracker.rControler_PosData.pos) / playspace.localScale.x);

            float clampedInitialScale = Mathf.Clamp(playspace.localScale.x, scaleMin, scaleMax);

            playspace.localScale = Vector3.one * clampedInitialScale;

            // Copy the transform to a new gameObject.

            initialPlayspace.transform.position = xrTransform.position;

            initialPlayspace.transform.rotation = xrTransform.rotation;

            initialPlayspace.transform.localScale = xrTransform.localScale;

            UpdateDebugAxes();
        }

        float clampedNewScale;
        public float GetClampedCurrentScale() => clampedNewScale;

        //set custom scaling
        public void CustomScalling(float unclampedScaleRatio)
        {
            //UpdateLocalPivotPoint(currentPivotPointInPlayspace, hands[0].transform.position, hands[1].transform.position);

            //// Compute Scale

            //handDistanceInPlayspace.Current = Vector3.Distance(hands[0].transform.position, hands[1].transform.position) / playspace.localScale.x;

            //float unclampedScaleRatio = 1.0f / (handDistanceInPlayspace.Current / handDistanceInPlayspace.Initial);

            clampedNewScale = Mathf.Clamp(unclampedScaleRatio * initialPlayspace.transform.localScale.x, scaleMin, scaleMax);

            if (clampedNewScale > -0.001f && clampedNewScale < 0.001f)
            {
                clampedNewScale = 0.0f;
            }

            float clampedScaleRatio = clampedNewScale / initialPlayspace.transform.localScale.x;

            // Compute Rotation

            float rotateAmount = ComputeDiffRotationY(initialPivotPointInPlayspace.rotation, currentPivotPointInPlayspace.rotation);

            UpdateDebugAxes();

            // Apply Scale and Rotation and Translation

            RotateAndScalePlayspaceAroundPointThenTranslate(rotateAmount, clampedScaleRatio, clampedNewScale);

            UpdateLineRenderersScale(clampedNewScale);

            SendAvatarScaleUpdate(clampedNewScale);

            // Ruler

            UpdateRulerValue(clampedNewScale);

            //   UpdateRulerPose(hands[0].transform.position, hands[1].transform.position, clampedNewScale);

            // UpdateHandToHandLineEndpoints(hands[0].transform.position, hands[1].transform.position);

            UpdateRulerPose(controlerHandTracker.lControler_PosData.pos, controlerHandTracker.rControler_PosData.pos, clampedNewScale);

            UpdateHandToHandLineEndpoints(controlerHandTracker.lControler_PosData.pos, controlerHandTracker.rControler_PosData.pos);


        }

        // This function is used externally by the GameStateManager.
        // Compares the current transforms of the hands to the initial transforms, then calls various functions
        // to make the world pulling experience happen.
        public void OnUpdate (float unusedFloat)
        {
            UpdateLocalPivotPoint(currentPivotPointInPlayspace, controlerHandTracker.lControler_PosData.pos, controlerHandTracker.rControler_PosData.pos);

            // Compute Scale

            handDistanceInPlayspace.Current = Vector3.Distance(controlerHandTracker.lControler_PosData.pos, controlerHandTracker.rControler_PosData.pos) / playspace.localScale.x;

            float unclampedScaleRatio = 1.0f / (handDistanceInPlayspace.Current / handDistanceInPlayspace.Initial);

            clampedNewScale = Mathf.Clamp(unclampedScaleRatio * initialPlayspace.transform.localScale.x, scaleMin, scaleMax);

            if (clampedNewScale > -0.001f && clampedNewScale < 0.001f)
            {
                clampedNewScale = 0.0f;
            }

            float clampedScaleRatio = clampedNewScale / initialPlayspace.transform.localScale.x;

            // Compute Rotation

            float rotateAmount = ComputeDiffRotationY(initialPivotPointInPlayspace.rotation, currentPivotPointInPlayspace.rotation);

            UpdateDebugAxes();

            // Apply Scale and Rotation and Translation

            RotateAndScalePlayspaceAroundPointThenTranslate(rotateAmount, clampedScaleRatio, clampedNewScale);

            UpdateLineRenderersScale(clampedNewScale);

            SendAvatarScaleUpdate(clampedNewScale);

            // Ruler

            UpdateRulerValue(clampedNewScale);

            //UpdateRulerPose(hands[0].transform.position, hands[1].transform.position, clampedNewScale);

            //UpdateHandToHandLineEndpoints(hands[0].transform.position, hands[1].transform.position);


            UpdateRulerPose(controlerHandTracker.ReturnCurrentActiveInputPosition_LEFT(), controlerHandTracker.ReturnCurrentActiveInputPosition_Right(), clampedNewScale);

            UpdateHandToHandLineEndpoints(controlerHandTracker.ReturnCurrentActiveInputPosition_LEFT(), controlerHandTracker.ReturnCurrentActiveInputPosition_Right());

            //controlerHandTracker.lControler_PosData.pos, controlerHandTracker.rControler_PosData.pos



         //   onScaleUpdated.Invoke(clampedNewScale);
            onChangeScale.Invoke(clampedNewScale);
        }

        public UnityEvent<float> onScaleUpdated = new UnityEvent<float>();
        public void RotateAndScalePlayspaceAroundPointThenTranslate(float amount, float scaleRatio, float newScale)
        {
            amount *= -1.0f; // Invert rotation amount

            // Store initial playspace values
            Vector3 actualInitialPlayspacePosition = initialPlayspace.transform.position;
            Quaternion actualInitialPlayspaceRotation = initialPlayspace.transform.rotation;

            // Perform rotation
            initialPlayspace.transform.RotateAround(copyOfInitialPivotPointPosition, Vector3.up, amount);
            xrTransform.rotation = initialPlayspace.transform.rotation;

            // Restore initial playspace values
            initialPlayspace.transform.rotation = actualInitialPlayspaceRotation;

            // Define a small threshold for scale changes to be considered intentional
            const float scaleThreshold = 0.1f;  // Adjust this value based on your needs

            //      newScale *= 0.1f;
            // Apply gradual scaling only if there's a clear intent to scale
            //if (Mathf.Abs(1.0f - scaleRatio) > scaleThreshold)
            //{
            // Apply linear scaling
            //    xrTransform.localScale = Vector3.Lerp(xrTransform.localScale, new Vector3(newScale, newScale, newScale), Time.deltaTime * 5); // Adjust '5' to control rate of scaling
            // }

            if (0.53f < xrOrigin.CameraYOffset && xrOrigin.CameraYOffset < 9.9f)
            {
                xrTransform.localScale = new Vector3(newScale, newScale, newScale);
                Debug.Log("NEW SCALE : " + newScale);
              //  onScaleUpdated.Invoke(newScale - 1);//Vector3.Lerp(xrTransform.localScale, new Vector3(newScale, newScale, newScale), Time.deltaTime * 5);
            }

            // Adjust CameraYOffset linearly based on the current scale
            float initialYOffset = xrOrigin.CameraYOffset;
            if (Mathf.Abs(1.0f - scaleRatio) > scaleThreshold)
            {
                float adjustedYOffset = initialYOffset + (newScale - xrTransform.localScale.x); // Simple linear adjustment
                xrOrigin.CameraYOffset = Mathf.Clamp(adjustedYOffset, 0.51f, 9.9f); // Adjust these values as necessary
            }
        }


        private float CalculateSmoothYOffset(float initialYOffset, float scaleRatio, float newScale)
{
    // Adjust based on whether scaling up or down
    float adjustmentFactor = newScale > xrTransform.localScale.x ? Mathf.Log10(scaleRatio) + 1 : Mathf.Sqrt(scaleRatio);
    return initialYOffset * adjustmentFactor;
}


        // Computes the transform of an invisible object that has the average position of the hands, a rotation corresponding to 
        // the line drawn between the two hands, and a scale corresponding to the distance between the hands.
        // Makes all values relative to the playspace, so that even as the playspace scales, rotates, and translates, we can compute
        // the correct difference corresponding to the user's physical actions
        public void UpdateLocalPivotPoint (Transform pivotPointInPlayspace, Vector3 hand0Position, Vector3 hand1Position)
        {
            pivotPointInPlayspace.localPosition = playspace.InverseTransformPoint((hand0Position + hand1Position) / 2);

            Vector3 deltaHandPositionsXZ = new Vector3(
                (hand1Position - hand0Position).x,
                0,
                (hand1Position - hand0Position).z
            );

            pivotPointInPlayspace.localRotation = Quaternion.Inverse(playspace.rotation) * Quaternion.LookRotation(deltaHandPositionsXZ, Vector3.up);
        }

        // Takes a rotation difference and returns that rotation difference projected to just a rotation around the Y axis.
        public float ComputeDiffRotationY (Quaternion initial, Quaternion current)
        {
            float result = (current.eulerAngles - initial.eulerAngles).y;

            if (result > -0.001f && result < 0.001f)
            {
                result = 0.0f;
            }

            return result;
        }

        // Makes the ruler always be between the hands and the right size.
        public void UpdateRulerPose (Vector3 hand0Position, Vector3 hand1Position, float scale)
        {
            animalRuler.position = ((hand0Position + hand1Position) / 2);

            animalRuler.localScale = Vector3.one * scale;
        }

        // Makes the hand-to-hand line always be connected to both hands.
        public void UpdateHandToHandLineEndpoints (Vector3 hand0Position, Vector3 hand1Position)
        {
            handToHandLine.SetPosition(0, hand0Position);

            handToHandLine.SetPosition(1, hand1Position);
        }

public float offsetAdjustment = 0.05f;
public float offsetMultiplier = 1f;

 float power = 2.0f;
public float middleValueStrength = 1f;
        // Scales the playspace scale range to the ruler's texture offset range
        public float ComputeRulerValue (float playerScale)
        {
            const float rulerMin = 0.0f;

            const float rulerMax = 1.0f;

            if (scaleMax - scaleMin == 0)
            {
                Debug.LogWarning("scaleMax - scaleMin was zero. Setting to 0.1m and 10m and proceeding.");

                scaleMin = 0.1f;

                scaleMax = 12f;
            }

            float percentScale = (playerScale - scaleMin) / (scaleMax - scaleMin);


//  float adjustedPercentScale;

//     if (percentScale < 0.5f)
//     {
//         adjustedPercentScale = Mathf.Pow(percentScale / 0.5f, power) * 0.5f;
//     }
//     else
//     {
//         adjustedPercentScale = 0.5f + Mathf.Pow((percentScale - 0.5f) / 0.5f, power) * 0.5f;
//     }




//             return  (adjustedPercentScale * (rulerMax - rulerMin)) + rulerMin;
  return  (percentScale * (rulerMax - rulerMin)) + rulerMin;
        }

        // Updates the ruler's texture offset
        public void UpdateRulerValue (float newScale)
        {
             var rulerValue = ComputeRulerValue(newScale);

    float min = ComputeRulerValue(scaleMin);

    float max = ComputeRulerValue(scaleMax);

    float textureRange = max - min;

rulerValue *= offsetMultiplier;


    // Calculate the amount by which the ruler value exceeds the max or min
    //float excessValue = Mathf.Max(rulerValue - max, min - rulerValue, 0f);

    // Calculate the amount by which to adjust the ruler value to fit within the texture range
   // float adjustment = excessValue * (textureRange / (textureRange - 2 * Mathf.Abs(excessValue)));

    // Adjust the ruler value and clamp it to the range defined by min and max
    float adjustedRulerValue = Mathf.Clamp(rulerValue , min, max);

    //adjustedRulerValue -=  adjustment;

    // Set the texture offset based on the adjusted ruler value
   // animalRulerMesh.material.SetTextureOffset("_MainTex", new Vector2(adjustedRulerValue - offsetAdjustment , 0));

            animalRulerMesh.material.SetTextureOffset("_BaseMap", new Vector2(adjustedRulerValue - offsetAdjustment, 0));

        }

        // TODO: Makes the current line renderer scale change proportionally with the playspace scale
        public void UpdateLineRenderersScale (float newScale)
        {
            // TODO: update size of drawing strokes here 
        }

        // TODO: Sends the playspace scale to other multiplayer clients, so their prefab avatar head and
        // hand represetnations of your own client are the correct size
        public void SendAvatarScaleUpdate (float newScale)
        {
            //TODO: send message that avatar scale changed to other clients
        }

        // Shows motion-sickness helper
        private void ShowPhysicalFloor ()
        {
            physicalFloor.SetActive(true);
        }

        // Hides motion-sickness helper
        private void HidePhysicalFloor ()
        {
            physicalFloor.SetActive(false);
        }

        // Creates debug axes iff showDebugAxes is on
        private void InitializeDebugAxes()
        {
            if (!showDebugAxes)
            {
                return;
            }

            var initialPivotPointAxes = Instantiate(debugAxes);

            initialPivotPointAxes.transform.parent = initialPivotPointInPlayspace;

            initialPivotPointAxes.transform.localPosition = Vector3.zero;

            initialPivotPointAxes.transform.localRotation = Quaternion.identity;

            initialPivotPointAxes.transform.GetChild(0).GetComponent<Renderer>().material = materials[0];

            initialPivotPointAxes.transform.GetChild(1).GetComponent<Renderer>().material = materials[0];

            initialPivotPointAxes.transform.GetChild(2).GetComponent<Renderer>().material = materials[0];

            var currentPivotPointAxes = Instantiate(debugAxes);

            currentPivotPointAxes.transform.parent = currentPivotPointInPlayspace;

            currentPivotPointAxes.transform.localPosition = Vector3.zero;

            currentPivotPointAxes.transform.localRotation = Quaternion.identity;

            currentPivotPointAxes.transform.GetChild(0).GetComponent<Renderer>().material = materials[1];

            currentPivotPointAxes.transform.GetChild(1).GetComponent<Renderer>().material = materials[1];

            currentPivotPointAxes.transform.GetChild(2).GetComponent<Renderer>().material = materials[1];

            initialPlayspaceAxes = Instantiate(debugAxes);

            initialPlayspaceAxes.transform.GetChild(0).GetComponent<Renderer>().material = materials[2];

            initialPlayspaceAxes.transform.GetChild(1).GetComponent<Renderer>().material = materials[2];

            initialPlayspaceAxes.transform.GetChild(2).GetComponent<Renderer>().material = materials[2];

            currentPlayspaceAxes = Instantiate(debugAxes);

            currentPlayspaceAxes.transform.parent = playspace;

            currentPlayspaceAxes.transform.GetChild(0).GetComponent<Renderer>().material = materials[3];

            currentPlayspaceAxes.transform.GetChild(1).GetComponent<Renderer>().material = materials[3];

            currentPlayspaceAxes.transform.GetChild(2).GetComponent<Renderer>().material = materials[3];

            var hand0Axes = Instantiate(debugAxes);

            hand0Axes.transform.parent = hands[0];

            hand0Axes.transform.localPosition = Vector3.zero;

            hand0Axes.transform.localRotation = Quaternion.identity;

            hand0Axes.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

            hand0Axes.transform.GetChild(0).GetComponent<Renderer>().material = materials[4];

            hand0Axes.transform.GetChild(1).GetComponent<Renderer>().material = materials[4];

            hand0Axes.transform.GetChild(2).GetComponent<Renderer>().material = materials[4];

            var hand1Axes = Instantiate(debugAxes);

            hand1Axes.transform.parent = hands[1];

            hand1Axes.transform.localPosition = Vector3.zero;

            hand1Axes.transform.localRotation = Quaternion.identity;

            hand1Axes.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

            hand1Axes.transform.GetChild(0).GetComponent<Renderer>().material = materials[5];

            hand1Axes.transform.GetChild(1).GetComponent<Renderer>().material = materials[5];

            hand1Axes.transform.GetChild(2).GetComponent<Renderer>().material = materials[5];

            copyOfInitialPivotPointPositionAxes = Instantiate(debugAxes);

            copyOfInitialPivotPointPositionAxes.transform.localPosition = Vector3.zero;

            copyOfInitialPivotPointPositionAxes.transform.localRotation = Quaternion.identity;

            copyOfInitialPivotPointPositionAxes.transform.GetChild(0).GetComponent<Renderer>().material = materials[6];

            copyOfInitialPivotPointPositionAxes.transform.GetChild(1).GetComponent<Renderer>().material = materials[6];

            copyOfInitialPivotPointPositionAxes.transform.GetChild(2).GetComponent<Renderer>().material = materials[6];
        }

        // Updates the debug axes that specifically need to be recalculated; others are parented
        private void UpdateDebugAxes ()
        {
            if (!showDebugAxes)
            {
                return;
            }

            initialPlayspaceAxes.transform.position = initialPlayspace.transform.position;

            initialPlayspaceAxes.transform.rotation = initialPlayspace.transform.rotation;

            copyOfInitialPivotPointPositionAxes.transform.position = copyOfInitialPivotPointPosition;
        }

        public void OnDestroy ()
        {
            Destroy(initialPlayspace);
        }
    }
}




