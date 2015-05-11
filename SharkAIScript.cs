using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SharkAIScript : MonoBehaviour
{
    public Rigidbody shark;
    public GameObject patrolArea;
    public GameObject player;
    private GameObject indicator;
    private PlayerScript playerScript;

    //shark
    public float distanceToTargetPosition;
    public float standartTurnValue;
    public float maxSharkSpeed;
    private Vector3 currentTargetPosition;
    private float currentSharkSpeed;
    private bool playerDetected = false;

    private Vector3 currentDirection;
    private bool directionChanged;
    private float sharkLength;
    private bool sharkIsAboutToAttack = false;

    //raycasts
    private float rayCastLength;

    //memory
    private bool playerDetectedStorage = false;
    private Vector3 playerLastSpot;

    // Use this for initialization
    void Start()
    {
        currentSharkSpeed = maxSharkSpeed / 2f;
        sharkLength = GetComponent<MeshFilter>().mesh.bounds.extents.z;
        rayCastLength = sharkLength * 5f;
        playerScript = FindObjectOfType<PlayerScript>();
        indicator = Instantiate(Resources.Load("TargetPointAreaShark", typeof(GameObject))) as GameObject;
        indicator.transform.localScale += new Vector3(distanceToTargetPosition, distanceToTargetPosition, distanceToTargetPosition);
        GetTargetPosition();
    }

    void FixedUpdate()
    {

    }

    // Update is called once per frame
    void Update()
    {
        PlayerDetection();
        CheckForTarget();
        ObstacleAvoidance();
        GetDirection(currentDirection, directionChanged);
    }

    void PlayerDetection()
    {
        RaycastHit hit;
        Vector3 delta = player.transform.position - shark.transform.position;
        Vector3 pos = shark.transform.position;

        if (Physics.Raycast(pos, delta, out hit, delta.magnitude))
        {
            if (hit.collider.CompareTag("Player"))
            {
                if (delta.magnitude > playerScript.playerVisibilityValue)
                {
                    Debug.DrawLine(pos, hit.point, Color.red);
                    playerDetected = false;
                }
                else
                {
                    Debug.DrawLine(pos, hit.point, Color.green);
                    playerDetected = true;
                    playerDetectedStorage = true;
                    playerLastSpot = player.transform.position;
                }
            }
            else
            {
                Debug.DrawLine(pos, hit.point, Color.red);
                playerDetected = false;
            }
        }
    }

    void CheckForTarget()
    {
        Vector3 vectorToTarget = currentTargetPosition - shark.transform.position;

        if (vectorToTarget.magnitude <= distanceToTargetPosition)
        {
            GetTargetPosition();
        }
    }

    void GetTargetPosition()
    {
        Vector3 rndPosWithinPatrolArea = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        rndPosWithinPatrolArea = patrolArea.transform.TransformPoint(rndPosWithinPatrolArea * 0.5f);
        Vector3 deltaVec = rndPosWithinPatrolArea - shark.transform.position;

        bool notInCollision = TargetPointCollisionTest(rndPosWithinPatrolArea);

        if (deltaVec.magnitude > 50f && notInCollision)
        {
            currentTargetPosition = rndPosWithinPatrolArea;
            indicator.transform.position = currentTargetPosition;
        }
        else
        {
            GetTargetPosition();
        }
    }

    bool TargetPointCollisionTest(Vector3 testPoint)
    {
        Vector3 point;
        Vector3 start = shark.transform.position; // This is defined to be some arbitrary point far away from the collider.
        Vector3 direction = testPoint - start; // This is the direction from start to goal.
        direction.Normalize();
        int iterations = 0; // If we know how many times the raycast has hit faces on its way to the target and back, we can tell through logic whether or not it is inside.
        point = start;

        while (point != testPoint) // Try to reach the point starting from the far off point.  This will pass through faces to reach its objective.
        {
            RaycastHit hit;
            if (Physics.Linecast(point, testPoint, out hit)) // Progressively move the point forward, stopping everytime we see a new plane in the way.
            {
                iterations++;
                point = hit.point + (direction / 100.0f); // Move the Point to hit.point and push it forward just a touch to move it through the skin of the mesh (if you don't push it, it will read that same point indefinately).
            }
            else
            {
                point = testPoint; // If there is no obstruction to our goal, then we can reach it in one step.
            }
        }
        while (point != start) // Try to return to where we came from, this will make sure we see all the back faces too.
        {
            RaycastHit hit;
            if (Physics.Linecast(point, start, out hit))
            {
                iterations++;
                point = hit.point + (-direction / 100.0f);
            }
            else
            {
                point = start;
            }
        }
        if (iterations % 2 == 0)
        {
            return false;
        }
        if (iterations % 2 == 1)
        {
            return true;
        }
        return false;
    }

    void ObstacleAvoidance()
    {
        directionChanged = false;

        Vector3 playerDirection = player.transform.position - shark.transform.position;
        Vector3 targetPointDirection = currentTargetPosition - shark.transform.position;
        Vector3 correctedDirection = targetPointDirection;

        sharkIsAboutToAttack = false;

        if (playerDetected && playerDirection.magnitude > playerScript.playerVisibilityValue / 2f) //player detected far away
        {
            correctedDirection = playerDirection;
        }
        else if (playerDetected && playerDirection.magnitude < playerScript.playerVisibilityValue / 2f) //player detected close to shark
        {
            currentDirection = playerDirection;
            sharkIsAboutToAttack = true;
            directionChanged = false;
            return;
        }
        else if (playerDetectedStorage && !playerDetected) //player has been detected but shark can no longer see him
        {
            currentTargetPosition = playerLastSpot;
            indicator.transform.position = playerLastSpot;
            if ((currentTargetPosition - shark.transform.position).magnitude < distanceToTargetPosition)
            {
                playerDetectedStorage = false;
                GetTargetPosition();
            }
        }

        Vector3 Fpos = shark.transform.position;

        Vector3 FRpos = Fpos + shark.transform.right;
        Vector3 FLpos = Fpos - shark.transform.right;
        Vector3 FUpos = Fpos + shark.transform.up;
        Vector3 FDpos = Fpos - shark.transform.up;

        Vector3 FRUpos = Fpos + (shark.transform.right + shark.transform.up) / 1.5f;
        Vector3 FRDpos = Fpos + (shark.transform.right - shark.transform.up) / 1.5f;
        Vector3 FLUpos = Fpos - (shark.transform.right + shark.transform.up) / 1.5f;
        Vector3 FLDpos = Fpos - (shark.transform.right - shark.transform.up) / 1.5f;

        Vector3 FRight = Quaternion.AngleAxis(5f, shark.transform.up) * shark.transform.forward;
        Vector3 FLeft = Quaternion.AngleAxis(-5f, shark.transform.up) * shark.transform.forward;
        Vector3 FUp = Quaternion.AngleAxis(5f, -shark.transform.right) * shark.transform.forward;
        Vector3 FDown = Quaternion.AngleAxis(-5f, -shark.transform.right) * shark.transform.forward;

        Vector3 FRightUp = Quaternion.AngleAxis(5f, shark.transform.up - shark.transform.right) * shark.transform.forward;
        Vector3 FRightDown = Quaternion.AngleAxis(-5f, -shark.transform.up - shark.transform.right) * shark.transform.forward;
        Vector3 FLeftUp = Quaternion.AngleAxis(5f, -shark.transform.up + shark.transform.right) * shark.transform.forward;
        Vector3 FLeftDown = Quaternion.AngleAxis(-5f, shark.transform.up + shark.transform.right) * shark.transform.forward;

        RaycastHit Fhit;
        RaycastHit FRhit;
        RaycastHit FLhit;
        RaycastHit FUhit;
        RaycastHit FDhit;

        RaycastHit FRUhit;
        RaycastHit FRDhit;
        RaycastHit FLUhit;
        RaycastHit FLDhit;

        float Fdistance = 9000f;

        float FRdistance = 9000f;
        float FLdistance = 9000f;
        float FUdistance = 9000f;
        float FDdistance = 9000f;

        float FRUdistance = 9000f;
        float FRDdistance = 9000f;
        float FLUdistance = 9000f;
        float FLDdistance = 9000f;

        //Raycast Forward
        bool RCforward = Physics.Raycast(Fpos, shark.transform.forward, out Fhit, rayCastLength);

        //Raycast Forward Right
        bool RCforwardRight = Physics.Raycast(FRpos, FRight, out FRhit, rayCastLength / 2f);
        //Raycast Forward Left
        bool RCforwardLeft = Physics.Raycast(FLpos, FLeft, out FLhit, rayCastLength / 2f);
        //Raycast Forward Up
        bool RCforwardUp = Physics.Raycast(FUpos, FUp, out FUhit, rayCastLength / 2f);
        //Raycast Forward Down 
        bool RCforwardDown = Physics.Raycast(FDpos, FDown, out FDhit, rayCastLength / 2f);

        //Raycast Forward Right Up
        bool RCforwardRightUp = Physics.Raycast(FRUpos, FRightUp, out FRUhit, rayCastLength / 2f);
        //Raycast Forward Right Down
        bool RCforwardRightDown = Physics.Raycast(FRDpos, FRightDown, out FRDhit, rayCastLength / 2f);
        //Raycast Forward Left Up
        bool RCforwardLeftUp = Physics.Raycast(FLUpos, FLeftUp, out FLUhit, rayCastLength / 2f);
        //Raycast Forward Left Down
        bool RCforwardLeftDown = Physics.Raycast(FLDpos, FLeftDown, out FLDhit, rayCastLength / 2f);

        //Display raycasts + check distances 
        if (RCforward)
        {
            Vector3 FVec = Fhit.point - Fpos;
            Fdistance = FVec.magnitude;
            Debug.DrawLine(Fpos, Fhit.point, Color.blue);
        }
        if (RCforwardRight)
        {
            Vector3 FVec = FRhit.point - FRpos;
            FRdistance = FVec.magnitude;
            Debug.DrawLine(FRpos, FRhit.point, Color.blue);
        }
        if (RCforwardLeft)
        {
            Vector3 FVec = FLhit.point - FLpos;
            FLdistance = FVec.magnitude;
            Debug.DrawLine(FLpos, FLhit.point, Color.blue);
        }
        if (RCforwardUp)
        {
            Vector3 FVec = FUhit.point - FUpos;
            FUdistance = FVec.magnitude;
            Debug.DrawLine(FUpos, FUhit.point, Color.blue);
        }
        if (RCforwardDown)
        {
            Vector3 FVec = FDhit.point - FDpos;
            FDdistance = FVec.magnitude;
            Debug.DrawLine(FDpos, FDhit.point, Color.blue);
        }
        if (RCforwardRightUp)
        {
            Vector3 FVec = FRUhit.point - FRUpos;
            FRUdistance = FVec.magnitude;
            Debug.DrawLine(FRUpos, FRUhit.point, Color.blue);
        }
        if (RCforwardRightDown)
        {
            Vector3 FVec = FRDhit.point - FRDpos;
            FRDdistance = FVec.magnitude;
            Debug.DrawLine(FRDpos, FRDhit.point, Color.blue);
        }
        if (RCforwardLeftUp)
        {
            Vector3 FVec = FLUhit.point - FLUpos;
            FLUdistance = FVec.magnitude;
            Debug.DrawLine(FLUpos, FLUhit.point, Color.blue);
        }
        if (RCforwardLeftDown)
        {
            Vector3 FVec = FLDhit.point - FLDpos;
            FLDdistance = FVec.magnitude;
            Debug.DrawLine(FLDpos, FLDhit.point, Color.blue);
        }

        //Resolving
        if (RCforward || RCforwardRight || RCforwardLeft || RCforwardUp || RCforwardDown || RCforwardRightUp || RCforwardRightDown || RCforwardLeftUp || RCforwardLeftDown)
        {
            if (correctedDirection == targetPointDirection)
                correctedDirection = shark.transform.forward;

            List<float> distancesList = new List<float>();
            distancesList.Add(Fdistance);
            distancesList.Add(FRdistance);
            distancesList.Add(FLdistance);
            distancesList.Add(FUdistance);
            distancesList.Add(FDdistance);
            distancesList.Add(FRUdistance);
            distancesList.Add(FRDdistance);
            distancesList.Add(FLUdistance);
            distancesList.Add(FLDdistance);

            float lowest = distancesList[0];

            foreach (float dist in distancesList)
                if (dist < lowest) lowest = dist;

            if (RCforward)
            {
                if (Fdistance < rayCastLength / 2f)
                    correctedDirection += Fhit.normal * (rayCastLength / Fdistance);
                else
                    correctedDirection += Fhit.normal;
            }
            else if (lowest == FRdistance)
            {
                correctedDirection += FRhit.normal;
            }
            else if (lowest == FLdistance)
            {
                correctedDirection += FLhit.normal;
            }
            else if (lowest == FUdistance)
            {
                correctedDirection += FUhit.normal;
            }
            else if (lowest == FDdistance)
            {
                correctedDirection += FDhit.normal;
            }
            else if (lowest == FRUdistance)
            {
                correctedDirection += FRUhit.normal;
            }
            else if (lowest == FRDdistance)
            {
                correctedDirection += FRDhit.normal;
            }
            else if (lowest == FLUdistance)
            {
                correctedDirection += FLUhit.normal;
            }
            else if (lowest == FLDdistance)
            {
                correctedDirection += FLDhit.normal;
            }

            directionChanged = true;
        }

        //Applying
        if (!directionChanged)
        {
            if (!playerDetected)
                currentDirection = targetPointDirection;
            else
                currentDirection = playerDirection;
        }
        else if (directionChanged)
        {
            currentDirection = correctedDirection;
        }
    }

    void GetDirection(Vector3 direction, bool directionChanged)
    {
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        if (directionChanged)
        {
            if (sharkIsAboutToAttack)
                shark.transform.rotation = Quaternion.Slerp(shark.transform.rotation, lookRotation, Time.deltaTime * standartTurnValue * 5f);
            else if (playerDetected)
                shark.transform.rotation = Quaternion.Slerp(shark.transform.rotation, lookRotation, Time.deltaTime * standartTurnValue * 3f);
            else if (playerDetectedStorage)
                shark.transform.rotation = Quaternion.Slerp(shark.transform.rotation, lookRotation, Time.deltaTime * standartTurnValue * 3f);
            else
                shark.transform.rotation = Quaternion.Slerp(shark.transform.rotation, lookRotation, Time.deltaTime);
        }
        else if (!directionChanged) {
            if (sharkIsAboutToAttack)
                shark.transform.rotation = Quaternion.Slerp(shark.transform.rotation, lookRotation, Time.deltaTime * standartTurnValue * 10f);
            else if (playerDetected)
                shark.transform.rotation = Quaternion.Slerp(shark.transform.rotation, lookRotation, Time.deltaTime * standartTurnValue * 2f);
            else if (playerDetectedStorage)
                shark.transform.rotation = Quaternion.Slerp(shark.transform.rotation, lookRotation, Time.deltaTime * standartTurnValue * 2f);
            else
                shark.transform.rotation = Quaternion.Slerp(shark.transform.rotation, lookRotation, Time.deltaTime * standartTurnValue);
        }

        Move(directionChanged);
    }

    void Move(bool directionChanged)
    {
        if (directionChanged)
        {
            if (playerDetected)
            {
                if (currentSharkSpeed < maxSharkSpeed * 1.5f)
                    currentSharkSpeed += 0.5f;
                if (currentSharkSpeed > maxSharkSpeed * 1.5f)
                    currentSharkSpeed -= 0.5f;
            }
            else if (playerDetectedStorage)
            {
                if (currentSharkSpeed < maxSharkSpeed)
                    currentSharkSpeed += 0.5f;
                if (currentSharkSpeed > maxSharkSpeed)
                    currentSharkSpeed -= 0.5f;
            }
            else
            {
                if (currentSharkSpeed > maxSharkSpeed / 3f)
                    currentSharkSpeed -= 0.5f;
            }
        }
        else if (!directionChanged)
        {

            if (playerDetected)
            {
                if (currentSharkSpeed < maxSharkSpeed * 2f)
                    currentSharkSpeed += 0.5f;
            }
            else if (playerDetectedStorage)
            {
                if (currentSharkSpeed < maxSharkSpeed * 1.5f)
                    currentSharkSpeed += 0.5f;
                if (currentSharkSpeed > maxSharkSpeed * 1.5f)
                    currentSharkSpeed -= 0.5f;
            }
            else
            {
                if (currentSharkSpeed < maxSharkSpeed)
                    currentSharkSpeed += 0.5f;
                if (currentSharkSpeed > maxSharkSpeed)
                    currentSharkSpeed -= 0.5f;
            }
        }

        shark.velocity = shark.transform.forward * currentSharkSpeed;
    }
}
