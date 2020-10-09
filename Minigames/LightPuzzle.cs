using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class ButtonPanel
{
    public Image image;
    public bool isOn;
}

enum ButtonPosition
{
    TopLeft, TopMid, TopRight,
    MidLeft, MidMid, MidRight,
    BotLeft, BotMid, BotRight
}

public class LightPuzzle : MonoBehaviour
{
    List<ButtonPanel> buttons = new List<ButtonPanel>();
    public float speedChange = 2.5f;
    bool interactable = false;
    public Animator doorAnimator;

    private void Awake()
    {
        print("not even me");
    }

    private void Start()
    {
        if (GameManager.Instance.db.CheckKey("LightPuzzle")) doorAnimator.Play("LightDoorOpen");
    }

    public void StartPuzzle()
    {
        CanvasManager.Instance.MobileControlsSetActive(false);
        interactable = true;
        if (buttons.Count == 0) InitializeButtons();
        transform.GetChild(0).gameObject.SetActive(true);
    }

    public void ExitPuzzle()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        CanvasManager.Instance.MobileControlsSetActive(true);
    }

    private void OnEnable()
    {

    }

    private void InitializeButtons()
    {
        byte counter = 0;
        foreach (Transform t in transform.GetChild(0).Find("Buttons"))
        {
            ButtonPanel newButtonPanel = new ButtonPanel()
            {
                image = t.GetComponent<Image>(),
                isOn = true
            };
            buttons.Add(newButtonPanel);
            counter++;
        }
        SetRandomStatusButtons();
    }

    private void Update()
    {
        ButtonsLerpColor();
    }

    private void SetRandomStatusButtons()
    {
        foreach (var button in buttons) button.isOn = Random.Range(0, 10) % 2 == 0 ? true : false;
        if (GameIsWon()) SetRandomStatusButtons();
    }

    private void ButtonsLerpColor()
    {
        foreach (var button in buttons)
        {
            button.image.color = button.isOn ?
                Color.Lerp(button.image.color, Color.red, Time.deltaTime * speedChange) :
                Color.Lerp(button.image.color, Color.white, Time.deltaTime * speedChange);
        }
    }

    private void SwapButtonStatus(int button_index)
    {
        buttons[button_index].isOn = !buttons[button_index].isOn;
    }

    private void CheckPuzzleStatus()
    {
        if (!GameIsWon()) return;
        interactable = false;
        Invoke("Win", 3f);
    }

    private bool GameIsWon()
    {
        foreach (var button in buttons) if (!button.isOn) return false;
        return true;
    }

    private void Win()
    {
        GameManager.Instance.db.SetKey("LightPuzzle", 1);
        CanvasManager.Instance.LightPuzzleSetActive(false);
        doorAnimator.Play("LightDoorOpen");
    }

    ButtonPosition GetButtonPositionByIndex(int button_index)
    {
        ButtonPosition buttonPosition;
        switch (button_index)
        {
            case 0: buttonPosition = ButtonPosition.TopLeft; break;
            case 1: buttonPosition = ButtonPosition.TopMid; break;
            case 2: buttonPosition = ButtonPosition.TopRight; break;

            case 3: buttonPosition = ButtonPosition.MidLeft; break;
            case 4: buttonPosition = ButtonPosition.MidMid; break;
            case 5: buttonPosition = ButtonPosition.MidRight; break;

            case 6: buttonPosition = ButtonPosition.BotLeft; break;
            case 7: buttonPosition = ButtonPosition.BotMid; break;
            default: buttonPosition = ButtonPosition.BotRight; break;
        }
        return buttonPosition;
    }

    public void OnButtonPressed(int button_index)
    {
        if (!interactable) return;

        ButtonPosition pressedPosition = GetButtonPositionByIndex(button_index);

        switch (pressedPosition)
        {
            case ButtonPosition.TopLeft:
                SwapButtonStatus(0);
                SwapButtonStatus(1);
                SwapButtonStatus(3);
                break;

            case ButtonPosition.TopMid:
                SwapButtonStatus(0);
                SwapButtonStatus(1);
                SwapButtonStatus(2);
                SwapButtonStatus(4);
                break;

            case ButtonPosition.TopRight:
                SwapButtonStatus(2);
                SwapButtonStatus(1);
                SwapButtonStatus(5);
                break;

            case ButtonPosition.MidLeft:
                SwapButtonStatus(0);
                SwapButtonStatus(3);
                SwapButtonStatus(6);
                SwapButtonStatus(4);
                break;

            case ButtonPosition.MidMid:
                SwapButtonStatus(4);
                SwapButtonStatus(1);
                SwapButtonStatus(3);
                SwapButtonStatus(7);
                SwapButtonStatus(5);
                break;

            case ButtonPosition.MidRight:
                SwapButtonStatus(2);
                SwapButtonStatus(4);
                SwapButtonStatus(8);
                SwapButtonStatus(5);
                break;

            case ButtonPosition.BotLeft:
                SwapButtonStatus(3);
                SwapButtonStatus(6);
                SwapButtonStatus(7);
                break;

            case ButtonPosition.BotMid:
                SwapButtonStatus(6);
                SwapButtonStatus(7);
                SwapButtonStatus(8);
                SwapButtonStatus(4);
                break;

            default:
                SwapButtonStatus(8);
                SwapButtonStatus(7);
                SwapButtonStatus(5);
                break;
        }

        CheckPuzzleStatus();
    }
}
