using Data;
using System;
using UI;
using UnityEngine.UIElements;

public static class StaticDragEvent
{
    // Existing event to start the visual drag operation
    public static Action<HeroBoxTreeAsset, HeroDefinition> OnStartHeroDragBox;

    // NEW: Signals that a pointer-down occurred on a hero box.
    // Carries the full event details and a reference to the specific hero box.
    public static Action<PointerDownEvent, HeroBoxTreeAsset> OnHeroPointerDown;
    
    // NEW: Signals that a pointer-up occurred anywhere.
    // This will be invoked by the central controller.
    public static Action<PointerUpEvent> OnGlobalPointerUp;


    public static void StartHeroDragBox(HeroBoxTreeAsset heroBoxTreeAsset, HeroDefinition heroDefinition)
    {
        OnStartHeroDragBox?.Invoke(heroBoxTreeAsset, heroDefinition);
    }

    public static void HeroPointerDown(PointerDownEvent evt, HeroBoxTreeAsset heroBox)
    {
        OnHeroPointerDown?.Invoke(evt, heroBox);
    }

    public static void GlobalPointerUp(PointerUpEvent evt)
    {
        OnGlobalPointerUp?.Invoke(evt);
    }
}
