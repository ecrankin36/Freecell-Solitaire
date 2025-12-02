using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class Solitaire : MonoBehaviour
{
    public string[] suits = { "C", "D", "H", "S" };
    public string[] ranks = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
    public Sprite[] cardFaces;
    public Sprite cardBack, emptyPlace;
    public GameObject[] foundationPositions, tableauPositions;
    public GameObject cardPrefab;
    public List<string> deck;
    public List<string>[] foundations, tableaus;
    public List<string> foundation0 = new List<string>();
    public List<string> foundation1 = new List<string>();
    public List<string> foundation2 = new List<string>();
    public List<string> foundation3 = new List<string>();

    // FreeCell UI slots (assign these 4 GameObjects in the Inspector)
    public GameObject[] freecellPositions;

    // internal lists to track freecell contents
    public List<string> freecell0 = new List<string>();
    public List<string> freecell1 = new List<string>();
    public List<string> freecell2 = new List<string>();
    public List<string> freecell3 = new List<string>();
    private List<string>[] freecells; // initialized in Start()

    public List<string> tableau0 = new List<string>();
    public List<string> tableau1 = new List<string>();
    public List<string> tableau2 = new List<string>();
    public List<string> tableau3 = new List<string>();
    public List<string> tableau4 = new List<string>();
    public List<string> tableau5 = new List<string>();
    public List<string> tableau6 = new List<string>();
    public List<string> tableau7 = new List<string>();

    private System.Random rng = new System.Random();
    private Vector3 cardOffset = new Vector3(0f, -.5f, -0.1f);

	public AudioSource audioSource;
	public AudioClip placeCardSFX;
	
	public Text winText;

    private readonly char[] foundationSuitMap = new char[] { 'C', 'H', 'D', 'S' };

    void Awake()
    {
        print("THIS INSTANCE NAME: " + gameObject.name);
        if (tableauPositions == null || tableauPositions.Length == 0)
            Debug.LogError("ERROR: tableauPositions is NOT assigned!");
        if (foundationPositions == null || foundationPositions.Length == 0)
            Debug.LogError("ERROR: foundationPositions is NOT assigned!");
        if (freecellPositions == null || freecellPositions.Length == 0)
            Debug.LogError("ERROR: freecellPositions is NOT assigned!");
    }

    void Start()
    {
        tableaus = new List<string>[] { tableau0, tableau1, tableau2, tableau3, tableau4, tableau5, tableau6, tableau7 };
        freecells = new List<string>[] { freecell0, freecell1, freecell2, freecell3 };
        foundations = new List<string>[] { foundation0, foundation1, foundation2, foundation3 };
		
		if (winText != null) {
			winText.gameObject.SetActive(false);
		}

        PlayGame();
    }

    void PlayGame()
    {
        deck = GenerateDeck();
        Deal();
    }

    List<string> GenerateDeck()
    {
        List<string> newDeck = new List<string>();
        foreach (string suit in suits)
            foreach (string rank in ranks)
                newDeck.Add(suit + rank + "_0");
        // shuffle
        newDeck = newDeck.OrderBy(x => rng.Next()).ToList();
        return newDeck;
    }

    void Deal()
    {
        Debug.Log("Dealing FreeCell layout...");

        // clear tableau lists
        foreach (var t in tableaus)
            t.Clear();

        int deckIndex = 0;

        // First 4 tableaus get 7 cards
        for (int col = 0; col < 4; col++)
        {
            for (int j = 0; j < 7; j++)
            {
                tableaus[col].Add(deck[deckIndex]);
                deckIndex++;
            }
        }

        // Last 4 tableaus get 6 cards
        for (int col = 4; col < 8; col++)
        {
            for (int j = 0; j < 6; j++)
            {
                tableaus[col].Add(deck[deckIndex]);
                deckIndex++;
            }
        }

        deck.RemoveRange(0, deckIndex);

        for (int tab = 0; tab < tableauPositions.Length; tab++)
        {
            Vector3 pos = tableauPositions[tab].transform.position + new Vector3(0, 0, -0.1f);

            foreach (string card in tableaus[tab])
            {
                bool isTopCard = card == tableaus[tab].Last();
                CreateCard(card, pos, tableauPositions[tab].transform, true); // faceUp=true for FreeCell
                pos += cardOffset;
            }
        }
		
		if (audioSource != null && placeCardSFX != null) {
			audioSource.PlayOneShot(placeCardSFX);
		}

        UpdateTopCards();
		CheckWinCondition();

    }

    void CreateCard(string cardName, Vector3 position, Transform parent, bool isFaceUp)
    {
        Debug.Log($"CreateCard: creating '{cardName}' at {position} (isTop={isFaceUp})");

        if (cardPrefab == null)
        {
            Debug.LogError("CreateCard: cardPrefab is null on Solitaire! Assign it in the Inspector.");
            return;
        }

        GameObject newCard = Instantiate(cardPrefab, position, Quaternion.identity, parent);
        newCard.name = cardName ?? "null_card";

        // find face sprite by exact name match
        Sprite cardFace = null;
        if (cardFaces != null && cardFaces.Length > 0)
            cardFace = cardFaces.FirstOrDefault(s => s != null && s.name == cardName);

        if (cardFace == null)
        {
            string sample = (cardFaces == null || cardFaces.Length == 0)
                ? "<no sprites in cardFaces array>"
                : string.Join(", ", cardFaces.Take(Math.Min(5, cardFaces.Length)).Select(s => s == null ? "<null>" : s.name));
            Debug.LogWarning($"CreateCard: NO face sprite found for '{cardName}'. cardFaces sample: {sample}");
        }
        else
        {
            Debug.Log($"CreateCard: matched face sprite '{cardFace.name}' for card '{cardName}'.");
        }

        // assign to CardSprite component (if present)
        CardSprite cs = newCard.GetComponent<CardSprite>();
        if (cs == null)
        {
            Debug.LogWarning($"CreateCard: cardPrefab missing CardSprite component on instance '{newCard.name}'.");
        }
        else
        {
            cs.cardFace = cardFace;
            cs.cardBack = cardBack;
            cs.isFaceUp = isFaceUp;
        }

        SpriteRenderer sr = newCard.GetComponent<SpriteRenderer>();
        if (sr == null) sr = newCard.GetComponentInChildren<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogWarning($"CreateCard: no SpriteRenderer found on card prefab instance '{newCard.name}'.");
        }
        else
        {
            Sprite toShow = isFaceUp ? cardFace : cardBack;
            if (toShow == null)
            {
                toShow = cardFace ?? cardBack;
                Debug.LogWarning($"CreateCard: toShow was null for '{cardName}', falling back to {(toShow == cardFace ? "cardFace" : "cardBack")}");
            }
            sr.sprite = toShow;
        }
        if (newCard.tag != "Card")
        {
            try { newCard.tag = "Card"; } catch { /* ignore if tag doesn't exist in project */ }
        }
        Collider2D col = newCard.GetComponent<Collider2D>();
        if (col == null)
        {
            col = newCard.AddComponent<BoxCollider2D>();
            Debug.Log($"CreateCard: added BoxCollider2D to '{newCard.name}' because none was present.");
        }

        col.enabled = true;
    }


    public bool IsValidMove(GameObject card, GameObject targetObject)
    {
        ResolveTarget(targetObject, out GameObject clickedTag, out int foundationIndex, out int tabIndex);

        CardSprite moving = card.GetComponent<CardSprite>();
        if (moving == null) return false;

        // FREECELL
        if (clickedTag.CompareTag("Freecell"))
        {
            if (clickedTag.transform.childCount > 0)
                return false;

            return IsTopCard(card);
        }

        // FOUNDATION
        if (clickedTag.CompareTag("Foundation"))
        {
			Debug.Log($"FOUNDATION INDEX = {foundationIndex}, EXPECTS SUIT = {foundationSuitMap[foundationIndex]}, MOVING SUIT = {moving.suit}");

            if (foundationIndex < 0)
                foundationIndex = Array.IndexOf(foundationPositions, clickedTag);

            if (foundationIndex < 0 || foundationIndex >= foundationSuitMap.Length)
                return false;

            char requiredSuit = foundationSuitMap[foundationIndex];

            CardSprite top = null;
            if (clickedTag.transform.childCount > 0)
                top = clickedTag.transform.GetChild(clickedTag.transform.childCount - 1).GetComponent<CardSprite>();

            if (top == null)
            {
                return moving.value == 1 && moving.suit == requiredSuit;
            }
            else
            {
                return moving.suit == top.suit && moving.value == top.value + 1;
            }
			Debug.Log($"FOUNDATION INDEX = {foundationIndex}, EXPECTS SUIT = {foundationSuitMap[foundationIndex]}, MOVING SUIT = {moving.suit}");

        }

        // TABLEAU
        if (clickedTag.CompareTag("Tableau"))
        {
            CardSprite top = null;

            if (clickedTag.transform.childCount > 0)
                top = clickedTag.transform.GetChild(clickedTag.transform.childCount - 1).GetComponent<CardSprite>();

            if (top == null)
            {
                // empty tableau: only Kings can be placed
                return moving.value == 13;
            }
            else
            {
                bool oppositeColors = moving.IsOppositeColor(top);
                bool correctValue = moving.value + 1 == top.value;
                return oppositeColors && correctValue;
            }
        }

        return false;
    }

    public void MoveCardsAbove(GameObject origParent, int originalTabIndex, int destTabIndex, int cardsToMoveCount, GameObject clickedTag, GameObject cardObject)
    {
        if (originalTabIndex == -1 || cardsToMoveCount <= 1) return;

        List<string> origTab = tableaus[originalTabIndex];
        int origCount = origTab.Count;

        int origIndex = origCount - cardsToMoveCount;

        for (int i = 0; i < cardsToMoveCount; i++)
        {
            string movingCardName = origTab[origIndex];
            origTab.RemoveAt(origIndex);
            tableaus[destTabIndex].Add(movingCardName);

            // move GameObject
            GameObject movingCardObj = origParent.transform.Find(movingCardName)?.gameObject;
            if (movingCardObj != null)
            {
                movingCardObj.transform.parent = clickedTag.transform;
                movingCardObj.transform.position = cardObject.transform.position + (cardOffset * i);
            }
        }

        if (audioSource != null && placeCardSFX != null) {
			audioSource.PlayOneShot(placeCardSFX);
		}
        UpdateTopCards();
		CheckWinCondition();

    }

    public void PlaceCard(GameObject cardObject, GameObject targetObject)
    {
        if (cardObject == null || targetObject == null || cardObject == targetObject)
            return;

        ResolveTarget(targetObject, out GameObject clickedTag, out int foundationIndex, out int resolvedTab);
        if (clickedTag == null) return;

        for (int i = 0; i < tableaus.Length; i++)
            tableaus[i].Remove(cardObject.name);

        for (int i = 0; i < foundations.Length; i++)
            foundations[i].Remove(cardObject.name);

        if (freecells != null)
        {
            for (int i = 0; i < freecells.Length; i++)
                freecells[i].Remove(cardObject.name);
        }

        // FREECELL PLACEMENT
        if (clickedTag.transform.CompareTag("Freecell"))
        {
            int freeIndex = Array.IndexOf(freecellPositions, clickedTag);
            if (freeIndex < 0 || freecells == null) return;

            freecells[freeIndex].Add(cardObject.name);

            cardObject.transform.SetParent(clickedTag.transform);
            cardObject.transform.position =
                clickedTag.transform.position + new Vector3(0f, 0f, -0.03f);

            FixSortingOrders(clickedTag.transform);

			if (audioSource != null && placeCardSFX != null) {
				audioSource.PlayOneShot(placeCardSFX);
			}
            UpdateTopCards();
			CheckWinCondition();

            return;
        }

        // TABLEAU PLACEMENT
        if (clickedTag.transform.CompareTag("Tableau"))
        {
            int tabIndex = Array.IndexOf(tableauPositions, clickedTag);
            if (tabIndex < 0) return;

            tableaus[tabIndex].Add(cardObject.name);

            Vector3 basePos = tableauPositions[tabIndex].transform.position;
            int depth = tableaus[tabIndex].Count - 1;
            Vector3 newPos = basePos + (cardOffset * depth);

            cardObject.transform.SetParent(clickedTag.transform);
            cardObject.transform.position = newPos;

            FixSortingOrders(clickedTag.transform);
			if (audioSource != null && placeCardSFX != null) {
				audioSource.PlayOneShot(placeCardSFX);
			}
            UpdateTopCards();
			CheckWinCondition();

            return;
        }

        // FOUNDATION PLACEMENT
        if (clickedTag.transform.CompareTag("Foundation"))
        {
            int fIndex = Array.IndexOf(foundationPositions, clickedTag);
            if (fIndex < 0) return;

            foundations[fIndex].Add(cardObject.name);

            cardObject.transform.SetParent(clickedTag.transform);
            cardObject.transform.position =
                clickedTag.transform.position + new Vector3(0f, 0f, -0.03f);

            FixSortingOrders(clickedTag.transform);
			if (audioSource != null && placeCardSFX != null) {
				audioSource.PlayOneShot(placeCardSFX);
			}
            UpdateTopCards();
			CheckWinCondition();

            return;
        }
		if (audioSource != null && placeCardSFX != null) {
			audioSource.PlayOneShot(placeCardSFX);
		}
        UpdateTopCards();
		CheckWinCondition();

    }

    public bool IsTopCard(GameObject cardObject)
    {
        if (cardObject == null) return false;

        Transform parent = cardObject.transform.parent;
        if (parent == null) return false;

        
        if (parent.CompareTag("Tableau") || parent.CompareTag("Foundation"))
        {
            return parent.childCount > 0 && parent.GetChild(parent.childCount - 1).gameObject == cardObject;
        }

        if (parent.CompareTag("Freecell"))
        {
            return parent.childCount == 1 && parent.GetChild(0).gameObject == cardObject;
        }

        return parent.childCount > 0 && parent.GetChild(parent.childCount - 1).gameObject == cardObject;
    }

    public bool IsLastInTab(GameObject cardObject)
    {
        foreach (List<string> tab in tableaus)
        {
            if (tab.Count > 0 && tab.Last() == cardObject.name)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsBlocked(GameObject cardObject)
    {
        foreach (Transform child in cardObject.transform.parent)
        {
            if (child.gameObject != cardObject && child.position.z < cardObject.transform.position.z)
            {
                return true;
            }
        }
        return false;
    }

    private string GetRankString(string card)
    {
        if (string.IsNullOrEmpty(card) || card.Length < 2) return null;
        string rest = card.Substring(1);
        string[] parts = rest.Split('_');
        return parts.Length > 0 ? parts[0] : rest;
    }

    private char? GetSuitChar(string card)
    {
        if (string.IsNullOrEmpty(card) || card.Length == 0) return null;
        return card[0];
    }

    public bool IsAlternatingColor(string card1, string card2)
    {
        if (card1 == null || card2 == null) return false;
        char suit1 = card1[0];
        char suit2 = card2[0];
        bool isRed1 = (suit1 == 'D' || suit1 == 'H');
        bool isRed2 = (suit2 == 'D' || suit2 == 'H');
        return isRed1 != isRed2;
    }

    public bool IsSameSuit(string card1, string card2)
    {
        if (card1 == null || card2 == null) return false;
        return card1[0] == card2[0];
    }

    public bool IsOneRankHigher(string card1, string card2)
    {
        if (card1 == null || card2 == null) return false;
        string r1s = GetRankString(card1);
        string r2s = GetRankString(card2);
        if (r1s == null || r2s == null) return false;
        int r1 = Array.IndexOf(ranks, r1s);
        int r2 = Array.IndexOf(ranks, r2s);
        return r1 == r2 + 1;
    }

    public bool IsOneRankLower(string card1, string card2)
    {
        if (card1 == null || card2 == null) return false;
        string r1s = GetRankString(card1);
        string r2s = GetRankString(card2);
        if (r1s == null || r2s == null) return false;
        int rank1 = Array.IndexOf(ranks, r1s);
        int rank2 = Array.IndexOf(ranks, r2s);
        return (rank1 + 1) % ranks.Length == rank2;
    }

    public bool CanPlaceOnFoundation(string card, int foundationIndex)
    {
        if (foundationIndex < 0 || foundationIndex >= foundationSuitMap.Length) return false;

        if (foundations[foundationIndex].Count == 0)
        {
            string rank = GetRankString(card);
            char? suit = GetSuitChar(card);
            return rank == "A" && suit.HasValue && suit.Value == foundationSuitMap[foundationIndex];
        }
        string topCard = foundations[foundationIndex].Last();
        return IsSameSuit(card, topCard) && IsOneRankHigher(card, topCard);
    }

    public bool CanPlaceOnTableau(string card, int tableauIndex)
    {
        if (tableaus[tableauIndex].Count == 0)
        {
            return GetRankString(card) == "K";
        }
        string topCard = tableaus[tableauIndex].Last();
        return IsAlternatingColor(card, topCard) && IsOneRankLower(card, topCard);
    }

    private Transform ResolveTarget(GameObject toLocation)
    {
        ResolveTarget(toLocation, out GameObject clickedTag, out _, out _);
        return clickedTag != null ? clickedTag.transform : null;
    }

    void ResolveTarget(GameObject toLocation, out GameObject clickedTag, out int foundationIndex, out int tableauIndex)
    {
        clickedTag = toLocation.transform.CompareTag("Card") ? toLocation.transform.parent.gameObject : toLocation;
        foundationIndex = -1;
        tableauIndex = -1;
        if (clickedTag.transform.CompareTag("Foundation"))
            foundationIndex = System.Array.IndexOf(foundationPositions, clickedTag);
        else if (clickedTag.transform.CompareTag("Tableau"))
            tableauIndex = System.Array.IndexOf(tableauPositions, clickedTag);
    }

    public void UpdateTopCards()
    {
        print("=== UpdateTopCards() CALLED ===");
        foreach (var t in tableauPositions)
        {
            print($"Pile {t.name} HAS CHILDREN = {t.transform.childCount}");
        }

        void DisableAllInPile(Transform pile)
        {
            if (pile == null) return;

            for (int i = 0; i < pile.childCount; i++)
            {
                Transform child = pile.GetChild(i);

                CardSprite cs = child.GetComponentInParent<CardSprite>();
                if (cs != null)
                    cs.SetTop(false);
            }
        }

        void EnableTop(Transform pile)
        {
            if (pile == null) return;
            if (pile.childCount == 0) return;

            Transform topChild = pile.GetChild(pile.childCount - 1);

            CardSprite cs = topChild.GetComponentInParent<CardSprite>();
            if (cs != null)
            {
                print("Top is now: " + cs.cardName);
                cs.SetTop(true);
            }
            else
            {
                print("ERROR: No CardSprite found on top object in pile " + pile.name);
            }
        }

        // Disable all
        foreach (var t in tableauPositions) DisableAllInPile(t.transform);
        foreach (var f in foundationPositions) DisableAllInPile(f.transform);
        foreach (var fc in freecellPositions) DisableAllInPile(fc.transform);

        // Enable new tops
        foreach (var t in tableauPositions) EnableTop(t.transform);
        foreach (var f in foundationPositions) EnableTop(f.transform);
        foreach (var fc in freecellPositions) EnableTop(fc.transform);
    }

    public void FixSortingOrders(Transform pile)
    {
        for (int i = 0; i < pile.childCount; i++)
        {
            SpriteRenderer sr = pile.GetChild(i).GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.sortingOrder = i;
        }
    }
	
	private void CheckWinCondition()
	{
		if (foundation0.Count == 13 &&
			foundation1.Count == 13 &&
			foundation2.Count == 13 &&
			foundation3.Count == 13)
		{
			if (winText != null)
				winText.gameObject.SetActive(true);
		}
	}

}
