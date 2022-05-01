using System.Collections;
using DarkRift.Client;
using TutorialPlugin;
using UnityEngine;

public class NetworkBehaviour : MonoBehaviour
{
    public Player Info { get; private set; }

    public void AssignTo(Player player) => 
        Info = player;
}