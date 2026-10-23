using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using System.ComponentModel.Design.Serialization;
using System.Threading.Tasks;
using System.Collections.Generic;
using Firebase.Auth;

public class HomeUI : MonoBehaviour
{
    [Header("Home UI Elements")]

    [Header("Navigation Manager")]
    public TestNavigationManager navigationManager;

    [Header("Screen Elements")]
    [SerializeField] private GameObject homeScreen;
    [SerializeField] private GameObject profileScreen;
    [SerializeField] private GameObject loginScreen;
    [SerializeField] private GameObject simulationScreen;

    [Header("Navigation Buttons")]
    [SerializeField] private Button btnHome;
    [SerializeField] private Button btnProfile;
    [SerializeField] private Button btnSignOut;

    [Header("Simulation")]
    [SerializeField] private Button btnSimulation;

    [Header("Leaderboard Stuff")]
    [SerializeField] private Transform leaderboardContainer; // scroll view
    [SerializeField] private GameObject leaderboardPrefab; // Prefab
    [SerializeField] private Button btnRefresh;

    void Start()
    {
        if (btnHome != null)
        {
            btnHome.onClick.AddListener(homeOnClick);
        }
        if (btnProfile != null)
        {
            btnProfile.onClick.AddListener(profileOnClick);
        }
        if (btnSignOut != null)
        {
            btnSignOut.onClick.AddListener(signOutOnClick);
        }
        if(btnSimulation != null)
        {
            btnSimulation.onClick.AddListener(simulationOnClick);
        }
    }

    private void Awake() // use awake to load the leaderboard first, before all starts
    {
        if (btnRefresh != null)
        {
            btnRefresh.onClick.AddListener(async () => await LoadLeaderboard());
        }
    }

    private async void OnEnable()
    {
        while(FirebaseManager.Instance == null || !FirebaseManager.Instance.isInitialized)
        {
            await Awaitable.NextFrameAsync(); // wait for firebase to initialize while using unity's awaitable
        }

        await LoadLeaderboard();
    }

    public void homeOnClick()
    {
        // Navigate to the home screen
        navigationManager.OpenScreen(homeScreen);
    } 
    
    public void profileOnClick()
    {
        // Navigate to the profile screen
        navigationManager.OpenScreen(profileScreen);
    }

    public void signOutOnClick()
    {
        // Sign out the user
        FirebaseManager.Instance.SignOut();

        // Navigate back to the login screen
        navigationManager.OpenScreen(loginScreen);
    }

    public void simulationOnClick()
    {
        navigationManager.OpenScreen(simulationScreen);
    }

    //Leaderboard data

    public async Task LoadLeaderboard()
    {
        //Clear previous entries
        foreach(Transform child in leaderboardContainer)
        {
            Destroy(child.gameObject);
        }

        try
        {
            FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

            //Collection group query matching the 'duration' index
            Query leaderboardQuery = db.CollectionGroup("simulation_results").OrderBy("duration").Limit(5);

            if(FirebaseAuth.DefaultInstance.CurrentUser == null)
            {
                Debug.LogError("Blocked by Rules: CurrentUser is null");
            }
            else
            {
                Debug.Log($"Signed in with {FirebaseAuth.DefaultInstance.CurrentUser.UserId}");
            }

            QuerySnapshot snapshot = await leaderboardQuery.GetSnapshotAsync();

            int place = 1;

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (!document.Exists)
                {
                    continue;
                }

                Dictionary<string, object> data = document.ToDictionary();
                string username = data.ContainsKey("username") ? data["username"].ToString() : "Anonymous";
                double duration = data.ContainsKey("duration") ? System.Convert.ToDouble(data["duration"]) : 0;

                DocumentReference userRef = document.Reference.Parent.Parent; // use parent parent to get the user docuemnt
                DocumentSnapshot userSnap = await userRef.GetSnapshotAsync();

                /*string username = "----";
                if(userSnap.Exists && userSnap.ToDictionary().ContainsKey(username))
                {
                    username = userSnap.ToDictionary()["username"].ToString();
                }*/

                //Instantiate rows
                GameObject rowObject = Instantiate(leaderboardPrefab, leaderboardContainer);
                LeaderboardSard row = rowObject.GetComponent<LeaderboardSard>();

                if(row != null)
                {
                    row.SetupRow(place, username, duration);
                }

                //increase place
                place++;
            }
        }
        catch(System.Exception ex)
        {
            Debug.Log($"Leaderbaord not available messgae : {ex.Message}");
        }
    }



    /*public async void simulationOnClick()
    {
        if (FirebaseManager.Instance.user == null || FirebaseManager.Instance == null)
        {
            Debug.LogError("[HomeUI] No user is logged in.");
            return;
        }
        string status = "Completed";
        int tapCount = 10;
        int errorCount = 2;
        float duration = 34;

        //Fetch the user ID from the authenticated user
        string uid = FirebaseManager.Instance.user.UserId;

        await FirestoreData.Instance.SaveSimulationResultAsync(uid, duration, tapCount, errorCount, status);
    }*/

}
