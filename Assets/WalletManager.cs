using UnityEngine;
using TMPro;

public class WalletManager : MonoBehaviour
{
    public static WalletManager Instance { get; private set; }

    [SerializeField] private TMP_Text WalletText;
    private int balance;

    private const string BalanceKey = "WalletBalance"; // PlayerPrefs key for balance

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        // Load balance from PlayerPrefs or set default to 5000
        balance = PlayerPrefs.GetInt(BalanceKey, 5000);
        UpdateWalletUI();
    }

    public void AddBalance(int amount)
    {
        balance += amount;
        SaveBalance();
        UpdateWalletUI();
    }

    public void DecreaseBalance(int amount)
    {
        balance -= amount;
        if (balance < 0)
        {
            balance = 5000; // Prevent negative balance
        }
        SaveBalance();
        UpdateWalletUI();
    }

    private void SaveBalance()
    {
        PlayerPrefs.SetInt(BalanceKey, balance);
        PlayerPrefs.Save();
    }

    private void UpdateWalletUI()
    {
        WalletText.text = balance.ToString();
    }
}
