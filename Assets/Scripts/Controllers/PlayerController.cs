using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Mail;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    #region Camera & Controller References

    private Camera playerCamera;
    public bool isMoving = false;

    #endregion Camera & Controller References
    #region Mech Components: Assembly & Configuration

    //Assembler handling
    public bool isAssembling = false;
    public Assembler assemblerRef;
    private AssemblyButton assButtonCurr;
    private UnityEngine.Color emissStorage = UnityEngine.Color.yellow;

    private Animator legAnim;

    public GameObject[] legsPool;
    public GameObject[] chassisPool;

    private int currLegID = 0;
    private int currChassisID = 0;

    public GameObject defaultLegs;
    public GameObject defaultChassis;
    public GameObject[] defaultAttachments
        = new GameObject[] { null, null, null, null };

    private GameObject currLegsForConfig;
    private GameObject currChassisForConfig;

    [SerializeField]
    private GameObject legs;
    [SerializeField]
    private GameObject chassis;
    [SerializeField]
    private GameObject[] attachments
        = new GameObject[] { null, null, null, null };

    [HideInInspector]
    public Legs legDATA;
    [HideInInspector]
    public Chassis chassisDATA;
    [HideInInspector]
    public Attachment[] attachmentsDATA
        = new Attachment[] { null, null, null, null };

    #endregion Mech Components: Assembly & Configuration
    #region Player Stats

    public float[] playerStats = new float[] 
        {
            20f, //mvmt
            2f, //sprnt (acts as coefficient on mvmt)
            100f, //max hp
            100f, //max shld
            100f, //max nrgy
            0f, //dmg
            0f, //rof
            0f, //crtc
            0f, //crtd (acts as coefficient on dmg)
            0f  //cd
        };

    public float curhp;
    public float curshld;
    public float curnrgy;

    public Dictionary<string, int> statDict = new Dictionary<string, int>()
            {
                {"mvmt", 0 },
                {"sprnt", 1 },
                {"hp", 2 },
                {"shld", 3 },
                {"nrgy", 4 },
                {"dmg", 5 },
                {"rof", 6 },
                {"crtc", 7 },
                {"crtd", 8 },
                {"cd", 9 }
            };

    public List<Item> items = new List<Item>();

    #endregion Player Stats

    void Awake()
    {
        legsPool = Resources.LoadAll<GameObject>("Legs");
        chassisPool = Resources.LoadAll<GameObject>("Chassis");

        currLegsForConfig = (defaultLegs == null) ? legsPool[currLegID] : defaultLegs;
        currChassisForConfig = (defaultChassis == null) ? chassisPool[currChassisID] : defaultChassis;

        if (legs == null) { MechAssembler(currLegsForConfig, currChassisForConfig, defaultAttachments); }
        if (legAnim == null) { legAnim = legs.GetComponentInChildren<Animator>(); }
        playerCamera = Camera.main;

        curhp = playerStats[statDict["hp"]];
        curshld = playerStats[statDict["shld"]];
        curnrgy = playerStats[statDict["nrgy"]];
    }

    void Update()
    {
        if(!isAssembling)
        {
            MovementInputs();
            AttachmentInputs();
        }

        else
        {
            legAnim.SetBool("Moving", false);
            AssemblyInputs();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Item item = other.GetComponent<Item>();
        if (item != null)
        {
            foreach (KeyValuePair<string, float> stat in item.itemData)
            {
                playerStats[statDict[stat.Key]] += stat.Value;
            }

            Destroy(other.gameObject);
            Debug.Log(other.name + " picked up.");
        }

        Assembler assembler = other.GetComponent<Assembler>();
        if (assembler != null)
        {
            assemblerRef = assembler;

            assembler.ChangeStateAssembling(true);
            EnterAssembly(assembler);
        }
    }

    #region Input Methods

    private void MovementInputs()
    {
        //Movement Inputs only handles the WASD movement and player aiming, separated out
        //to make it readable against component inputs (i.e. activated ability inputs) or
        //inputs related to UI interaction.
        Vector3 pos = transform.position;
        float speed = (Input.GetKey(KeyCode.LeftShift)) ? playerStats[statDict["mvmt"]] * playerStats[statDict["sprnt"]] : playerStats[statDict["mvmt"]];

        bool moving = false;
        if (Input.GetKey(KeyCode.W))
        {
            transform.position = transform.position + Vector3.forward * speed * Time.smoothDeltaTime;
            moving = true;
            legAnim.SetBool("Moving", moving);
        }
        
        if (Input.GetKey(KeyCode.A))
        {
            transform.position = transform.position - Vector3.right * speed * Time.smoothDeltaTime;
            moving = true;
            legAnim.SetBool("Moving", true);
        }

        if (Input.GetKey(KeyCode.S))
        {
            transform.position = transform.position - Vector3.forward * speed * Time.smoothDeltaTime;
            moving = true;
            legAnim.SetBool("Moving", true);
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.position = transform.position + Vector3.right * speed * Time.smoothDeltaTime;
            moving = true;
            legAnim.SetBool("Moving", true);
        }

        legAnim.SetBool("Moving", moving);

        //Uses a normalized vector to rotate legs to face the direction they're moving.
        Vector3 direction = (transform.position - pos).normalized;
        if (direction != Vector3.zero)
        {
            try { legs.transform.rotation = Quaternion.LookRotation(direction); }
            catch { }
        }

        //Uses raycasting to rotate player chassis towards mouse point direction, the right stick
        //in a 'twin stick' shooter.
        RaycastHit hit;
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            try { chassis.transform.LookAt(hit.point); }
            catch { }
        }
    }
    private void AttachmentInputs()
    {
        //Mouse click LMB should always be a weapon, but RMB may not be a weapon. So
        //we use nested try / catch to activate each potential attachment type in an
        //order that prevents excess 'tries' that would be doomed to fail.
        if (Input.GetMouseButton(0))
        {
            try
            {
                ((Weapon)attachmentsDATA[0]).Fire();
                try { ((Weapon)attachmentsDATA[2]).Fire(); }
                catch { }
            } //Only fires weapon [2] if [0] can fire.
            catch { }
        }

        if (Input.GetMouseButton(1))
        {
            try
            {
                ((Weapon)attachmentsDATA[1]).Fire();
                try { ((Weapon)attachmentsDATA[3]).Fire(); }
                catch { }
            } //Only fires weapon [3] if [1] can fire.
            catch
            {
                try { ((Auxilliary)attachmentsDATA[1]).Activate(); }
                catch { }
            }
        }

        if (Input.GetKey(KeyCode.Q))
        {
            try { ((Auxilliary)attachmentsDATA[2]).Activate(); }
            catch { }
        }

        if (Input.GetKey(KeyCode.E))
        {
            try { ((Auxilliary)attachmentsDATA[3]).Activate(); }
            catch { }
        }
    }
    private void AssemblyInputs()
    {
        if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Escape))
        {
            ExitAssembly();
        }

        RaycastHit hit;
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            if (hit.collider.gameObject.GetComponent<AssemblyButton>() != null)
            {
                //Just handles the UI highlighting
                if (assButtonCurr != null)
                {
                    assButtonCurr.GetComponent<Renderer>().material.SetColor("_EmissionColor", emissStorage);
                    assButtonCurr = null;
                    emissStorage = UnityEngine.Color.yellow;
                }

                assButtonCurr = hit.collider.gameObject.GetComponent<AssemblyButton>();
                emissStorage = (emissStorage == UnityEngine.Color.yellow) ? assButtonCurr.GetComponent<Renderer>().material.GetColor("_EmissionColor") : emissStorage;
                assButtonCurr.GetComponent<Renderer>().material.SetColor("_EmissionColor", UnityEngine.Color.red);

                //Component cycling controls
                if(Input.GetMouseButtonDown(0))
                {
                    if (assButtonCurr.legs)
                    {
                        currLegID = (assButtonCurr.left) ? currLegID -1 : currLegID +1;

                        currLegsForConfig = legsPool[ArrayClamper(currLegID, legsPool)];
                        MechAssembler(currLegsForConfig, currChassisForConfig, defaultAttachments);
                    }

                    else
                    {
                        currChassisID = (assButtonCurr.left) ? currChassisID -1 : currChassisID + 1;

                        currChassisForConfig = chassisPool[ArrayClamper(currChassisID, chassisPool)];
                        MechAssembler(currLegsForConfig, currChassisForConfig, defaultAttachments);
                    }
                }
            }

            else if (assButtonCurr != null)
            {
                //Cleanup UI highlighting
                assButtonCurr.GetComponent<Renderer>().material.SetColor("_EmissionColor", emissStorage);
                assButtonCurr = null;
                emissStorage = UnityEngine.Color.yellow;
            }

        }
    }

    #endregion Input Methods
    #region Mech Building & Component Methods

    public void MechClear()
    {
        //Exists because Unity was giving me some weird error if I Destroy()'d components
        //Inside the MechAssembler, didn't impact runtime but this should clear it up.
        Debug.Log("Destroying Mech Configuration.");

        if (legs != null) { Destroy(legs); legDATA = null; }
        if (chassis != null) { Destroy(chassis); chassisDATA = null; }
        if (attachments[0] != null) { Destroy(attachments[0]); attachmentsDATA[0] = null; }
        if (attachments[1] != null) { Destroy(attachments[1]); attachmentsDATA[1] = null; }
        if (attachments[2] != null) { Destroy(attachments[2]); attachmentsDATA[2] = null; }
        if (attachments[3] != null) { Destroy(attachments[3]); attachmentsDATA[3] = null; }

    }

    public void MechClear(int attachment)
    {
        Debug.Log("Removing Attachment Number " + (attachment + 1).ToString());

        if (attachments[attachment] != null) 
        { 
            Destroy(attachments[attachment]); 
            attachmentsDATA[attachment] = null; 
        }
    }

    public void MechAssembler()
    {
        MechAssembler(legs, chassis, attachments);
        Debug.Log("Rebuilt Mech Configuration: No Change");
    }

    public void MechAssembler(GameObject mechComponent)
    {
        if (mechComponent.GetComponent<Attachment>() != null)
        {
            //Attachments
            int point = 0;
            while (attachments[point] != null && point < attachments.Length) { point++; }

            if (attachments[point] != null)
            {
                Destroy(attachments[point]);
                attachmentsDATA[point] = null;
            }

            attachments[point] = Instantiate(mechComponent, chassisDATA.attachmentPoints[point]);
            attachmentsDATA[point] = attachments[point].GetComponent<Attachment>();

            Debug.Log("Added " + mechComponent.name + " at position " + point.ToString() + " to mech.");
        }

        else if (mechComponent.GetComponent<Chassis>())
        {
            //Chassis
            if (chassis != null)
            {
                Destroy(chassis);
                chassisDATA = null;
            }

            chassis = Instantiate(mechComponent, legDATA.chassisPoint);
            chassisDATA = chassis.GetComponent<Chassis>();

            for (int i = 0; i < attachments.Length; i++)
            {
                attachments[i].transform.SetParent(chassisDATA.attachmentPoints[i]);
                attachments[i].transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }
        }
    }

    public void MechAssembler(GameObject attachment, int point)
    {
        if (attachments[point] != null)
        {
            Destroy(attachments[point]);
            attachmentsDATA[point] = null;
        }

        attachments[point] = Instantiate(attachment, chassisDATA.attachmentPoints[point]);
        attachmentsDATA[point] = attachments[point].GetComponent<Attachment>();

        Debug.Log("Added " + attachment.name + " at position " + point.ToString() + " to mech.");
    }

    public void MechAssembler(GameObject newLegs, GameObject newChassis, GameObject[] newAttachments)
    {
        MechClear();

        legs = Instantiate(newLegs, gameObject.transform);
        legDATA = legs.GetComponent<Legs>();
        legAnim = legs.GetComponentInChildren<Animator>();

        chassis = Instantiate(newChassis, legDATA.chassisPoint);
        chassisDATA = chassis.GetComponent<Chassis>();

        for (int i = 0; i < attachments.Length; i++)
        {
            attachments[i] = Instantiate(newAttachments[i], chassisDATA.attachmentPoints[i]);
            attachmentsDATA[i] = attachments[i].GetComponent<Attachment>();
        }

        Debug.Log("Built Mech at " + Time.time.ToString() + " with components:"
            + "\n"
            + "\n\t\tLegs: \t\t" + legDATA.GetType().ToString()
            + "\n\t\tChassis: \t\t" + chassisDATA.GetType().ToString()
            + "\n\t\tAttachment 1: \t" + attachmentsDATA[0].GetType().ToString()
            + "\n\t\tAttachment 2: \t" + attachmentsDATA[1].GetType().ToString()
            + "\n\t\tAttachment 3: \t" + attachmentsDATA[2].GetType().ToString()
            + "\n\t\tAttachment 4: \t" + attachmentsDATA[3].GetType().ToString()
            + "\n");
    }

    public void EnterAssembly(Assembler assembler)
    {
        isAssembling = true;
        transform.position = assembler.snapPoint.transform.position;
        Debug.Log("gg");

        Vector3 dirLook = transform.position - assembler.snapView.transform.position;
        legs.transform.rotation = Quaternion.LookRotation(dirLook);
        chassis.transform.rotation = Quaternion.LookRotation(dirLook);
    }

    public void ExitAssembly()
    {
        assemblerRef.ChangeStateAssembling(false);
        assemblerRef = null;
        isAssembling = false;
    }

    #endregion Mech Building & Component Methods

    int ArrayClamper(int index, GameObject[] arr)
    {
        if (index >= arr.Length) 
        { 
            return index % arr.Length; 
        }

        else if (index < 0)
        {
            return ArrayClamper(arr.Length - index, arr);
        }

        else
        {
            return index;
        }
    }
}
