using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool : MonoBehaviour
{
    [SerializeField]
    private GameObject prefab;

    public static GameObjectPool Instance { get; private set; }
    private Queue<GameObject> objects = new Queue<GameObject>();

    public Vector3 spawnerPosition { get; set; }

    private void Awake()
    {
        Instance = this;
    }

    public GameObject Get()
    {
        if (objects.Count == 0)
        {
            AddObjects(1);
        }

        return objects.Dequeue();
    }

    /*private PooledBall AddBall()
    {
        var ball = Instantiate(prefab);
        return ball;
    }

    public void Return(PooledBall ball)
    {
        ballsAvailable.Enqueue(ball);
    }*/

    public void ReturnToPool(GameObject objectToReturn)
    {
        objectToReturn.SetActive(false);
        if(spawnerPosition != null)
            objectToReturn.transform.position = spawnerPosition;
        objects.Enqueue(objectToReturn);
    }

    private void AddObjects(int count)
    {
        var newObject = GameObject.Instantiate(prefab);
        newObject.SetActive(false);
        objects.Enqueue(newObject);

        #region
        newObject.GetComponent<IGameObjectPooled>().Pool = this;
        #endregion
    }
}