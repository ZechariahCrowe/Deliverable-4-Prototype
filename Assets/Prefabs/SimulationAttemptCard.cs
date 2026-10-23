using UnityEngine;
using TMPro;
using Firebase.Firestore;
using UnityEngine.UI;
using System.Threading.Tasks;

public class SimulationAttemptCard : MonoBehaviour
{
    [Header("Simulation inputs")]
    [SerializeField] private TMP_Text txtAttemptNumber;
    [SerializeField] private TMP_Text txtError;
    [SerializeField] private TMP_Text txtDuration;
    [SerializeField] private TMP_Text txtTapCount;
    [SerializeField] private TMP_Text txtStatus;
    [SerializeField] private TMP_Text txtTimestamp;

    [Header("Button")]
    [SerializeField] private Button btnDelete;

    private string UserID;
    private string simulationID;

    public void SetAttemptData(int attemptNumber, int error, float duration, int tapCount, string status, Timestamp timestamp, string uid, string simId)
    {
        UserID = uid;
        simulationID = simId;
        txtAttemptNumber.text = $"Attempt: {attemptNumber}";
        txtError.text = error.ToString();
        txtDuration.text = duration.ToString("F2") + " seconds";
        txtTapCount.text = tapCount.ToString();
        txtStatus.text = status;
        
        System.DateTime dateTime = timestamp.ToDateTime().ToLocalTime();
        txtTimestamp.text = dateTime.ToString("yyyy-MM-dd HH:mm:ss");

        if (btnDelete != null)
        {
            btnDelete.onClick.RemoveAllListeners();
            btnDelete.onClick.AddListener(DeleteSimulation);
        }
    }

    private async void DeleteSimulation()
    {
        //test if the userID is null or empty
        if (string.IsNullOrEmpty(UserID) || string.IsNullOrEmpty(simulationID))
        {
            Debug.LogError("User ID is null or empty. Cannot delete user.");
            return;
        }

        Debug.Log($"Delete simulation {simulationID} for user with ID: {UserID}");
        bool success = await DeleteSimulationFromFirebase(UserID, simulationID); // Call the DeleteUser method from FirebaseManager

        if (success)
        {
            //remove the card from the UI
            Destroy(gameObject);
        }
    }

    private async Task<bool> DeleteSimulationFromFirebase(string userID, string simID)
    {
        try
        {
            FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

            await db.Collection("users").Document(userID).Collection("simulation_results").Document(simID).DeleteAsync();
            Debug.Log($"Successfylly deleted simulation {simID}");
            return true;
        }
        catch(System.Exception ex)
        {
            Debug.LogError($"Failed to delete user with ID: {userID}. Error: {ex.Message}");
            return false;
        }
    } 
}
