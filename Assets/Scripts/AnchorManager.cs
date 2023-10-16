using System;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnchorManager : MonoBehaviour
{
    [SerializeField]
    private InputActionReference rightGrip;
    [SerializeField]
    private GameObject anchorPreview;
    [SerializeField]
    private GameObject anchorPrefab;
    [SerializeField]
    private float maxDriftDelay = 0.5f;

    private float currrDriftDelay = 0f;

    private Dictionary<ulong, GameObject> anchorMap= new Dictionary<ulong, GameObject>();
    // Start is called before the first frame update

    private void OnEnable()
    {
        rightGrip.action.started += OnRightGripPressed;
        rightGrip.action.canceled += OnRightGripReleased;
        PXR_Manager.AnchorEntityCreated += AnchorEntityCreated;
    }

    private void OnDisable()
    {
        rightGrip.action.started -= OnRightGripPressed;
        rightGrip.action.canceled -= OnRightGripReleased;
        PXR_Manager.AnchorEntityCreated -= AnchorEntityCreated;
    }

    private void FixedUpdate()
    {
        HandleSpatialDrift();
    }
    
    //called on action.started
    private void OnRightGripPressed(InputAction.CallbackContext callback)
    {
        ShowAnchorPreview();
    }

    //called on action.release
    private void OnRightGripReleased(InputAction.CallbackContext callback)
    {
        CreateAnchor();
    }

    private void ShowAnchorPreview()
    {
        //Show anchor
        anchorPreview.SetActive(true);
    }

    private void CreateAnchor()
    {
        //hide anchor
        anchorPreview.SetActive(false);
        //Use Spatial Anchor Api to create anchor
        //This will  trigger AnchorEntityCreatedEvent
        PXR_MixedReality.CreateAnchorEntity(anchorPreview.transform.position, anchorPreview.transform.rotation, out ulong taskID);
    }

    private void AnchorEntityCreated(PxrEventAnchorEntityCreated result)
    {
        if(result.result == PxrResult.SUCCESS)
        {
            GameObject anchorObject = Instantiate(anchorPrefab);

            PXR_MixedReality.GetAnchorPose(result.anchorHandle, out var rotation, out var positon);
            anchorObject.transform.rotation = rotation;
            anchorObject.transform.position = positon;

            //Keep track of our anchors to handle spatial drift
            anchorMap.Add(result.anchorHandle, anchorObject);
        }
    }

    private void HandleSpatialDrift()
    {
        //if no anchors, dont need to handle spatial
        if (anchorMap.Count == 0)
            return;

        currrDriftDelay += Time.deltaTime;
        if(currrDriftDelay >= maxDriftDelay)
        {
            currrDriftDelay = 0;
            foreach (var handlePair in anchorMap)
            {
                var handle = handlePair.Key;
                var anchorObj = handlePair.Value;

                if (handle == UInt64.MinValue)
                {
                    Debug.LogError("Handle is invalid");
                    continue;
                }

                PXR_MixedReality.GetAnchorPose(handle, out var rotation, out var position);
                anchorObj.transform.rotation = rotation;
                anchorObj.transform.position = position;
            }
        }
    }

}
