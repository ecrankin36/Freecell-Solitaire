using UnityEngine;

public class SolitaireInput : MonoBehaviour
{
    private Solitaire solitaire;
    private GameObject selectedCard = null;
    private bool pointerDownOverCard = false;

    void Start()
    {
        solitaire = FindObjectOfType<Solitaire>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouse = Input.mousePosition;
            mouse.z = Mathf.Abs(Camera.main.transform.position.z);
            Vector3 world = Camera.main.ScreenToWorldPoint(mouse);
            Vector2 world2 = new Vector2(world.x, world.y);

            Collider2D hit = GetTopMostCollider(world2);
            if (hit == null) return;

            if (hit.CompareTag("Card"))
            {
                GameObject clicked = hit.gameObject;

                if (selectedCard == clicked)
                {
                    DeselectCurrent();
                    return;
                }

                CardSprite cs = clicked.GetComponent<CardSprite>();
                if (cs == null) return;

                if (!cs.isFaceUp)
                {
                    cs.isFaceUp = true;
                    return;
                }

                Collider2D cardCol = clicked.GetComponent<Collider2D>();
                if (!cs.isTop || cardCol == null || !cardCol.enabled)
                    return;

                SelectCard(clicked, world);
                pointerDownOverCard = true;
                return;
            }

            if (hit.CompareTag("Tableau") || hit.CompareTag("Foundation") || hit.CompareTag("Freecell"))
            {
                if (selectedCard != null)
                {
                    TryPlaceOn(hit.gameObject);
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (selectedCard != null)
            {
                Vector3 mouse = Input.mousePosition;
                mouse.z = Mathf.Abs(Camera.main.transform.position.z);
                Vector3 world = Camera.main.ScreenToWorldPoint(mouse);
                Vector2 world2 = new Vector2(world.x, world.y);

                Collider2D hit = GetTopMostCollider(world2);

                if (hit != null && (hit.CompareTag("Tableau") || hit.CompareTag("Foundation") || hit.CompareTag("Card") || hit.CompareTag("Freecell")))
                {
                    TryPlaceOn(hit.gameObject);
                }
                else
                {
                    selectedCard.GetComponent<CardSprite>().ReturnToOriginalPosition();
                    DeselectCurrent();
                }
            }

            pointerDownOverCard = false;
        }
    }

    private void SelectCard(GameObject card, Vector3 worldMouse)
    {
        if (selectedCard != null) DeselectCurrent();

        selectedCard = card;

        var sr = selectedCard.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = Color.gray;

        Vector3 worldMouse3 = Camera.main.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y,
            Mathf.Abs(Camera.main.transform.position.z - selectedCard.transform.position.z))
        );
        worldMouse3.z = selectedCard.transform.position.z;
        Vector3 offset = selectedCard.transform.position - worldMouse3;

        var cs = selectedCard.GetComponent<CardSprite>();
        if (cs != null) cs.StartDragging(offset);

        var col = selectedCard.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }

    private void DeselectCurrent()
    {
        if (selectedCard == null) return;

        var sr = selectedCard.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = Color.white;

        var cs = selectedCard.GetComponent<CardSprite>();
        if (cs != null) cs.StopDragging();

        var col = selectedCard.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        selectedCard = null;
    }

    private void TryPlaceOn(GameObject hitObject)
    {
        if (selectedCard == null) return;

        GameObject target = hitObject;

        if (solitaire.IsValidMove(selectedCard, target))
        {
            var cs = selectedCard.GetComponent<CardSprite>();
            if (cs != null) cs.StopDragging();

            var col = selectedCard.GetComponent<Collider2D>();
            if (col != null) col.enabled = true;

            solitaire.PlaceCard(selectedCard, target);
            solitaire.UpdateTopCards();

            var sr = selectedCard.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = Color.white;

            selectedCard = null;
        }
        else
        {
            selectedCard.GetComponent<CardSprite>().ReturnToOriginalPosition();
            DeselectCurrent();
        }
    }

    private Collider2D GetTopMostCollider(Vector2 point)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(point);
        if (hits == null || hits.Length == 0)
            return null;

        foreach (var h in hits)
        {
            if (h == null) continue;
            if (h.CompareTag("Card") && solitaire != null && solitaire.IsTopCard(h.gameObject))
                return h;
        }

        Collider2D best = null;
        int bestIndex = int.MinValue;
        foreach (var h in hits)
        {
            if (h == null) continue;
            int idx = h.transform.GetSiblingIndex();
            if (idx > bestIndex)
            {
                bestIndex = idx;
                best = h;
            }
        }
        return best;
    }
}
