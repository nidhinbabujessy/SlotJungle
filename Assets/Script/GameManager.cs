using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

using TMPro;
using DG.Tweening;
using Sequence = DG.Tweening.Sequence;


public class GameManager : MonoBehaviour
{
    [Header("Slot Settings")]
    [SerializeField] private GameObject[] symbolPrefabs;         // 11 symbol prefabs
    [SerializeField] private Transform[] slotPositions;          // 15 slot positions (3x5 grid)
    [SerializeField] private float spinDuration = 2f;            // Duration of spin
    [SerializeField] private float spinSpeed = 10f;              // Speed of spinning
    [SerializeField] private float stopAnimationDuration = 0.5f; // Duration for stop animation
    [SerializeField] private Transform symbolParent;             // Parent to hold all symbols

    private GameObject[] activeSymbols = new GameObject[15]; // Store active symbols

    [Header("BackGround Animation")]
    [SerializeField] private GameObject[] objectPrefabs; // Prefab to spawn
    [SerializeField] private Transform[] spawnPoints; // Array of spawn positions
    [SerializeField] private int poolSize = 10; // Size of object pool

    private Dictionary<GameObject, Queue<GameObject>> objectPools;

    [Header("Game Win")]
    private int[] WinData;
    private GameObject winningSymbolPrefab;
    public Color highlightColor = Color.yellow; // Color for highlighting
    [SerializeField] private GameObject win;
    [SerializeField] private GameObject winChild;
    [SerializeField] private GameObject fail;
    [SerializeField] private GameObject FailChild;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private Vector3 initialPosition = new Vector3(-1200, 0, 0);
    [SerializeField] private Vector3 visiblePosition = new Vector3(-220, 0, 0);
    [SerializeField] private Vector3 exitPosition = new Vector3(2500, 0, 0);
    [SerializeField] private float pauseInterval = 2f;

    [Header("Bet Amount")]
    [SerializeField] private TMP_Text ChipAmountDisplay;
    int betAmunt;
    int i=1;


    void Start()
    {
        SoundManager.instance.PlayBGMusic();
       // WalletManager.Instance.UpdateWalletUI();
       
        InitializeSlots();
       // SaveTagsToWinData();
       // StartSpin();
        UpdateChipAmount();


        //-----------BackGround Animantion---------
        InitializeObjectPools();
        InvokeRepeating("SpawnObject", 1f, 1f);
    }



    #region  Slot Spinng
    public void StartSpinButton()
    {
        StartSpin();
        WalletManager.Instance.DecreaseBalance(betAmunt);
    }
   void StartSpin()
    {
       
        StartCoroutine(SpinReels());
        SoundManager.instance.PlaySFX("buttonClick");
       // WalletManager.Instance.DecreaseBalance(betAmunt);
        SoundManager.instance.PlaySFX("spinning");


    }
    // Start the spin
    // Initialize slots with random symbols
    void InitializeSlots()
    {
        int randomIndex1 = Random.Range(0, symbolPrefabs.Length);
        winningSymbolPrefab = symbolPrefabs[randomIndex1];

        for (int i = 0; i < slotPositions.Length; i++)
        {
            // Check if the current index is part of a winning condition
            if (IsWinningSlot(i))
            {
                // Instantiate a winning symbol at this slot
                activeSymbols[i] = Instantiate(winningSymbolPrefab, slotPositions[i].position, Quaternion.identity, symbolParent);
            }
            else
            {
                // Instantiate a random symbol if not part of a winning condition
                int randomIndex = Random.Range(0, symbolPrefabs.Length);
                activeSymbols[i] = Instantiate(symbolPrefabs[randomIndex], slotPositions[i].position, Quaternion.identity, symbolParent);
            }
        }
    }

    // Check if the slot is part of a winning condition
    bool IsWinningSlot(int index)
    {
        // Loop through all possible winning conditions
        int[][] winningPatterns = new int[][]
        {
        new int[] {6, 7, 8, 9, 10},
        new int[] {11, 10, 13, 14, 15},
        new int[] {1, 2, 3, 4, 5},
        new int[] {11, 7, 3, 9, 15},
        new int[] {1, 7, 13, 9, 5}
        };

        foreach (var pattern in winningPatterns)
        {
            if (pattern.Contains(index))
            {
                return true;
            }
        }

        return false;
    }

  

