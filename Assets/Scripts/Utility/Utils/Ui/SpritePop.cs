using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpritePop : MonoBehaviour
{
    SpriteRenderer sr;
    Color c;

    public void Setup(Sprite s, Color c, Vector2 p, int sortingOrder, float time, Vector2 dir, float speed)
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = s;
        this.c = c;
        sr.color = c;
        transform.position = p;
        sr.sortingOrder = sortingOrder;
        this.time = time;
        this.dir = dir;
        this.speed = speed;
    }
    float speed;
    float tp = 0;
    float time;
    Vector2 dir;
    private void Update()
    {
        tp += Time.deltaTime;
        if(tp >= time)
        {
            Destroy(gameObject);
            return;
        }

        transform.Translate(dir * Time.deltaTime * speed);
        c.a = 1 - (tp / time);
        sr.color = c;

    }

}
