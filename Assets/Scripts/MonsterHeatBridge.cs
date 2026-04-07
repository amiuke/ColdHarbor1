using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MonsterHeatBridge : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private MonsterAI monsterAI;
    [SerializeField] private bool useTrigger = true;
    [SerializeField] private HeatReceiver heatReceiver;

    private int burningObjectsInContact = 0;

    private void Awake()
    {
        // Auto-fetch MonsterAI if not assigned
        if (monsterAI == null)
        {
            monsterAI = GetComponent<MonsterAI>();
        }

        if (monsterAI == null)
        {
            monsterAI = GetComponentInParent<MonsterAI>();
        }

        // Ensure collider is trigger
        Collider col = GetComponent<Collider>();
        if (col != null && useTrigger)
        {
            col.isTrigger = true;
        }
        // �� HeatReceiver ���յ������������ǹ�磩ʱ����ȼ��
        heatReceiver.OnStateChanged.AddListener(CheckHeat);
    }
    private void CheckHeat()
    {
        // ������ǹ������ﵽ��ĳЩ����״̬������ AI ȼ��
        if (heatReceiver.materialType == HeatReceiver.MaterialType.HEATED)
        {
            monsterAI.StartBurn();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (monsterAI == null) return;

        // Check if the overlapping object has a HeatReceiver
        HeatReceiver heatReceiver = other.GetComponent<HeatReceiver>();

        if (heatReceiver != null && heatReceiver.IsOnFire)
        {
            // Object is on fire, start burning
            monsterAI.StartBurn();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (monsterAI == null) return;

        // Check if the overlapping object has a HeatReceiver
        HeatReceiver heatReceiver = other.GetComponent<HeatReceiver>();

        if (heatReceiver != null && heatReceiver.IsOnFire)
        {
            burningObjectsInContact++;
            monsterAI.StartBurn();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (monsterAI == null) return;

        // Check if the exiting object has a HeatReceiver
        HeatReceiver heatReceiver = other.GetComponent<HeatReceiver>();

        if (heatReceiver != null && heatReceiver.IsOnFire)
        {
            burningObjectsInContact--;

            // If no more burning objects in contact, stop burning
            if (burningObjectsInContact <= 0)
            {
                burningObjectsInContact = 0;
                monsterAI.StopBurn();
            }
        }
    }

    // Alternative: Use OnCollisionStay for non-trigger colliders
    private void OnCollisionStay(Collision collision)
    {
        if (monsterAI == null || useTrigger) return;

        HeatReceiver heatReceiver = collision.collider.GetComponent<HeatReceiver>();

        if (heatReceiver != null && heatReceiver.IsOnFire)
        {
            monsterAI.StartBurn();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (monsterAI == null || useTrigger) return;

        HeatReceiver heatReceiver = collision.collider.GetComponent<HeatReceiver>();

        if (heatReceiver != null && heatReceiver.IsOnFire)
        {
            burningObjectsInContact++;
            monsterAI.StartBurn();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (monsterAI == null || useTrigger) return;

        HeatReceiver heatReceiver = collision.collider.GetComponent<HeatReceiver>();

        if (heatReceiver != null && heatReceiver.IsOnFire)
        {
            burningObjectsInContact--;

            if (burningObjectsInContact <= 0)
            {
                burningObjectsInContact = 0;
                monsterAI.StopBurn();
            }
        }
    }
}
