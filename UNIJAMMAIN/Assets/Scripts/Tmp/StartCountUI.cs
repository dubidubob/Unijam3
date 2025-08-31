using System.Collections.Generic;
using UnityEngine;

public class StartCountUI : MonoBehaviour
{
    [SerializeField] List<Sprite> sprites = new List<Sprite>();

    private SpriteRenderer sp;
    private bool running = false;
    void OnEnable()
    {
        sp = GetComponent<SpriteRenderer>();

        BeatClock.OnBeat -= Show123Start;
        BeatClock.OnBeat += Show123Start;
    }

    private void OnDisable()
    {
        BeatClock.OnBeat -= Show123Start;
    }

    public void Start123()
    {
        if (cnt != 0) return;
        running = true;
    }

    
    int cnt = 0;
    private void Show123Start(double _, long __)
    {
        if (!running) return;
        if (sprites.Count <= cnt)
        {
            running = false;
            this.enabled = false;
            this.gameObject.SetActive(false);
            return;
        }

        sp.sprite = sprites[cnt++];
    }
}
