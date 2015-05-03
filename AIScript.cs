using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIScript : MonoBehaviour
{
    float sharkLength;

    public Rigidbody shark;
    public Transform patrolArea;
    public Transform indicator; 
    public Transform player;

    //shark
    private Vector3 currentTargetPosition;
    private float distanceToTargetPosition = 15f;
    public float maxSharkSpeed;
    private float currentSharkSpeed;
    private float standartTurnValue = 2f;

    //raycasts
    private float rayCastLength;

    //stackfix
    private Quaternion stuckRotation;

    // Use this for initialization
    void Start()
    {
        currentSharkSpeed = maxSharkSpeed / 2;
        sharkLength = GetComponent<MeshFilter>().mesh.bounds.extents.z;
        rayCastLength = sharkLength * 3;
        GetTargetPosition();
    }

    void FixedUpdate()
    {

    }

    // Update is called once per frame
    void Update()
    {
        ObstacleAvoidance();
        PlayerDetection();
    }

    void Move()
    {
        shark.velocity = shark.transform.forward * currentSharkSpeed;
    }

    void GetDirection(Vector3 direction, bool directionChanged)
    {
        Vector3 vectorToTarget = (currentTargetPosition - shark.transform.position);
        //create the rotation we need to be in to look at the target
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        //rotate us over time according to speed until we are in the required rotation
        if (directionChanged)
            shark.transform.rotation = Quaternion.Slerp(shark.transform.rotation, lookRotation, Time.deltaTime * 0.5f);
        else
            shark.transform.rotation = Quaternion.Slerp(shark.transform.rotation, lookRotation, Time.deltaTime * Random.Range(standartTurnValue / 2, standartTurnValue));

        if (vectorToTarget.magnitude <= distanceToTargetPosition)
        {
            GetTargetPosition();
        }
    }

    void GetTargetPosition()
    {
        Vector3 rndPosWithinPatrolArea;
        rndPosWithinPatrolArea = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        rndPosWithinPatrolArea = patrolArea.TransformPoint(rndPosWithinPatrolArea * 0.5f);
        Vector3 direction = (rndPosWithinPatrolArea - shark.transform.position);

        if (direction.magnitude > 50)
        {
            currentTargetPosition = rndPosWithinPatrolArea;
            indicator.position = currentTargetPosition;
        }
        else
        {
            GetTargetPosition();
        }
    }

    void ObstacleAvoidance()
    {
        bool directionChanged = false;

        Vector3 direction = (currentTargetPosition - shark.transform.position);
        Vector3 correctedDirection = direction;

        Vector3 pos = shark.transform.position;

        RaycastHit Fhit;

        RaycastHit Uhit;
        RaycastHit Dhit;
        RaycastHit Rhit;
        RaycastHit Lhit;

        Vector3 Right = Quaternion.AngleAxis(30f, shark.transform.up) * shark.transform.forward;
        Vector3 Left = Quaternion.AngleAxis(-30f, shark.transform.up) * shark.transform.forward;
        Vector3 Up = Quaternion.AngleAxis(30f, -shark.transform.right) * shark.transform.forward;
        Vector3 Down = Quaternion.AngleAxis(-30f, -shark.transform.right) * shark.transform.forward;

        float Fdistance = 9000f;
        float Udistance = 9000f;
        float Ddistance = 9000f;
        float Rdistance = 9000f;
        float Ldistance = 9000f;

        //Raycast Forward
        bool RCforward = Physics.Raycast(pos, shark.transform.forward, out Fhit, rayCastLength * 2);

        //Raycast Up
        bool RCup = Physics.Raycast(pos, Up, out Uhit, rayCastLength);
        //Raycast Down
        bool RCdown = Physics.Raycast(pos, Down, out Dhit, rayCastLength);
        //Raycast Right
        bool RCright = Physics.Raycast(pos, Right, out Rhit, rayCastLength);
        //Raycast Left
        bool RCleft = Physics.Raycast(pos, Left, out Lhit, rayCastLength);

        //Display raycasts + check distances 
        if (RCforward)
        {
            Vector3 FVec = Fhit.point - pos;
            Fdistance = FVec.magnitude;
            Debug.DrawLine(pos, Fhit.point, Color.blue);
        }
        if (RCup)
        {
            Vector3 UVec = Uhit.point - pos;
            Udistance = UVec.magnitude;
            Debug.DrawLine(pos, Uhit.point, Color.blue);
        }
        if (RCdown)
        {
            Vector3 DVec = Dhit.point - pos;
            Ddistance = DVec.magnitude;
            Debug.DrawLine(pos, Dhit.point, Color.blue);
        }
        if (RCright)
        {
            Vector3 RVec = Rhit.point - pos;
            Rdistance = RVec.magnitude;
            Debug.DrawLine(pos, Rhit.point, Color.blue);
        }
        if (RCleft)
        {
            Vector3 LVec = Lhit.point - pos;
            Ldistance = LVec.magnitude;
            Debug.DrawLine(pos, Lhit.point, Color.blue);
        }

        bool alert = false;

        //Resolving Forward
        if (RCforward)
        {
            correctedDirection = shark.transform.forward;

            correctedDirection += Fhit.normal * Fdistance / 5f;
            directionChanged = true;
            alert = true;
        }

        //Resolving Vertical
        if ((RCup || RCdown) && !alert)
        {
            if (correctedDirection == direction)
                correctedDirection = shark.transform.forward;

            if (Mathf.Abs(Udistance - Ddistance) > 2f)
            {
                if (Udistance < Ddistance)
                    correctedDirection += new Vector3(0, rayCastLength / -Udistance, 0);
                else if (Ddistance < Udistance)
                    correctedDirection += new Vector3(0, rayCastLength / Ddistance, 0);
            }

            directionChanged = true;
        }

        //Resolving Horizontal
        if ((RCright || RCleft) && !alert)
        {
            if (correctedDirection == direction)
                correctedDirection = shark.transform.forward;

            if (Mathf.Abs(Rdistance - Ldistance) > 2f)
            {
                if (Rdistance < Ldistance)
                    correctedDirection += new Vector3(rayCastLength / -Rdistance, 0, 0);
                else if (Ldistance < Rdistance)
                    correctedDirection += new Vector3(rayCastLength / Ldistance, 0, 0);
            }

            directionChanged = true;
        }

        //Applying
        if (!directionChanged)
        {
            if (currentSharkSpeed < maxSharkSpeed)
                currentSharkSpeed += 0.1f;
        }
        else if (directionChanged && correctedDirection != direction)
        {
            direction = correctedDirection;
            if (currentSharkSpeed > maxSharkSpeed / 3)
                currentSharkSpeed -= 0.1f;
        }
        GetDirection(direction, directionChanged);
        Move();
    }

    void PlayerDetection()
    {
        RaycastHit hit;
        Vector3 delta = player.transform.position - shark.transform.position;
        Vector3 pos = shark.transform.position;

        if (Physics.Raycast(pos, delta, out hit, delta.magnitude))
        {
            if (delta.magnitude > 50f)
            {
                Debug.DrawLine(pos, hit.point, Color.red);
            }
            else
            {
                if ((player.transform.position - hit.point).magnitude > 5f)
                    Debug.DrawLine(pos, hit.point, Color.red);
                else
                    Debug.DrawLine(pos, hit.point, Color.green);

                if ((player.transform.position - hit.point).magnitude < 5f)
                    Debug.Log("detected");
            }
        }
    }
}
