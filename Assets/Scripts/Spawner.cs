using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject floorType, pointType;
    public int floorCount, pointCount;
    public float rareSpawnChance, commonSpawnChance;

    private GameObject[] floors, points;
    private int floorIndex = 0, pointIndex = 0;
    private int x = 0, z = 0;
    // Start is called before the first frame update
    void Start()
    {
        floors = new GameObject[floorCount];
        points = new GameObject[pointCount];

        for (int i = 0; i < floorCount; i++)
        {
            var floor = Instantiate(floorType, position: new(0, -4, 0), new());
            floors[i] = floor;
            if (i == 0)
            {
                floor.transform.position = Vector3.zero;
                continue;
            }
            else if (i < 5)
            {
                z += 2;
                floor.transform.position = new(x, 0, z);
                continue;
            }
            MoveFloor(floors[i]);
        }

        for (int i = 0; i < pointCount; i++)
        {
            var point = Instantiate(pointType, position: new(0, -4, 0), new());
            point.GetComponent<Point>().enabled = true;
            points[i] = point;
        }
        StartCoroutine(MoveFloorAndPoint());
    }

    private IEnumerator MoveFloorAndPoint()
    {
        while(true)
        {
            GameObject curFloor = floors[floorIndex], curPoint = points[pointIndex];
            yield return new WaitUntil(() => AnyDead(floors, floorIndex));
            MoveFloor(curFloor);
            if (AnyDead(points, pointIndex) && floorIndex % 3 == 0) PointSetup(curPoint);
            floorIndex = (floorIndex + 1) % floorCount;
        }
    }

    private bool AnyDead(GameObject[] gameObjects, int startIndex)
    {
        for (int index = 0; index < gameObjects.Length; index++)
        {
            if (gameObjects[(index + startIndex) % gameObjects.Length].TryGetComponent(out Piller piller))
            {
                if (piller.isDead)
                {
                    return true;
                }
            }
            else if (gameObjects[(index + startIndex) % gameObjects.Length].GetComponent<Point>().IsDead)
            {
                return true;
            }
        }
        return false;
    }

    private void PointSetup(GameObject curPoint)
    {
        var rand = Random.value;
        if (rand > 1 - rareSpawnChance)
            MoveAndSetPoint(curPoint, true);
        else if (rand > 1 - commonSpawnChance)
            MoveAndSetPoint(curPoint, false);
    }

    void MoveAndSetPoint(GameObject point, bool rare)
    {
        Debug.LogFormat("Spawned {0} point", rare ? "rare" : "common");
        point.transform.position = new(x, 2.75f, z);
        point.GetComponent<Point>().Setup(rare);
        pointIndex = (pointIndex + 1) % pointCount;
    }

    void MoveFloor(GameObject floor)
    {
        if (Random.value < 0.5) x += 2;
        else z += 2;
        floor.GetComponent<Piller>().isDead = false;
        floor.transform.position = new(x, 0, z);
    }
}
