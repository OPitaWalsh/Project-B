using System.Collections;
using System.Threading;
using UnityEngine;

public class EDeathEffects : MonoBehaviour
{
    public void Play()
    {
        GetComponent<AudioSource>().Play();
        GetComponent<ParticleSystem>().Play();
    }


    public void PlayThenDestroy()
    {
        Play();
        StartCoroutine(Timer());
    }


    private IEnumerator Timer()
    {
        yield return new WaitForSeconds(1.0f);
        Destroy(gameObject);
    }
}
