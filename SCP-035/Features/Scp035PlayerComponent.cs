using Exiled.API.Features;
using SCP_035.Features.Components;
using UnityEngine;

namespace SCP_035.Features;

public sealed class Scp035PlayerComponent() : MonoBehaviour
{
    private void Awake()
    {
        Player = Player.Get(gameObject);
        PlayerProps = new PlayerProperties(this);
    }
    
    public Player Player { get; private set; }
    public PlayerProperties PlayerProps { get; private set; }
}