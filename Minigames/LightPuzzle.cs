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
    bool interactable = true;

    private void Start()
    {
        byte counter = 0;
        foreach (Transform t in transform)
        {
            ButtonPanel newButtonPanel = new ButtonPanel()
            {
                image = t.GetComponent<Image>(),
                isOn = counter % 3 == 0 ? true : false
            };
            buttons.Add(newButtonPanel);
            counter++;
        }
    }

    private void Update()
    {
        ButtonsLerpColor();
    }

    private void SetRandomStatusButtons()
    {
        foreach (var button in buttons) button.isOn = Random.Range(0, 10) % 2 == 0 ? true : false;
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

    private void SwapButtonStatus(int button_index)
    {
        buttons[button_index].isOn = !buttons[button_index].isOn;
    }

    private void CheckPuzzleStatus()
    {
        foreach (var button in buttons) if (!button.isOn) return;
        interactable = false;
        Invoke("Win", 3f);
    }

    private void Win()
    {
        SetRandomStatusButtons();
        interactable = true;
        print("¡Has ganado!");
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
