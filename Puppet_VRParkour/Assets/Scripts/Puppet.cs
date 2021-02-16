using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puppet : MonoBehaviour
{
    private GameObject handAnchor;
    private GameObject centerEyeAnchor;
    private GameObject cameraRig;
    private GameObject secondCamera;
    public OVRHand rightHand;
    public OVRHand leftHand;

    // Puppet's bones
    private GameObject leftLeg1;
    private GameObject rightLeg1;
    private GameObject leftLeg2;
    private GameObject rightLeg2;
    private GameObject leftArm;
    private GameObject rightArm;

    // Hand's bones
    private GameObject indexBone1;
    private GameObject middleBone1;
    private GameObject indexBone2;
    private GameObject middleBone2;

    // Puppet's bones initial rotations
    private Quaternion leftLeg1_initRot;
    private Quaternion rightLeg1_initRot;
    private Quaternion leftLeg2_initRot;
    private Quaternion rightLeg2_initRot;
    private Quaternion leftArm_initRot;
    private Quaternion rightArm_initRot;

    private float handRotation = 0;
    private float indexRotation = 0;
    private float middleRotation = 0;

    private bool puppetView = false;
    private GameObject puppetViewPoint;
    private GameObject puppeteerViewPoint;

    private float step = 0.04f;
    private float jumpStep = 0.3f;
    private float maxDistance = 40.0f;
    private float scale = 2.5f;

    void Start()
    {
        handAnchor = GameObject.Find("RightHandAnchor");
        centerEyeAnchor = GameObject.Find("CenterEyeAnchor");
        puppetViewPoint = GameObject.Find("PuppetViewPoint");
        puppeteerViewPoint = GameObject.Find("PuppeteerViewPoint");
        cameraRig = GameObject.Find("OVRCameraRig");
        secondCamera = GameObject.Find("ExternalCamera");

        leftLeg1 = GameObject.Find("B-thigh_L");
        rightLeg1 = GameObject.Find("B-thigh_R");
        leftLeg2 = GameObject.Find("B-shin_L");
        rightLeg2 = GameObject.Find("B-shin_R");
        leftArm = GameObject.Find("B-upper_arm_L");
        rightArm= GameObject.Find("B-upper_arm_R");
       
        leftLeg1_initRot = leftLeg1.transform.localRotation;
        rightLeg1_initRot = rightLeg1.transform.localRotation;
        leftLeg2_initRot = leftLeg2.transform.localRotation;
        rightLeg2_initRot = rightLeg2.transform.localRotation;
        leftArm_initRot = leftArm.transform.localRotation;
        rightArm_initRot = rightArm.transform.localRotation;


        // Initially : puppeteer's view
        cameraRig.transform.parent = puppeteerViewPoint.transform;
        cameraRig.transform.localPosition = new Vector3(0, 0, 0);
        cameraRig.transform.localScale = new Vector3(1, 1, 1);
        cameraRig.transform.localRotation = new Quaternion(0, 0, 0, 1);
        cameraRig.transform.parent = null;

        secondCamera.transform.parent = puppetViewPoint.transform;
        secondCamera.transform.localPosition = new Vector3(0, 0, 0);
        secondCamera.transform.localScale = new Vector3(0, 0, 0);
        secondCamera.transform.localRotation = new Quaternion(0, 0, 0, 1);
    }

    void Update()
    {
        if (indexBone1 == null || middleBone1 == null || indexBone2 == null || middleBone2 == null)
        {
            indexBone1 = GameObject.Find("OVRHandPrefab_R/Bones/Hand_Start/Hand_Index1");
            middleBone1 = GameObject.Find("OVRHandPrefab_R/Bones/Hand_Start/Hand_Middle1");
            indexBone2 = GameObject.Find("OVRHandPrefab_R/Bones/Hand_Start/Hand_Index1/Hand_Index2");
            middleBone2 = GameObject.Find("OVRHandPrefab_R/Bones/Hand_Start/Hand_Middle1/Hand_Middle2");
        }
        
        // Scale 
        transform.localScale = new Vector3(scale, scale, scale);
        step = 0.03f * scale;
        jumpStep = 0.3f * scale;
        maxDistance = 40.0f * scale;


        // Assign hand bones rotation to puppet's legs rotation
        leftLeg1.transform.localRotation = leftLeg1_initRot * new Quaternion(0, - indexBone1.transform.localRotation.z-0.2f, 0, indexBone1.transform.localRotation.w);
        rightLeg1.transform.localRotation = rightLeg1_initRot * new Quaternion(0, - middleBone1.transform.localRotation.z-0.2f, 0, middleBone1.transform.localRotation.w);
        leftLeg2.transform.localRotation = leftLeg2_initRot * new Quaternion(0, - indexBone2.transform.localRotation.z, 0, indexBone2.transform.localRotation.w);
        rightLeg2.transform.localRotation = rightLeg2_initRot * new Quaternion(0, - middleBone2.transform.localRotation.z, 0, middleBone2.transform.localRotation.w);

        leftArm.transform.localRotation = leftArm_initRot * new Quaternion(0, - indexBone1.transform.localRotation.z-0.2f, 0, indexBone1.transform.localRotation.w);
        rightArm.transform.localRotation = rightArm_initRot * new Quaternion(0, -middleBone1.transform.localRotation.z-0.2f, 0, middleBone1.transform.localRotation.w);

        // Jumping
        if (indexRotation - indexBone1.transform.localRotation.z > 0.05f && middleRotation - middleBone1.transform.localRotation.z > 0.05f)
        {
            transform.position += transform.TransformDirection(jumpStep * Vector3.up);
        }

        // Walking 
        else if (indexRotation - indexBone1.transform.localRotation.z > 0.005f || middleRotation - middleBone1.transform.localRotation.z > 0.005f)
        {
            transform.position += transform.TransformDirection(step * Vector3.fwd);
        }

        // Rotating
        else if ((Mathf.Abs(handRotation - handAnchor.transform.localRotation.y) > 0.001) && !puppetView)
        { 
            float correctedHandRotation = 3.5f * handAnchor.transform.localRotation.y - 1.05f;
            this.transform.localRotation = new Quaternion(transform.localRotation.x, correctedHandRotation, transform.localRotation.z, transform.localRotation.w);
        }
       
        if (puppetView)
        {
            this.transform.rotation = new Quaternion(transform.rotation.x, centerEyeAnchor.transform.rotation.y, transform.rotation.z, centerEyeAnchor.transform.rotation.w);
            cameraRig.transform.position = puppetViewPoint.transform.position;
        }
        
        // Center puppeteer view
        if(!puppetView && (cameraRig.transform.position - transform.position).magnitude > maxDistance)
        {
            cameraRig.transform.position = puppeteerViewPoint.transform.position;
            cameraRig.transform.rotation = puppeteerViewPoint.transform.rotation;
        }
        
        indexRotation = indexBone1.transform.localRotation.z;
        middleRotation = middleBone1.transform.localRotation.z;
        handRotation = handAnchor.transform.localRotation.y;

        // View Point change
        if (OVRInput.GetDown(OVRInput.Button.Three))
        {
            puppetView = !puppetView;
            if (puppetView)
            {
                // Principal camera at puppetViewPoint position
                cameraRig.transform.parent = puppetViewPoint.transform;
                cameraRig.transform.localPosition = new Vector3(0, 0, 0);
                cameraRig.transform.localScale = new Vector3(1, 1, 1);
                cameraRig.transform.localRotation = new Quaternion(0, 0, 0, 1);
                cameraRig.transform.parent = null;

                // Second camera at puppeteerViewPoint position
                secondCamera.transform.parent = puppeteerViewPoint.transform;
                secondCamera.transform.localPosition = new Vector3(0, 0, 0);
                secondCamera.transform.localScale = new Vector3(0, 0, 0);
                secondCamera.transform.localRotation = new Quaternion(0, 0, 0, 1);
            }
            else
            {
                // Principal camera at puppeteerViewPoint position
                cameraRig.transform.parent = puppeteerViewPoint.transform;
                cameraRig.transform.localPosition = new Vector3(0, 0, 0);
                cameraRig.transform.localScale = new Vector3(1, 1, 1);
                cameraRig.transform.localRotation = new Quaternion(0, 0, 0, 1);
                cameraRig.transform.parent = null;

                // Second camera at puppetViewPoint position
                secondCamera.transform.parent = puppetViewPoint.transform;
                secondCamera.transform.localPosition = new Vector3(0, 0, 0);
                secondCamera.transform.localScale = new Vector3(0, 0, 0);
                secondCamera.transform.localRotation = new Quaternion(0, 0, 0, 1);
            }
        }

        // Change Size
        if (rightHand.GetFingerIsPinching(OVRHand.HandFinger.Index) && scale < 5.0f){
            scale += 0.01f;
        }
        if (rightHand.GetFingerIsPinching(OVRHand.HandFinger.Middle) && scale > 0.5f){
            scale -= 0.01f;
        }
        // Center view
        if (leftHand.GetFingerIsPinching(OVRHand.HandFinger.Middle) && puppeteerViewPoint){
            cameraRig.transform.position = puppeteerViewPoint.transform.position;
            cameraRig.transform.rotation = puppeteerViewPoint.transform.rotation;
        }

    }
   
}
