using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectGameObjectPooled : MonoBehaviour, IGameObjectPooled
{
    public float moveSpeed = 30f;
    private float lifeTime;
    public float maxLifeTime = 5f;

    private GameObjectPool pool;
    public GameObjectPool Pool
    {
        get { return pool; }
        set
        {
            if (pool == null)
                pool = value;

            else
                throw new System.Exception("Bad pool use, this should only get set once");

        }
    }

    private void OnEnable()
    {
        lifeTime = 0f;
    }
    void Update()
    {
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        lifeTime += Time.deltaTime;
        if (lifeTime > maxLifeTime)
        {
            pool.ReturnToPool(this.gameObject);
        }
    }
}

internal interface IGameObjectPooled
{
    GameObjectPool Pool { get; set; }
}
