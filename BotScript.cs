using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotScript : MonoBehaviour
{
    public static int maxHealth = 100;
    public int currentHealth = 100;
    private float timeHidden = 3f;
    public GameObject bot;

    private void Start()
    {
        bot = gameObject.transform.GetChild(0).gameObject;
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            //StartCoroutine(DestroyAfter(3f));

            //photonView.RPC("IsDead", RpcTarget.All);
            //PhotonNetwork.
            //photonView.RPC("HidePlayer", RpcTarget.All, true);
            StartCoroutine(HideThenShow(timeHidden));
        }
    }

    IEnumerator HideThenShow(float time)
    {
        bot.SetActive(false);
        yield return new WaitForSeconds(time);
        currentHealth = maxHealth;
        bot.SetActive(true);
    }
}
