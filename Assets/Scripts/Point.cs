using UnityEngine;

public class Point : MonoBehaviour
{

    private bool rare = false;
    [SerializeField] private AudioClip commonClip, rareClip;
    [SerializeField] private Material standardMat, rareMat;
    [SerializeField] private int rarePoints, standardPoints;

    public bool IsDead { get; private set; } = true;

    public void Setup(bool rare)
    {
        GetComponent<Renderer>().material = (this.rare = rare) ? rareMat : standardMat;
        gameObject.GetComponent<Renderer>().enabled = !(IsDead = false);
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject.Find("GameMaster").GetComponent<GameMaster>().UpdatePoints(rare ? rarePoints : standardPoints);
        var audioSource = GetComponent<AudioSource>();
        audioSource.clip = rare ? rareClip : commonClip;
        audioSource.Play();
        gameObject.GetComponent<Renderer>().enabled = !(IsDead = true);
    }
}
