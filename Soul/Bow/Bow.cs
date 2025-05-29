using UnityEngine;

public class Bow : MonoBehaviour
{
    [SerializeField] GameObject arrowPrefab;
    [SerializeField] GameObject arrowSpawnPoint;

    [SerializeField] GameObject aimPoint;
    [SerializeField] GameObject TopPoint;
    [SerializeField] GameObject MiddlePoint;
    [SerializeField] GameObject BottomPoint;
    [SerializeField] GameObject UpCylinder;
    [SerializeField] GameObject DownCylinder;

    [SerializeField] GameObject handPosition;

    public GameObject root;
    
    public bool isShooting = false;

    GameObject arrow;
    Vector3 forwardDirection;

    void Start()
    {
        UpdateCylinder();
    }

    void Update()
    {
        if (isShooting)
        {
            UpdateCylinder();
            ArrowUpdate();
        }
    }

    public void AttackStart(GameObject handPosition, Vector3 forwardDirection)
    {
        arrow = Instantiate(arrowPrefab, arrowSpawnPoint.transform.position, arrowSpawnPoint.transform.rotation);
        arrow.transform.parent = handPosition.transform;
        arrow.SetActive(true);
        isShooting = true;
        this.handPosition = handPosition;
        this.forwardDirection = forwardDirection;
    }

    public void Shoot(float pitch)
    {
        if (arrow != null)
        {
            arrow.transform.parent = null;
            //forwardDirection을 pitch값으로 회전시킨다.
            // forwardDirection = Quaternion.Euler(pitch, 0, 0) * forwardDirection;
            forwardDirection = Quaternion.AngleAxis(pitch, transform.right) * forwardDirection;
            arrow.GetComponent<Arrow>().Shoot(forwardDirection, root);
            arrow = null;
        }
        isShooting = false;
        handPosition = MiddlePoint;
        UpdateCylinder();
    }

    public void EnemyShoot(Vector3 targetPosition)
    {
        if (arrow != null)
        {
            arrow.transform.parent = null;
            Vector3 shootDirection = targetPosition - arrow.transform.position;
            shootDirection.Normalize();
            arrow.GetComponent<Arrow>().Shoot(shootDirection, root);
            arrow = null;
        }
        isShooting = false;
        handPosition = MiddlePoint;
        UpdateCylinder();
    }

    public void UpdateCylinder()
    {
        //up실린더의 경우 toppoint과 handPosition 사이의 거리로 길이를 조정하고 그 사이에 위치시킨다.
        float upDistance = Vector3.Distance(TopPoint.transform.position, handPosition.transform.position);
        UpCylinder.transform.localScale = new Vector3(UpCylinder.transform.localScale.x, upDistance/2, UpCylinder.transform.localScale.z);
        UpCylinder.transform.position = Vector3.Lerp(TopPoint.transform.position, handPosition.transform.position, 0.5f);
        Vector3 upDirection = (handPosition.transform.position - TopPoint.transform.position).normalized;
        UpCylinder.transform.up = upDirection;
        
        
        //down실린더의 경우 bottompoint과 handPosition 사이의 거리로 길이를 조정하고 그 사이에 위치시킨다.
        float downDistance = Vector3.Distance(BottomPoint.transform.position, handPosition.transform.position);
        DownCylinder.transform.localScale = new Vector3(DownCylinder.transform.localScale.x, downDistance/2, DownCylinder.transform.localScale.z);
        DownCylinder.transform.position = Vector3.Lerp(BottomPoint.transform.position, handPosition.transform.position, 0.5f);
        Vector3 downDirection = (handPosition.transform.position - BottomPoint.transform.position).normalized;
        DownCylinder.transform.up = downDirection;
    }

    public void ArrowUpdate()
    {
        if (arrow != null)
        {
            arrow.transform.position = handPosition.transform.position;
            arrow.transform.forward = aimPoint.transform.position - handPosition.transform.position;
        }
    }

    public void AttackStop()
    {
        isShooting = false;
        handPosition = MiddlePoint;
        UpdateCylinder();
        Destroy(arrow);
        arrow = null;
    }
}
