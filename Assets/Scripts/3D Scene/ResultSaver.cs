using UnityEngine;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;

public class ResultSaver : MonoBehaviour
{
    [Header("Script References")]
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private InputTracker tapTracker;
   public async void SaveSimulationResults()
    {
        string userId = FirebaseManager.Instance.user.UserId;
        string username = "Unknown user";
        string status = "";

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        try
        {
            //fetch user document to get the username
            DocumentSnapshot userDoc = await db.Collection("users").Document(userId).GetSnapshotAsync();
            if (userDoc.Exists && userDoc.ContainsField("username"))
            {
                username = userDoc.GetValue<string>("username");
            }
        }catch(System.Exception ex)
        {
            Debug.Log($"Could not fetch username from firestore");
        }

        

        //Extract from different scripts
        double duration = levelManager != null ? levelManager.EndTime : 0f;
        int errorCount = levelManager != null ? levelManager.currentErrors : 0;
        int tapCount = tapTracker != null ? tapTracker.tapCount : 0;

        if(errorCount == 0)
        {
            status = "Complete";
        }
        if(errorCount > 0 && errorCount < 3)
        {
            status = "Partially Complete";
        }
        if(errorCount == 3)
        {
            status = "Failed";
        }

        //Firebase dictionary
        await FirestoreData.Instance.SaveSimulationResultAsync(userId, username, duration, tapCount, errorCount, status);
    }

    
}
