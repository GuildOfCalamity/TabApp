using Microsoft.UI.Xaml;

namespace TabApp.Helpers;

public class CustomStateTrigger : StateTriggerBase
{
    bool _state = false;

    /// <summary>
    /// Property that determines the active state.
    /// </summary>
    public bool CurrentState 
    { 
        get => _state; 
        set { 
            _state = value;
            SetActive(_state); // Microsoft.UI.Xaml.StateTriggerBase.SetActive(bool IsActive)
        } 
    }

    public CustomStateTrigger()
    {
        SetActive(_state); // Microsoft.UI.Xaml.StateTriggerBase.SetActive(bool IsActive)
    }
}
