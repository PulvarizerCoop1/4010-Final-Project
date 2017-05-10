using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Susan : MonoBehaviour {

    private SteamVR_TrackedObject trackedObj;

    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    // Refer to laser prefab
    public GameObject laserPrefab;
    // RSusan the laser
    private GameObject laser;
  
    private Transform laserTransform;
    // Position the laser hits
    private Vector3 hitPoint;

    private void ShowLaser(RaycastHit hit)
    {
        // Make RSusan visible
        laser.SetActive(true);
        // Position the laser
        laserTransform.position = Vector3.Lerp(trackedObj.transform.position, hitPoint, .5f);
        // Point laser to the collision point of the raycast
        laserTransform.LookAt(hitPoint);
        // Scale the laser to fit
        laserTransform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y,
            hit.distance);
    }

    void start()
    {
        // Create the laser
        laser = Instantiate(laserPrefab);
        laserTransform = laser.transform;
    }

    // Update is called once per frame
    void Update ()
    {
        // If touchpad is held down
        //if (Controller.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
        //{
            RaycastHit hit;

            // Get hit point and display laser
            if (Physics.Raycast(trackedObj.transform.position, transform.forward, out hit, 100))
            {
                hitPoint = hit.point;
                ShowLaser(hit);
            }
        //}
        //else // Hide laser when touchpad released
        //{
        //    laser.SetActive(false);
        //}
    }
}
