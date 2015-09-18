using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;

public class SurvivalShooterNetworkManager : NetworkManager
{

    public void StartupHost()
    {
        SetPort();
        NetworkManager.singleton.StartHost();
    }

    public void JoinGame()
    {
        SetIPAddress();
        SetPort();
        NetworkManager.singleton.StartClient();
    }

    void SetIPAddress()
    {
        // string ipAddress = GameObject.Find("InputFieldIPAdress").transform.FindChild("Text").GetComponent<Text>().text;
        // NetworkManager.singleton.networkAddress = ipAddress;
        NetworkManager.singleton.networkAddress = "localhost";
    }

    void SetPort()
    {
        NetworkManager.singleton.networkPort = 7777;
    }

    void OnLevelWasLoaded(int level)
    {
        if (level == 0)
        {
            SetupMenuSceneButton();
        }
        else
        {
            SetupOtherSceneButton();
        }

    }

    void SetupMenuSceneButton()
    {
        GameObject.Find("ButtonStartHost").GetComponent<Button>().onClick.RemoveAllListeners(); ;
        GameObject.Find("ButtonStartHost").GetComponent<Button>().onClick.AddListener(StartupHost);
        GameObject.Find("ButtonJointHost").GetComponent<Button>().onClick.RemoveAllListeners(); ;
        GameObject.Find("ButtonJointHost").GetComponent<Button>().onClick.AddListener(JoinGame);
    }

    void SetupOtherSceneButton()
    {
        Button buttonDisconnect = GameObject.Find("ButtonDisconnect").GetComponent<Button>();
        buttonDisconnect.onClick.RemoveAllListeners();
        buttonDisconnect.onClick.AddListener(NetworkManager.singleton.StopHost);
    }
}
