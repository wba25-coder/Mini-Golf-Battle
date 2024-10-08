﻿using UnityEngine;

/// <summary>
/// Script which controls the ball
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BallControl : MonoBehaviour
{
    public static BallControl instance;                 

    [SerializeField] private LineRenderer lineRenderer;     //reference to lineRenderer child object
    [SerializeField] private float MaxForce;                //maximum force that an be applied to ball
    [SerializeField] private float forceModifier = 0.5f;    //multipliers of force
    private GameObject areaAffector;       //reference to sprite object which show area around ball to click
    [SerializeField] private LayerMask rayLayer;            //layer allowed to be detected by ray
    [SerializeField] private float MaxLineRange = 2.6f;

    [SerializeField] private Color startColor = Color.green;
    [SerializeField] private Color midColor = Color.yellow;
    [SerializeField] private Color endColor = Color.red;

    private MaterialPropertyBlock propertyBlock;
    private Camera mainCamera;

    private float force;                                    //actuale force which is applied to the ball
    private Rigidbody rgBody;                               //reference to rigidbody attached to this gameobject
    /// <summary>
    /// The below variables are used to decide the force to be applied to the ball
    /// </summary>
    public Vector3 startPos, endPos;
    public bool canShoot = false, ballIsStatic = true;    //bool to make shooting stopping ball easy
    private Vector3 direction;                              //direction in which the ball will be shot
    public bool cameraIsMoving = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        rgBody = GetComponent<Rigidbody>();                 //get reference to the rigidbody
        propertyBlock = new MaterialPropertyBlock();
    }

    void Start()
    {
        GameObject UIAreaAffector = GameObject.FindWithTag("AreaAffector");
        mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();

        if (UIAreaAffector != null)
        {
            areaAffector = UIAreaAffector;
        }
        else
        {
            Debug.LogError("Nenhum game obj foi encontrada na cena com a tag 'AreaAffector'");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (rgBody.velocity == Vector3.zero && !ballIsStatic)   //if velocity is zero and ballIsStatic is false
        {
            ballIsStatic = true;                                //set ballIsStatic to true
            LevelManager.instance.ShotTaken();                  //inform LevelManager of shot taken
            rgBody.angularVelocity = Vector3.zero;              //set angular velocity to zero
            areaAffector.SetActive(true);                       //activate areaAffector
        }
    }

    private void FixedUpdate()
    {
        if (canShoot)                                               //if canSHoot is true
        {
            canShoot = false;                                       //set canShoot to false
            ballIsStatic = false;                                   //set ballIsStatic to false
            direction = startPos - endPos;                          //get the direction between 2 vectors from start to end pos
            rgBody.AddForce(direction * force, ForceMode.Impulse);  //add force to the ball in given direction
            areaAffector.SetActive(false);                          //deactivate areaAffector
            UIManager.instance.PowerBar.fillAmount = 0;             //reset the powerBar to zero
            force = 0;                                              //reset the force to zero
            startPos = endPos = Vector3.zero;                       //reset the vectors to zero
        }
    }

    // Unity native Method to detect colliding objects
    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Destroyer")                              //if the object name is Destroyer
        {
            LevelManager.instance.LevelFailed();                    //Level Failed
        }
        else if (other.name == "Hole")                              //if the object name is Hole
        {
            LevelManager.instance.LevelComplete();                  //Level Complete
        }
    }

    public void MouseDownMethod()                                           //method called on mouse down by InputManager
    {
        if(!ballIsStatic) return;                                           //no mouse detection if ball is moving
        startPos = ClickedPoint();                                          //get the vector in word space
        lineRenderer.gameObject.SetActive(true);                            //activate lineRenderer
        lineRenderer.SetPosition(0, lineRenderer.transform.localPosition);  //set its 1st position
    }

    public void MouseNormalMethod()                                         //method called by InputManager
    {
        if(!ballIsStatic) return;                                           //no mouse detection if ball is moving

        endPos = ClickedPoint();                                             //get the vector in word space
        float distance = Vector3.Distance(endPos, startPos); // Calcular a distância entre startPos e endPos
        Vector3 direction = (endPos - startPos).normalized;  // Normaliza a direção
        if (distance > MaxLineRange)
        {
            endPos = startPos + direction * MaxLineRange;     // Ajusta o endPos para o limite máximo
            distance = MaxLineRange;                          // Atualiza a distância para o limite

        }

        if (Mathf.Abs(Vector3.Dot(direction.normalized, Vector3.forward)) > 0.15f)
        {
            cameraIsMoving = true;
        }
        else
        {
            cameraIsMoving = false;
        }

        force = Mathf.Clamp(distance * forceModifier, 0, MaxForce);   //calculate the force
        UIManager.instance.PowerBar.fillAmount = force / MaxForce;              //set the powerBar image fill amount
        
        // Interpolação de cor baseada na distância atual
        Color currentColor;
        if (distance < MaxLineRange / 2)
        {
            currentColor = Color.Lerp(startColor, midColor, distance / (MaxLineRange / 2));
        }
        else
        {
            currentColor = Color.Lerp(midColor, endColor, (distance - MaxLineRange / 2) / (MaxLineRange / 2));
        }

        // Aplica a cor ao material usando MaterialPropertyBlock
        propertyBlock.SetColor("_Color", currentColor);
        lineRenderer.GetComponent<Renderer>().SetPropertyBlock(propertyBlock);

        //we convert the endPos to local pos for ball as lineRenderer is child of ball
        lineRenderer.SetPosition(1, transform.InverseTransformPoint(endPos));   //set its 1st position
    }

    public void MouseUpMethod()                                             //method called by InputManager
    {
        if(!ballIsStatic) return;                                           //no mouse detection if ball is moving
        canShoot = true;                                                    //set canShoot true
        lineRenderer.gameObject.SetActive(false);                           //deactive lineRenderer
    }

    /// <summary>
    /// Method used to convert the mouse position to the world position in respect to Level
    /// </summary>
    Vector3 ClickedPoint()
    {
        Vector3 position = Vector3.zero;                                //get a new Vector3 varialbe
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);    //create a ray from camera in mouseposition direction
        RaycastHit hit = new RaycastHit();                              //create a RaycastHit
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, rayLayer))    //check for the hit 
        {
            position = hit.point;                                       //save the hit point in position
        }
        return position;                                                //return position
    }

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
    }

#endif

}
