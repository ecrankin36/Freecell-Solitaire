using System.Collections;
using UnityEngine;

public class CardSprite : MonoBehaviour
{
    public Sprite cardFace, cardBack;
    public bool isFaceUp = true;
    public bool isTop = false;

    private SpriteRenderer spriteRenderer;
    private Collider2D myCollider;
    private bool isBeingDragged = false;
    private Vector3 dragOffset = Vector3.zero;
    [HideInInspector] public Vector3 originalPosition;
    private Coroutine returnCoroutine;

    public string cardName;
    public int value;
    public char suit;

    private Animator anim;
    private bool isHovering = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
        myCollider = GetComponent<Collider2D>();

        anim = GetComponent<Animator>();
        if (anim == null)
            anim = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        if (spriteRenderer != null)
            spriteRenderer.sprite = cardFace ?? cardBack;

        if (myCollider == null)
        {
            myCollider = gameObject.AddComponent<BoxCollider2D>();
            myCollider.enabled = true;
        }

        cardName = transform.name;
        Debug.Log("CardSprite assigned cardName = " + cardName);

        if (!string.IsNullOrEmpty(cardName) && cardName.Length > 1)
        {
            suit = cardName[0];
            string rankPart = cardName.Substring(1).Split('_')[0];

            switch (rankPart)
            {
                case "A": value = 1; break;
                case "J": value = 11; break;
                case "Q": value = 12; break;
                case "K": value = 13; break;
                default: int.TryParse(rankPart, out value); break;
            }
        }
    }

    void Update()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.sprite = isFaceUp ? cardFace : cardBack;

        if (isBeingDragged)
        {
            Vector3 mp = Input.mousePosition;
            mp.z = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
            Vector3 world = Camera.main.ScreenToWorldPoint(mp);
            world.z = transform.position.z;
            transform.position = world + dragOffset;
        }
    }

    void OnMouseDown()
    {
        if (!isTop) return;
        if (myCollider == null || !myCollider.enabled) return;

        originalPosition = transform.position;

        Vector3 mp = Input.mousePosition;
        mp.z = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
        Vector3 world = Camera.main.ScreenToWorldPoint(mp);

        dragOffset = transform.position - world;
        StartDragging(dragOffset);
    }

    public void StartDragging(Vector3 offset)
    {
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }

        if (anim != null)
            anim.SetBool("isHovering", false);

        dragOffset = offset;
        isBeingDragged = true;

        Vector3 p = transform.position;
        p.z = -5f;
        transform.position = p;
    }

    void OnMouseUp()
    {
        isBeingDragged = false;

        if (anim != null)
            anim.SetBool("isHovering", false);
    }

    void OnMouseEnter()
    {
        if (!isTop) return;
        if (isBeingDragged) return;
        if (anim == null) return;

        isHovering = true;
        anim.SetBool("isHovering", true);
    }

    void OnMouseExit()
    {
        if (anim == null) return;

        isHovering = false;
        anim.SetBool("isHovering", false);
    }

    public void StopDragging()
    {
        isBeingDragged = false;

        if (anim != null)
            anim.SetBool("isHovering", false);
    }

    public void ReturnToOriginalPosition(float duration = 0.12f)
    {
        if (returnCoroutine != null)
            StopCoroutine(returnCoroutine);

        returnCoroutine = StartCoroutine(ReturnRoutine(duration));
    }

    private IEnumerator ReturnRoutine(float duration)
    {
        isBeingDragged = false;

        if (anim != null)
            anim.SetBool("isHovering", false);

        float t = 0f;
        Vector3 start = transform.position;

        while (t < duration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(
                start,
                originalPosition,
                Mathf.SmoothStep(0f, 1f, t / duration)
            );
            yield return null;
        }

        transform.position = originalPosition;
        returnCoroutine = null;
    }

    public bool IsOppositeColor(CardSprite other)
    {
        bool selfRed = (suit == 'D' || suit == 'H');
        bool otherRed = (other.suit == 'D' || other.suit == 'H');
        return selfRed != otherRed;
    }

    public void SetTop(bool top)
    {
        isTop = top;

        if (myCollider == null)
            myCollider = GetComponent<Collider2D>();

        if (myCollider != null)
        {
            myCollider.enabled = false;
            myCollider.enabled = top;
        }

        if (!top && anim != null)
            anim.SetBool("isHovering", false);
    }
}
