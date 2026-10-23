using UnityEngine;
using TMPro;
using Firebase.Firestore;
using UnityEngine.Playables;
using Unity.VisualScripting;
using System;
using UnityEngine.UI;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

public class InspectEmployeeUI : MonoBehaviour
{
    [Header("Fields")]
    [SerializeField] private Transform simulationAttempts;
    [SerializeField] private GameObject simulationAttemptCardPrefab;
    [SerializeField] private TMP_Text txtEmployeeName;

    [Header("Edit Section")]
    [SerializeField] private TMP_InputField edtUsername;
    [SerializeField] private Button btnEdit;
    [SerializeField] private Button btnSave;

    [Header("Navigation Manger")]
    public TestNavigationManager navigationManager;

    [Header("Panel References")]
    public GameObject loginScreen;
   

    [Header("Buttons")]
    [SerializeField] private Button btnLogout;
    [SerializeField] private Button btnHome;

    private string UserID;
    public string simuationDoc;

    void Start()
    {
        if (btnLogout != null)
        {
            btnLogout.onClick.AddListener(OnLogoutClicked);
        }

        if (btnHome != null)
        {
            btnHome.onClick.AddListener(() =>
            {
                navigationManager.GoBack();
            });
        }

        if(btnEdit != null)
        {
            btnEdit.onClick.AddListener(() =>
            {
                btnSave.gameObject.SetActive(true);
                btnEdit.gameObject.SetActive(false);
                txtEmployeeName.gameObject.SetActive(false);
                edtUsername.gameObject.SetActive(true);
             
            });
        }

        if (btnSave != null)
        {
            btnSave.onClick.AddListener(EditUsername);
        }
    }


  

    private void OnLogoutClicked()
    {
       if(navigationManager != null)
        {
            FirebaseManager.Instance.SignOut();
            navigationManager.OpenScreen(loginScreen);
        }
    }

    private async void EditUsername()
    {
        string newUsername = edtUsername.text.Trim();

        if (string.IsNullOrEmpty(UserID) || newUsername == null)
        {
            Debug.Log($"Username or ID cannot be empty");
            return;
        }
        try
        {
            FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
            DocumentReference userRef = db.Collection("users").Document(UserID);

            //Update the username field in firestore
            Dictionary<string, object> updated = new Dictionary<string, object>
            {
                {"username", newUsername }
            };

            await userRef.UpdateAsync(updated);

            //Update the header section
            txtEmployeeName.text = $"{newUsername}'s Simulation History";

            //Deactivate and reactivate the stuffies
            btnSave.gameObject.SetActive(false);
            btnEdit.gameObject.SetActive(true);
            txtEmployeeName.gameObject.SetActive(true);
            edtUsername.gameObject.SetActive(false);
        }
        catch (Exception ex)
        {
            Debug.Log($"[InspectEmployeeUI] Failure: Could not update username. {ex.Message}");
        }


    }

    public async void LoadUserSimulationAttempts(string userId, string username)
    {
        UserID = userId;
        //set the employee name header
        txtEmployeeName.text = $"{username}'s Simulation History";

        //clear the previous attempt cards
        foreach (Transform child in simulationAttempts)
        {
            Destroy(child.gameObject);
        }

        //initialize firestore
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        try
        {

            //Fetch the simulation attempts for the given userId from Firestore
            QuerySnapshot attemptSnapshot = await db.Collection("users").Document(userId).Collection("simulation_results").OrderByDescending("completedAt").GetSnapshotAsync();
            Debug.Log($"[AdminHistoryUI] Found {attemptSnapshot.Count} simulation documents for user ID: {userId}"); // Add this line

            if (attemptSnapshot.Count == 0)
            {
                Debug.Log($"[InspectEmployeeUI] No simulation attempts found for user: {username}");
                return;
            }

            //Loop through each simulation attempt and create a card for it
            int count = 0;
            foreach (DocumentSnapshot simDoc in attemptSnapshot.Documents)
            {
                simuationDoc = simDoc.Id;
                count++;
                if (simDoc.Exists) //Check if the document exists
                {
                    //Extract the relevant fields from the simulation attempt document
                    float duration = simDoc.ContainsField("duration") ? simDoc.GetValue<float>("duration") : 0f;
                    int errors = simDoc.ContainsField("errors") ? simDoc.GetValue<int>("errors") : 0;
                    int taps = simDoc.ContainsField("tapCount") ? simDoc.GetValue<int>("tapCount") : 0;
                    string status = simDoc.ContainsField("status") ? simDoc.GetValue<string>("status") : "Unknown";

                    Timestamp completedAt = simDoc.ContainsField("completedAt") ? simDoc.GetValue<Timestamp>("completedAt") : Timestamp.FromDateTime(System.DateTime.UtcNow);

                    //Create a new simulation attempt card and set its data
                    GameObject attemptCard = Instantiate(simulationAttemptCardPrefab, simulationAttempts);
                    SimulationAttemptCard cardScript = attemptCard.GetComponent<SimulationAttemptCard>();

                    if(cardScript != null)
                    {
                        cardScript.SetAttemptData(count, errors, duration, taps, status, completedAt, UserID, simuationDoc);
                    }

                }
            }
            Debug.Log($"[InspectEmployeeUI] Loaded {count} simulation attempts for user: {username}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[InspectEmployeeUI] Error loading simulation attempts: {ex.Message}");

        }
    }
}
