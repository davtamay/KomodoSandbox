using Komodo.Runtime;
using Komodo.Utilities;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ImpressStretchManager : SingletonComponent<ImpressStretchManager>, IUpdatable
{
    public static ImpressStretchManager Instance
    {
        get { return ((ImpressStretchManager)_Instance); }
        set { _Instance = value; }
    }

    [ShowOnly] public Transform firstObjectGrabbed;

    //get parent if we are switching objects between hands we want to keep track of were to place it back, to avoid hierachy parenting displacement
    [ShowOnly] public Transform originalParentOfFirstHandTransform;

    [ShowOnly] public Transform secondObjectGrabbed;

    [ShowOnly] public Transform originalParentOfSecondHandTransform;

    [ShowOnly] public Transform[] hands = new Transform[2];

    public UnityEvent onStretchStart;

    public UnityEvent onStretchEnd;

    [ShowOnly] public Transform endpoint1;

    private static Transform endpoint0;

    [ShowOnly] public Transform midpoint;

    [ShowOnly] public Transform stretchParent;

    [ShowOnly] public bool didStartStretching;

    public bool debug;

    public GameObject axesPrefab;

    private float initialDistance;

    private Vector3 initialScale;

    private float initialLineMulti;

    private GameObject objectPoseDisplay;

    public DrawingManager impressDrawingManager;
    public void Awake()
    {
        //set our manager's alive state to true to detect if it exists
        var initManager = Instance;

        onStretchStart.AddListener(() => StretchStart());

        onStretchEnd.AddListener(() => StretchRelease());

        var stretchEndpoint0Object = CreatePivotPoint("StretchEndpoint0", debug, axesPrefab);

        endpoint0 = stretchEndpoint0Object.transform;

        endpoint0.SetParent(transform.parent, true);

        endpoint0.localPosition = Vector3.zero;

        var stretchEndpoint1Object = CreatePivotPoint("StretchEndpoint1", debug, axesPrefab);

        endpoint1 = stretchEndpoint1Object.transform;

        endpoint1.parent = transform.parent;

        var stretchMidpointObject = CreatePivotPoint("StretchMidpoint", debug, axesPrefab);

        midpoint = stretchMidpointObject.transform;

        midpoint.SetParent(transform.parent, true);

        midpoint.localPosition = Vector3.zero;

        objectPoseDisplay = CreatePivotPoint("ObjectPose", debug, axesPrefab);
    }

    /**
    * Creates a GameObject and gives it a name.
    * name: name of the GameObject.
    * doDisplay: if true, instantiate from a prefab
    *   Otherwise, return a default GameObject.
    * prefab: the prefab to create and name.
    */
    public GameObject CreatePivotPoint(string name, bool doDisplay, GameObject prefab)
    {
        if (doDisplay)
        {
            var result = Instantiate(prefab);

            result.name = name;

            return result;
        }

        return new GameObject(name);
    }

    public void Start()
    {
        var player = GameObject.FindWithTag(TagList.player);

        if (player)
        {
            if (player.TryGetComponent(out PlayerReferences pR))
            {
                hands[0] = pR.handL;
                hands[1] = pR.handR;
            }
        }

        //set references for parent
        stretchParent = endpoint1.parent;
    }

    LineRenderer[] currentLR;
    int[] lRNetIDs;
    float[] currentLRInitialValues;
    private void StretchStart()
    {
        //Debug.Log("STRETCH");
      
        ////if (firstObjectGrabbed.get(out LineRenderer lr))
        ////{
        ////    Debug.Log(currentLR.gameObject.name); //Debug.Log(lr)
        ////    currentLR = lr;
        ////}
        currentLR = firstObjectGrabbed.GetComponentsInChildren<LineRenderer>();
      

        if (currentLR.Length > 0)
        {
            currentLRInitialValues = new float[currentLR.Length];
            lRNetIDs = new int[currentLR.Length];

            for (int i = 0; i < currentLR.Length; i++)
            {
                currentLRInitialValues[i] =  currentLR[i].widthMultiplier;
                lRNetIDs[i] = currentLR[i].GetComponentInParent<NetworkedGameObject>().thisEntityID;
            }
            //Debug.Log(currentLR.gameObject.name);
            //initialLineMulti = currentLR.widthMultiplier;
        }




        if (GameStateManager.IsAlive)
        {
            GameStateManager.Instance.RegisterUpdatableObject(this);
        }

       
        //if (firstObjectGrabbed.CompareTag(TagList.drawing))
        //{
        //    currentLR = firstObjectGrabbed.GetComponentInChildren<LineRenderer>();
        //    Debug.Log(currentLR.gameObject.name);
        //}
    }

    private void StretchRelease()
    {
        if (GameStateManager.IsAlive)
        {
            GameStateManager.Instance.DeRegisterUpdatableObject(this);
        }

        currentLR = null;

        if (firstObjectGrabbed.CompareTag(TagList.drawing))
        {
            currentLR = null;
        }
    }

    public void OnUpdate(float realltime)
    {
      //if(firstObjectGrabbed == null)
      //  {
      //      GameStateManager.Instance.DeRegisterUpdatableObject(this);
      //      return;



      //  }

        if ( didStartStretching == false)
        {
            firstObjectGrabbed.SetParent(stretchParent, true);

            UpdateGrabPoints();

            InitializeScale();

            //if (firstObjectGrabbed == null)
            //    return;

            if (objectPoseDisplay == null)
                objectPoseDisplay = CreatePivotPoint("ObjectPose", debug, axesPrefab);
            //    return;

            objectPoseDisplay.transform.SetParent(firstObjectGrabbed);

            objectPoseDisplay.transform.localPosition = Vector3.zero;

            firstObjectGrabbed.SetParent(midpoint, true);

            didStartStretching = true;

            return;
        }

        UpdateGrabPoints();

        UpdateScale();

        UpdateRotationAndPosition();
    }

    private void UpdateRotationAndPosition()
    {
        endpoint1.rotation = hands[1].transform.rotation;

        endpoint0.rotation = hands[0].transform.rotation;

        Vector3 averageUp = (endpoint0.up + endpoint1.up) / 2;

        midpoint.LookAt(endpoint0.position, averageUp);
    }

    private void UpdateGrabPoints()
    {
        endpoint1.position = hands[1].transform.position;

        endpoint0.position = hands[0].transform.position;

        midpoint.position = (endpoint0.position + endpoint1.position) / 2;

        endpoint1.rotation = hands[1].transform.rotation;

        endpoint0.rotation = hands[0].transform.rotation;

        Vector3 averageUp = (endpoint0.up + endpoint1.up) / 2;

        midpoint.LookAt(endpoint0.position, averageUp);
    }

    private void InitializeScale()
    {
        initialDistance = Vector3.Distance(endpoint1.position, endpoint0.position);

        initialScale = Vector3.one;

        midpoint.localScale = Vector3.one;
    }

    
    private void UpdateScale()
    {
        var currentScaleRatio = GetCurrentScaleRatio();

      //  InitializeScale();

        if (float.IsNaN(firstObjectGrabbed.localScale.y))
        {
            Debug.LogError("First Object Grabbed's' local scale was NaN");
        }

        midpoint.localScale = initialScale * currentScaleRatio;


        if (currentLR.Length > 0)
        {
            for (int i = 0; i < currentLR.Length; i++)
            {
                currentLR[i].widthMultiplier = currentLRInitialValues[i] * currentScaleRatio;

                impressDrawingManager.SendDrawUpdate2(lRNetIDs[i], Entity_Type.Line,currentLR[i].widthMultiplier, Vector3.zero, Vector4.zero);
            }
            //Vector3.Distance(endpoint1.position, endpoint0.position)/initialDistance;//(currentScaleRatio *0.5f);//currentScaleRatio * midpoint.localScale.x;
        }
        // else




        //if (firstObjectGrabbed.CompareTag(TagList.drawing))
        //{
        //    firstObjectGrabbed.GetComponent<LineRenderer>()
        //}
    }

    private float GetCurrentScaleRatio()
    {
        return Vector3.Distance(hands[0].transform.position, hands[1].transform.position) / initialDistance;
    }
}
