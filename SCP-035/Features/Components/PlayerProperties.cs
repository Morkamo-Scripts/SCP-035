using SCP_035.Features.Components.Interfaces;
using UnityEngine;

namespace SCP_035.Features.Components;

public class PlayerProperties(Scp035PlayerComponent scp035PlayerComponent) : IPropertyModule
{
    public Scp035PlayerComponent Scp035PlayerComponent { get; } = scp035PlayerComponent;
    
    public bool IsEquipping035Item { get; set; }
    public bool IsPlayScp035 { get; set; }
    public bool IsEscapingHintActive { get; set; }
    
    public Coroutine Scp035ProcessorCoroutine { get; set; }
    public Coroutine ScpLabelHintProcessorCoroutine { get; set; }
    public Coroutine HumeShieldProcessorCoroutine { get; set; }
    public GameObject HighlightPrefab { get; set; }
}