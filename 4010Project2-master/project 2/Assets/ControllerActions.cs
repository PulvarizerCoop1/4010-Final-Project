/*
 * Adam Kacmarsky, Jordan Cooper, Colin Riley
 * 
 * Overall:
 * 	Colin developed the environment and helped with selection/manipulation
 * 	Jordan did the selection/manipulation
 * 	Adam worked on navigation
 * However most of the work was done in lab and together.
 * 
 * Some code inspiration for how to implement selection and manipulation taken from
 * 		https://www.raywenderlich.com/149239/htc-vive-tutorial-unity
 * 
 * Instructions
 * Use the grip button on the ccontroller to pick up and object and move the controller to manipulate it.
 * When an object is in hand, click the right side of the touch pad to increase the size of it.  Click the
 * 		left side of the touch pad to scale the object down.
 * Press the trigger down to set the location of the teleport and release it to go to that location.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class ControllerActions : MonoBehaviour
{

    // Use this for initialization
    private SteamVR_TrackedObject trackedObject;

    // Store the GameObject that the trigger is currently colliding with,
    //  so we can grab the object
    private GameObject collidingObject;
    // Serves as a reference to the GameObject that the player is currently
    //  grabbing
    private GameObject objectInHand;

    // cube object to make teleport location
	private	GameObject cube;


    public GameObject bird;
    //public sound file (caw and flap)
    public AudioSource caw;
    public AudioSource flap;

    // bool for limiting teleportation, untested
    private bool canTP = true;

    public void birdMove()
    {
        canTP = false;
        // Position the bird over the user's position
        Vector3 birdPos = transform.parent.position;
        Vector3 userPos = transform.parent.position;
        birdPos += new Vector3(0, 5f, 0);

        // Bird Caw's
        caw.Play();
        
        // Bird Picks up player
        for (float i = 0.5f; i < 9.5; i += 3)
        {
            Thread.Sleep(500);
            // Bird flaps
            flap.Play();

            // Raise bird
            bird.transform.position += new Vector3(0, i, 0);
            // Raise user:
            transform.parent.position += new Vector3(0, i, 0);
        }

        // Get the travel distance
        Vector3 destination = cube.transform.position;


        // Fly player to the location in 5 iterations
        for (float i = 5; i >= 1; --i)
        {
            // bird flaps
            flap.Play();
            Thread.Sleep(500);
            // change player position
            userPos.x = destination.x / i;
            userPos.z = destination.z / i;
            // change bird position in x and z direction
            birdPos.x = destination.x / i;
            birdPos.z = destination.z / i;

            // Update user and bird positions
            transform.parent.position = userPos;
            bird.transform.position = birdPos;
        }

        // Drop the player off
        for (float i = 9.5f; i >= 0.5f; i -= 3)
        {
            // bird flaps
            flap.Play();

            Thread.Sleep(500);

            // Lower user:
            transform.parent.position -= new Vector3(0, i, 0);

            // Lower bird
            bird.transform.position -= new Vector3(0, i, 0);
        }
        // bird flys away and caw's
        caw.Play();

        // Hide the bird
        bird.transform.position = new Vector3(-.2f, 10, -30);
    }

    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObject.index); }
    }

    void Awake()
    {
        trackedObject = GetComponent<SteamVR_TrackedObject>();
        //canTP = true;
    }

    // Accepts Collider as parameter and uses its GameObject as the collidingObject
    //  for grabbing and releasing
    private void SetCollidingObject(Collider col)
    {
        // Doesn't make the GameObject a potential grab target if player is
        //  holding something or the object has no rigid body
        if (collidingObject || !col.GetComponent<Rigidbody>())
        {
            return;
        }
        // Assigns the object as a potential grab target
        collidingObject = col.gameObject;
    }

    // Handle what should happen when the trigger collider enters and exits
    //  another colider
    // When the trigger collider enters another, sets up the collider as
    //  a potential grab target
    public void OnTriggerEnter(Collider other)
    {
        SetCollidingObject(other);
    }

    // Ensures the target is set when the player holds a controller over an object.
    public void OnTriggerStay(Collider other)
    {
        SetCollidingObject(other);
    }

    // When the collider exits an object, abandoning an ugrabbed target.
    // Removes it's target by setting it to null
    public void OnTriggerExit(Collider other)
    {
        if (!collidingObject)
        {
            return;
        }

        collidingObject = null;
    }

    private void SelectObject()
    {
        // Move the GameObject inside the player's hand and remove it from
        //  the collidingObject variable
        objectInHand = collidingObject;
		print ("Object in hand: " + objectInHand);
        collidingObject = null;
        // Add a new joint that connects the controller to the object
        var joint = AddFixedJoint();
        joint.connectedBody = objectInHand.GetComponent<Rigidbody>();
    }

    // Make a new fixed joint, add it to the controller, and then set it up
    //  so it doesn't break easily.
    private FixedJoint AddFixedJoint()
    {
        FixedJoint fx = gameObject.AddComponent<FixedJoint>();
        fx.breakForce = 20000;
        fx.breakTorque = 20000;
        return fx;
    }

    // Handles releasing the object
    private void DeselectObject()
    {
        // Check if fixed joint is attached to the controller
        if (GetComponent<FixedJoint>())
        {
            // Remove the connection to the object held by the joint
            GetComponent<FixedJoint>().connectedBody = null;
            Destroy(GetComponent<FixedJoint>());
            // Add the speed and rotation of the controller when the user
            //  releases the object
            objectInHand.GetComponent<Rigidbody>().velocity = Controller.velocity;
            objectInHand.GetComponent<Rigidbody>().angularVelocity = Controller.angularVelocity;
        }
        // Remove the reference to the formerly attached object.
        objectInHand = null;
    }

	// scales object size up
    void ScaleUp(){
		objectInHand.transform.localScale += new Vector3 (.5f, .5f, .5f);
		//objectInHand.GetComponent<BoxCollider>().size += new Vector3 (.1f, .1f, .1f); // can be used to scale the box collieder, removed because couldn't figure the proper scale
	}

    // scales object size down
	void ScaleDown(){
		objectInHand.transform.localScale -= new Vector3 (.5f, .5f, .5f);
		//objectInHand.GetComponent<BoxCollider>().size -= new Vector3 (.1f, .1f, .1f); // can be used to scale the box collieder, removed because couldn't figure the proper scale
	}

    // Update is called once per frame
    void Update()
	{
    	// on grip press, select obejct to manipulate
		if (Controller.GetPressDown(Valve.VR.EVRButtonId.k_EButton_Grip))
		{
			if (collidingObject)
			{
				SelectObject();
			}
		}

		// let go of object when grip is released
		if (Controller.GetPressUp(Valve.VR.EVRButtonId.k_EButton_Grip))
		{
			if (objectInHand)
			{
				DeselectObject();
			}
		}

		// on touch pad click right, scale held object up in size
		if (Controller.GetAxis (Valve.VR.EVRButtonId.k_EButton_Axis0) [0] > 0.8f
		   && Controller.GetPressDown (Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad)) {
			ScaleUp();
		}

		// on touch pad click left, scale held object down in size
		if (Controller.GetAxis (Valve.VR.EVRButtonId.k_EButton_Axis0) [0] < 0.8f
			&& Controller.GetPressDown (Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad)) {
			print ("here2");
			ScaleDown();
		}

		// on trigger press, aim teleport
		if (Controller.GetHairTriggerDown () ) {	//&& canTP

            //canTP = false;
            // creates vectors for moving position
            Vector3 startPos = transform.position;
			Vector3 endPos = startPos + transform.forward * 4f;

			// creates object for cube to mark teleport location
			cube = GameObject.CreatePrimitive (PrimitiveType.Cube);

			// cube's scale and location
			cube.transform.position = new Vector3(endPos[0],.5f,endPos[2]);
			cube.transform.localScale = new Vector3 (.5f, .1f, .5f);

			// removes collision from cube
			Destroy (cube.GetComponent<Rigidbody> ());
			Destroy (cube.GetComponent<BoxCollider> ());

			// sets cubes color
			cube.GetComponent<Renderer> ().material.color = Color.green;
		}

		// on trigger release teleport
		if (Controller.GetHairTriggerUp ()) {
			// move camera, delete cube
			//transform.parent.position = cube.transform.position;
			Destroy (cube);

            if(canTP == true) birdMove();
			
			canTP=true;
		}
    }
}
