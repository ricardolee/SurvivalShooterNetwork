
using UnityEngine.Networking;

public class PlayerID : NetworkBehaviour
{
    void Start() {
        transform.name = MakeUniqueIdentity();
    }
    
    private string MakeUniqueIdentity()
    {
        NetworkInstanceId playerNetID = GetComponent<NetworkIdentity>().netId;
        return  "Player_" + playerNetID;
    }
}

