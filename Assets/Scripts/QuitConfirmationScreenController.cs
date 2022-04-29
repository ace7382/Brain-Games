using UnityEngine;
using Doozy.Runtime.Signals;
using Doozy.Runtime.UIManager.Containers;
using UnityEngine.SceneManagement;

public class QuitConfirmationScreenController : MonoBehaviour
{
    //NOTES:    To add custom pause/exit behavior to certain games
    //          just add a stream listener for the popup/return to game streams

    //Called by the QUitConfirmation Screen's Return to Game Button's OnClick
    public void ReturnToGame()
    {
        Signal.Send("QuitConfirmation", "BackToGame");

        AudioManager.instance.Play("Button Click");

        GetComponent<UIView>().Hide();
    }

    //Called by the Quit Confirmation Screen's Exit Button's OnClick
    //Each mode with a quit confirmation will handle ending the game by listening for this signal
    public void ExitLevel()
    {
        Signal.Send("QuitConfirmation", "ExitLevel");

        AudioManager.instance.Play("Button Click");

        GetComponent<UIView>().Hide();
    }

    //Called by Quit Confirmation Screen's OnHidden callback (after hide animation is finished)
    public void UnloadScene()
    {
        SceneManager.UnloadSceneAsync("ExitConfirmationScreen");
    }

    private void ShowExitPopup(Signal signal)
    {
        GetComponent<UIView>().Show();
    }
}
