using UnityEngine;
using UnityEngine.UIElements;

public class HumanoidIkController : MonoBehaviour
{
    Animator anim;
    public bool iKActive = false;
    public Transform objTarget, objRifleTarget;
    public float lookWeight;
    public float desireDist = 2.6f;

    GameObject objPivot;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        objPivot = new GameObject("DummyPivot");
        objPivot.transform.parent = transform;
        objPivot.transform.localPosition = new Vector3(0, 1.7f, 0);
    }

    // Update is called once per frame
    void Update()
    {
        objPivot.transform.LookAt(objTarget);
        float pivotRotY = objPivot.transform.localRotation.y;

        float dist = Vector3.Distance(objPivot.transform.position, objTarget.position);

        if (pivotRotY < 0.45f && pivotRotY > -0.45f && dist < desireDist)
        {
            lookWeight = Mathf.Lerp(lookWeight, 1, Time.deltaTime * 2.5f);
        }
        else
        {
            lookWeight = Mathf.Lerp(lookWeight, 0, Time.deltaTime * 2.5f);
        }
    }
    private void OnAnimatorIK()
    {
        if (anim)
        {
            if (iKActive)
            {
                if (objTarget != null)
                {
                    anim.SetLookAtWeight(lookWeight);
                    anim.SetLookAtPosition(objTarget.position);
                }
            }
            else
            {
                anim.SetLookAtWeight(0);
            }
        }
    }
}
