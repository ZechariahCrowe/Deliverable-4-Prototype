using UnityEngine;
using Firebase.Firestore;
using TMPro;
using System.Threading.Tasks;
using Unity.VisualScripting;
using System;
//using Unity.Android.Types;
using UnityEngine.Android;
using System.Linq;
using UnityEngine.UI;

public class ProfileUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text employeeName;

    [Header("Simulation Data")]
    [SerializeField] private TMP_Text bestTime;

    [SerializeField] private TMP_Text numAttempts;

    [Header("Navigation Manager")]
    public TestNavigationManager navigationManager;

    [Header("Screen")]
    [SerializeField] private GameObject loginScreen;

    [Header("Buttons")]
    [SerializeField] private Button btnHome;
    [SerializeField] private Button btnLogout;

    //On Enable, fetch the user data from firestore and display it on the UI
    private void OnEnable()
    {
        //load the user data from firestore and display it on the UI
        LoadUserData();

        //Load user simulation data from firestore and display it on the UI
        LoadSimulationData();
    }

    //start for the buttons
    public void Start()
    {
        if(btnHome != null)
        {
            btnHome.onClick.AddListener(() =>
            {
                navigationManager.GoBack();
            });
        }

        if(btnLogout != null)
        {

            btnLogout.onClick.AddListener(() =>
            {
                // Sign out the user
                FirebaseManager.Instance.SignOut();

                // Navigate back to the login screen
                navigationManager.OpenScreen(loginScreen);
            });
            
        }
      
    }
    public async void LoadUserData()
    {
        //Check if the user is logged in
        if (FirebaseManager.Instance == null || FirebaseManager.Instance.user == null)
        {
            Debug.LogError("[ProfileUI] No user is logged in.");
            return;
        }

        //Fetch the user ID from the authenticated user
        string uid = FirebaseManager.Instance.user.UserId;

        //Fetch the user document from Firestore
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DocumentReference userRef = db.Collection("users").Document(uid);

        try
        {
            // Get the user document snapshot and check if it exists
            DocumentSnapshot snapshot = await userRef.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                string username = snapshot.GetValue<string>("username");

                // Update the UI with the fetched user data
                if (username != null)
                {
                    employeeName.text =  $"Welcome, {username}!";
                }
                else
                {
                    employeeName.text = "Unknown User";
                }
                
            }
            else
            {
                Debug.LogError("[ProfileUI] User document does not exist.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ProfileUI] Error fetching user data: {ex.Message}");
        }
    }

    public async void LoadSimulationData()
    {
        if (FirebaseManager.Instance == null || FirebaseManager.Instance.user == null)
        {
            Debug.LogError("[ProfileUI] No user is logged in.");
            return;
        }

        string uid = FirebaseManager.Instance.user.UserId;
        Debug.Log($"[ProfileUI] Trying to fetch path: users/{uid}");

        // Fetch simulation data from Firestore
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        CollectionReference simReference = db.Collection("users").Document(uid).Collection("simulation_results");

        try
        {
            //Get all simulation snapshots for the user
            QuerySnapshot allSimSnapshots = await simReference.GetSnapshotAsync();
            int totalSimulations = allSimSnapshots.Count;

            //If there are no attempts then display "No attempts yet" in the UI
            if (totalSimulations == 0)
            {
                numAttempts.text = "No attempts yet";
                bestTime.text = "N/A";
                return;
            }

            //Get the best time from the simulations if there are any attempts
            QuerySnapshot bestTimeSnapshot = await simReference.OrderBy("duration").Limit(1).GetSnapshotAsync();
            
            if (bestTimeSnapshot.Count > 0)
            {
                DocumentSnapshot bestSimDoc = bestTimeSnapshot.Documents.FirstOrDefault();
                float bestDuration = bestSimDoc.GetValue<float>("duration");
                bestTime.text = $"{bestDuration:F2} seconds";
                numAttempts.text = totalSimulations.ToString();
            }
            else
            {
                bestTime.text = "N/A";
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ProfileUI] Error fetching simulation data: {ex.Message}");

        }
    }
}