    // Spin logic with smooth animation
    IEnumerator SpinReels()
    {
        float elapsedTime = 0f;

        while (elapsedTime < spinDuration)
        {
            for (int i = 0; i < slotPositions.Length; i++)
            {
                // Move symbol down smoothly
                activeSymbols[i].transform.Translate(Vector3.down * spinSpeed * Time.deltaTime);

                // Reset position if it goes too far down
                if (activeSymbols[i].transform.position.y < slotPositions[i].position.y - 0.1f) // Reduced downward distance
                {
                    Destroy(activeSymbols[i]);
                   int randomIndex = Random.Range(0, symbolPrefabs.Length);
                    activeSymbols[i] = Instantiate(symbolPrefabs[randomIndex], slotPositions[i].position + Vector3.up * 0.3f, Quaternion.identity, symbolParent);
                }
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        StartCoroutine(StopReelsWithAnimation());
        SaveTagsToWinData();
        SoundManager.instance.PlaySFX("spinningStop");
        
    }

    // Stop the reels with a smooth animation
    IEnumerator StopReelsWithAnimation()
    {
        float elapsedTime = 0f;
        Vector3[] startPositions = new Vector3[slotPositions.Length];
        Vector3[] endPositions = new Vector3[slotPositions.Length];
        GameObject[] finalSymbols = new GameObject[slotPositions.Length];

        for (int i = 0; i < slotPositions.Length; i++)
        {
            Destroy(activeSymbols[i]);
            int randomIndex = Random.Range(0, symbolPrefabs.Length);
            finalSymbols[i] = Instantiate(symbolPrefabs[randomIndex], slotPositions[i].position + Vector3.up * 0.3f, Quaternion.identity, symbolParent);
            startPositions[i] = finalSymbols[i].transform.position;
            endPositions[i] = slotPositions[i].position;
        }

        while (elapsedTime < stopAnimationDuration)
        {
            float t = elapsedTime / stopAnimationDuration;
            t = t * t * (3f - 2f * t); // Smoothstep for better easing

            for (int i = 0; i < slotPositions.Length; i++)
            {
                finalSymbols[i].transform.position = Vector3.Lerp(startPositions[i], endPositions[i], t);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Finalize positions
        for (int i = 0; i < slotPositions.Length; i++)
        {
            finalSymbols[i].transform.position = slotPositions[i].position;
            activeSymbols[i] = finalSymbols[i]; // Set new symbols as active
        }

        Debug.Log("Spin Complete!");
    }
    #endregion

    #region BackGound animation
    // Create and populate object pools for each prefab
    void InitializeObjectPools()
    {
        objectPools = new Dictionary<GameObject, Queue<GameObject>>();

        foreach (GameObject prefab in objectPrefabs)
        {
            Queue<GameObject> pool = new Queue<GameObject>();

            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = Instantiate(prefab);
                obj.SetActive(false);
                pool.Enqueue(obj);
            }

            objectPools[prefab] = pool;
        }
    }

    // Get object from pool or create a new one if pool is empty
    GameObject GetPooledObject(GameObject prefab)
    {
        if (objectPools[prefab].Count > 0)
        {
            GameObject obj = objectPools[prefab].Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            GameObject obj = Instantiate(prefab);
            return obj;
        }
    }

    // Spawn a random object at a random position
    void SpawnObject()
    {
        if (spawnPoints.Length == 0 || objectPrefabs.Length == 0) return;

        // Select a random prefab and spawn point
        GameObject selectedPrefab = objectPrefabs[Random.Range(0, objectPrefabs.Length)];
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        GameObject obj = GetPooledObject(selectedPrefab);
        obj.transform.position = spawnPoint.position;

        // Move object in a random Y direction and disable after 2 seconds
        StartCoroutine(MoveAndDisable(obj, selectedPrefab));
    }

    System.Collections.IEnumerator MoveAndDisable(GameObject obj, GameObject prefab)
    {
        float elapsedTime = 0f;
        Vector3 startPos = obj.transform.position;

        // Random Y direction (up or down)
        float randomYDirection = Random.Range(-1f, 1f);
        Vector3 endPos = startPos + new Vector3(0, randomYDirection * 2f, 0);

        while (elapsedTime < 2f)
        {
            obj.transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / 2f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        obj.SetActive(false);
        objectPools[prefab].Enqueue(obj);
    }
    #endregion

    #region bet Amount

   
    private void UpdateChipAmount()
    {
        switch (i)
        {
            case 1:
                ChipAmountDisplay.text = "10";
                betAmunt = 10;
                break;
            case 2:
                ChipAmountDisplay.text = "20";
                betAmunt = 20;
                break;
            case 3:
                ChipAmountDisplay.text = "50";
                betAmunt = 50;
                break;
            case 4:
                ChipAmountDisplay.text = "100";
                betAmunt = 100;
                break;
            case 5:
                ChipAmountDisplay.text = "300";
                betAmunt = 300;
                break;
            case 6:
                ChipAmountDisplay.text = "500";
                betAmunt = 500;
                break;
            case 7:
                ChipAmountDisplay.text = "1000";
                betAmunt = 1000;
                break;
            default:
                Debug.LogError("Invalid Chip Amount Index!");
                break;
        }

        Debug.Log($"Updated Chip Amount: {ChipAmountDisplay.text} (i = {i})");
    }

    public void AddAmount()
    {
        SoundManager.instance.PlaySFX("buttonClick");
        i = (i >= 7) ? 1 : i + 1;
        Debug.Log("Clicked AddAmount, New i = " + i);
        UpdateChipAmount();
    }

    public void DecreaseAmount()
    {
        SoundManager.instance.PlaySFX("buttonClick");
        i = (i <= 1) ? 7 : i - 1;
        Debug.Log("Clicked DecreaseAmount, New i = " + i);
        UpdateChipAmount();
    }
    #endregion

    #region Win Logic
    // Convert tags to integers and save to WinData
    void SaveTagsToWinData()
    {
        int childCount = symbolParent.childCount;
        WinData = new int[childCount];

        for (int i = 0; i < childCount; i++)
        {
            Transform child = symbolParent.GetChild(i);
            string tag = child.tag;

            // Try converting tag to integer and store in WinData
            if (int.TryParse(tag, out int tagValue))
            {
                WinData[i] = tagValue;
            }
            else
            {
                Debug.LogWarning($"Tag '{tag}' of child '{child.name}' is not a valid integer. Setting value to -1.");
                WinData[i] = -1; // Set to -1 if conversion fails
            }
        }

        // Print WinData for verification
        Debug.Log("WinData: " + string.Join(", ", WinData));
        CheckWinConditions();

    }


    void CheckWinConditions()
    {
        // Define your winning conditions
        if (CheckCondition(6, 7, 8, 9, 10) || CheckCondition(11, 10, 13, 14, 15) ||
            CheckCondition(1, 2, 3, 4, 5) || CheckCondition(11, 7, 3, 9, 15) ||
            CheckCondition(1, 7, 13, 9, 5) || CheckCondition(6, 7, 8, 9, 10) ||
            CheckCondition(6, 12, 13, 14, 10) || CheckCondition(6, 2, 3, 4, 10)||
            CheckCondition(1,2,8,14,15) || CheckCondition(11,12,8,4,5) ||
            CheckCondition(6,12,8,4,10) || CheckCondition(6,2,8,14,10) ||
            CheckCondition(11,7,8,9,15) || CheckCondition(1,7,8,9,5) ||
            CheckCondition(11,7,13,9,15) || CheckCondition(1,7,3,9,5) ||
            CheckCondition(6,7,13,9,10) || CheckCondition(6,7,3,9,10) ||
            CheckCondition(11,12,3,14,15) || CheckCondition(1,2,13,4,5) ||
            CheckCondition(11,2,3,4,15) )
        {
            Debug.Log("Winning condition met! Highlighting objects...");
            HighlightWinningObjects();
            winMethod();
        }
        else
        {
            Debug.Log("No winning condition met.");
            FailMethod();
        }
    }

    // Check if specific WinData elements are equal
    bool CheckCondition(int a, int b, int c, int d, int e)
    {
        // Check if indices are within bounds and if all values are the same
        if (a < WinData.Length && b < WinData.Length && c < WinData.Length &&
            d < WinData.Length && e < WinData.Length)
        {
            return (WinData[a] == WinData[b] && WinData[b] == WinData[c] &&
                    WinData[c] == WinData[d] && WinData[d] == WinData[e]);
        }

        return false;
    }

    // Highlight objects that satisfy winning conditions
    void HighlightWinningObjects()
    {
        for (int i = 0; i < WinData.Length; i++)
        {
            if (symbolParent.GetChild(i) != null)
            {
                Renderer objRenderer = symbolParent.GetChild(i).GetComponent<Renderer>();
                if (objRenderer != null)
                {
                    objRenderer.material.color = highlightColor;
                }
            }
        }
    }
    #endregion

    #region Win and Fail Display

    void winMethod()
    {
        win.SetActive(true);
        AnimateBetImage(winChild);

        // Deactivate after 3 seconds
        StartCoroutine(DisableAfterDelay(win, 3f));
    }

    void FailMethod()
    {
        fail.SetActive(true);
        AnimateBetImage(FailChild);

        // Deactivate after 3 seconds
        StartCoroutine(DisableAfterDelay(fail, 3f));
    }

    // Coroutine to disable the object after a delay
    private IEnumerator DisableAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (obj != null)
        {
            obj.SetActive(false);
        }
    }


    #endregion

    #region Animation
    private void AnimateBetImage(GameObject image)
    {
        if (image == null)
        {
            Debug.LogError("AnimateBetImage: Image GameObject is null!");
            return;
        }

        image.SetActive(true); // Ensure it's visible before animating
        image.transform.localPosition = initialPosition;
        image.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

        // Create a DOTween sequence
        Sequence sequence = DOTween.Sequence();

        sequence.Append(image.transform.DOLocalMove(visiblePosition, animationDuration).SetEase(Ease.OutBack));
        sequence.AppendInterval(pauseInterval);
        sequence.Append(image.transform.DOLocalMove(exitPosition, animationDuration).SetEase(Ease.InBack));

        // Ensure the image is deactivated after the animation
        sequence.OnComplete(() => image.SetActive(false));

        sequence.Play();
      //  parent.SetActive(false);
    }

    #endregion
}
