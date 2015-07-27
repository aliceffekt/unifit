﻿using System;
using UnityEngine;
using System.Collections.Generic;
using GamepadInput;
using UnityEngine.UI;

public class InputConfigurator : MonoBehaviour
{
    public int PlayersToConfigure = 4;

    public Text PlayerLabel;
    public Image KeyImage;
    public Text KeyLabel;

    public int CurrentKeyImageIndex;
    public Sprite[] KeyImages;

    enum ConfigurationPhase 
    {
        WaitingToReadInput,
        
        SlideUp,
        SlideDown,
        SlideLeft,
        SlideRight,

        RocketUp,
        RocketDown,
        RocketLeft,
        RocketRight,

        Finished,
    }

    enum ControllerType
    {
        Keyboard,
        Gamepad
    }

    int CurrentPlayer;
    ConfigurationPhase CurrentPhase;

    private ControllerType CurrentControllerType;

    private KeyCode KeyboardSlideUp;
    private KeyCode KeyboardSlideDown;
    private KeyCode KeyboardSlideLeft;
    private KeyCode KeyboardSlideRight;
    private KeyCode KeyboardRocketUp;
    private KeyCode KeyboardRocketDown;
    private KeyCode KeyboardRocketLeft;
    private KeyCode KeyboardRocketRight;

    private GamePad.Index GamePadIndex;
    private GamePad.Axis GamePadHorizontalAxis;
    private AxisDirection GamePadHorizontalAxisDirection;
    private GamePad.Axis GamePadVerticalAxis;
    private AxisDirection GamePadVerticalAxisDirection;
    private GamePad.Button GamePadRocketUp;
    private GamePad.Button GamePadRocketDown;
    private GamePad.Button GamePadRocketLeft;
    private GamePad.Button GamePadRocketRight;

    public enum AxisDirection
    {
        X,
        X_Negative,
        Y,
        Y_Negative
    }

	void Start () 
    {
        CurrentPlayer = 1;
        CurrentPhase = ConfigurationPhase.SlideUp;
    }

    void SaveAndQuit()
    {
        foreach (var player in RawMappings)
        {
            foreach (var setting in player.Value)
            {
                PlayerPrefs.SetString("p"+player.Key+setting.Key, setting.Value);
                Debug.Log("saving p" + player.Key + setting.Key + " : " + setting.Value);
            }
        }

        PlayerPrefs.Save();
        Application.LoadLevel(1);
    }

    void QuitWithoutSaving()
    {

    }

    Dictionary<int, Dictionary<string, string>> RawMappings = new Dictionary<int, Dictionary<string, string>>();

    void StoreForSaving()
    {
        Dictionary<string, string> mapping = new Dictionary<string, string>();
        RawMappings[CurrentPlayer] = mapping;

        if (CurrentControllerType == ControllerType.Keyboard)
        {
            mapping.Add("ControllerType", "keyboard");

            mapping.Add("KeyboardSlideUp", KeyboardSlideUp.ToString());
            mapping.Add("KeyboardSlideDown", KeyboardSlideDown.ToString());
            mapping.Add("KeyboardSlideLeft", KeyboardSlideLeft.ToString());
            mapping.Add("KeyboardSlideRight", KeyboardSlideRight.ToString());

            mapping.Add("KeyboardRocketUp", KeyboardRocketUp.ToString());
            mapping.Add("KeyboardRocketDown", KeyboardRocketDown.ToString());
            mapping.Add("KeyboardRocketLeft", KeyboardRocketLeft.ToString());
            mapping.Add("KeyboardRocketRight", KeyboardRocketRight.ToString());
        }
        else
        {
            mapping.Add("ControllerType", "gamepad");

            mapping.Add("GamePadIndex", GamePadIndex.ToString());
            mapping.Add("GamePadHorizontalAxis", GamePadHorizontalAxis.ToString());
            mapping.Add("GamePadHorizontalAxisDirection", GamePadHorizontalAxisDirection.ToString());
            mapping.Add("GamePadVerticalAxis", GamePadVerticalAxis.ToString());
            mapping.Add("GamePadVerticalAxisDirection", GamePadVerticalAxisDirection.ToString());

            mapping.Add("GamePadRocketUp", GamePadRocketUp.ToString());
            mapping.Add("GamePadRocketDown", GamePadRocketDown.ToString());
            mapping.Add("GamePadRocketLeft", GamePadRocketLeft.ToString());
            mapping.Add("GamePadRocketRight", GamePadRocketRight.ToString());
        }
    }

	void Update () 
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (CurrentPhase == ConfigurationPhase.SlideUp)
            {
                if (CurrentPlayer == 1)
                {
                    QuitWithoutSaving();
                }
                else
                {
                    CurrentPlayer--;
                    GetComponent<GameJoinControl>().PlayersReady[CurrentPlayer - 1] = false;
                }
            }
            else
            {
                CurrentPhase = ConfigurationPhase.SlideUp;
            }            
        }

	    if (WaitingForAxisReset)
	    {
	        if (GamePad.GetAxis(GamePad.Axis.Dpad, GamePadIndex).magnitude > 0.1f ||
	            GamePad.GetAxis(GamePad.Axis.LeftStick, GamePadIndex).magnitude > 0.1f ||
	            GamePad.GetAxis(GamePad.Axis.RightStick, GamePadIndex).magnitude > 0.1f)
	        {
	            return;
	        }

	        WaitingForAxisReset = false;
	    }

	    if (CurrentPhase == ConfigurationPhase.WaitingToReadInput)
	    {
	        if (CurrentControllerType != ControllerType.Gamepad ||
                AnyPad() == GamePad.Index.Any)
	        {
	            CurrentPhase++;
	        }
	    }
        else if (CurrentPhase == ConfigurationPhase.SlideUp)
        {
            if (PressedKeyOrButton() != KeyCode.None)
            {
                CurrentControllerType = ControllerType.Keyboard;
                FetchAxisMapping(ref KeyboardSlideUp, ref GamePadVerticalAxis, ref GamePadVerticalAxisDirection);
            }
            else
            {
                GamePad.Index index = AnyPad();
                if (index != GamePad.Index.Any)
                {
                    CurrentControllerType = ControllerType.Gamepad;
                    GamePadIndex = index;
                    FetchAxisMapping(ref KeyboardSlideUp, ref GamePadVerticalAxis, ref GamePadVerticalAxisDirection);
                }
            }
        }
        else if (CurrentPhase == ConfigurationPhase.SlideDown)
        {
            FetchAxisMapping(ref KeyboardSlideDown, ref GamePadHorizontalAxis, ref GamePadHorizontalAxisDirection, true);
        }
        else if (CurrentPhase == ConfigurationPhase.SlideLeft)
        {
            FetchAxisMapping(ref KeyboardSlideLeft, ref GamePadHorizontalAxis, ref GamePadHorizontalAxisDirection);
        }
        else if (CurrentPhase == ConfigurationPhase.SlideRight)
        {
            FetchAxisMapping(ref KeyboardSlideRight, ref GamePadHorizontalAxis, ref GamePadHorizontalAxisDirection, true);
        }
        else if (CurrentPhase == ConfigurationPhase.RocketUp)
        {
            FetchButtonMapping(ref KeyboardRocketUp, ref GamePadRocketUp);
        }
        else if (CurrentPhase == ConfigurationPhase.RocketDown)
        {
            FetchButtonMapping(ref KeyboardRocketDown, ref GamePadRocketDown);
        }
        else if (CurrentPhase == ConfigurationPhase.RocketLeft)
        {
            FetchButtonMapping(ref KeyboardRocketLeft, ref GamePadRocketLeft);
        }
        else if (CurrentPhase == ConfigurationPhase.RocketRight)
        {
            FetchButtonMapping(ref KeyboardRocketRight, ref GamePadRocketRight);
        }
        else if (CurrentPhase == ConfigurationPhase.Finished)
        {
            StoreForSaving();
            GetComponent<GameJoinControl>().PlayersReady[CurrentPlayer - 1] = true;

            if (CurrentPlayer < PlayersToConfigure)
            {
                CurrentPlayer++;
                CurrentPhase = ConfigurationPhase.WaitingToReadInput;
            }
            else
            {
                SaveAndQuit();
            }
        }

        UpdateGUI();
	}

    private bool WaitingForAxisReset;

    void FetchAxisMapping(ref KeyCode fetchedKey, ref GamePad.Axis fetchedAxis, ref AxisDirection fetchedDirection, bool ignored = false)
    {
        if (CurrentControllerType == ControllerType.Keyboard)
        {
            KeyCode key = PressedKeyOrButton();
            if (key != KeyCode.None)
            {
                fetchedKey = key;
                CurrentPhase++;
            }
        }
        else
        {
            if (GamePad.GetAxis(GamePad.Axis.Dpad, GamePadIndex).magnitude > 0.3f)
            {
                if (!ignored)
                {
                    fetchedAxis = GamePad.Axis.Dpad;
                    fetchedDirection = ToDirection(GamePad.GetAxis(GamePad.Axis.Dpad, GamePadIndex));
                }
                CurrentPhase++;
                WaitingForAxisReset = true;
            }
            else if (GamePad.GetAxis(GamePad.Axis.LeftStick, GamePadIndex).magnitude > 0.3f)
            {
                if (!ignored)
                {
                    fetchedAxis = GamePad.Axis.LeftStick;
                    fetchedDirection = ToDirection(GamePad.GetAxis(GamePad.Axis.LeftStick, GamePadIndex));
                }
                CurrentPhase++;
                WaitingForAxisReset = true;
            }
            else if (GamePad.GetAxis(GamePad.Axis.RightStick, GamePadIndex).magnitude > 0.3f)
            {
                if (!ignored)
                {
                    fetchedAxis = GamePad.Axis.RightStick;
                    fetchedDirection = ToDirection(GamePad.GetAxis(GamePad.Axis.RightStick, GamePadIndex));
                }
                CurrentPhase++;
                WaitingForAxisReset = true;
            }
        }
    }

    private void FetchButtonMapping(ref KeyCode fetchedKey, ref GamePad.Button fetchedButton)
    {
        if (CurrentControllerType == ControllerType.Keyboard)
        {
            KeyCode key = PressedKeyOrButton();
            if (key != KeyCode.None)
            {
                fetchedKey = key;
                CurrentPhase++;
            }
        }
        else
        {
            if (GamePad.GetButtonDown(GamePad.Button.A, GamePadIndex))
            {
                fetchedButton = GamePad.Button.A;
                CurrentPhase++;
            }
            else if (GamePad.GetButtonDown(GamePad.Button.B, GamePadIndex))
            {
                fetchedButton = GamePad.Button.B;
                CurrentPhase++;
            }
            else if (GamePad.GetButtonDown(GamePad.Button.X, GamePadIndex))
            {
                fetchedButton = GamePad.Button.X;
                CurrentPhase++;
            }
            else if (GamePad.GetButtonDown(GamePad.Button.Y, GamePadIndex))
            {
                fetchedButton = GamePad.Button.Y;
                CurrentPhase++;
            }
            if (GamePad.GetButtonDown(GamePad.Button.Back, GamePadIndex))
            {
                fetchedButton = GamePad.Button.Back;
                CurrentPhase++;
            }
            else if (GamePad.GetButtonDown(GamePad.Button.Start, GamePadIndex))
            {
                fetchedButton = GamePad.Button.Start;
                CurrentPhase++;
            }
        }
    }

    AxisDirection ToDirection(Vector2 axis)
    {
        if (axis.x > 0.1f)
        {
            return AxisDirection.X_Negative;
        }
        else if (axis.x < -0.1f)
        {
            return AxisDirection.X;
        }
        else if (axis.y > 0.1f)
        {
            return AxisDirection.Y;
        }
        else if(axis.y < -0.1f)
        {
            return AxisDirection.Y_Negative;
        }
        throw new Exception();
    }

    GamePad.Index AnyPad()
    {
        if (GamePad.GetState(GamePad.Index.One).Any())
        {
            return GamePad.Index.One;
        }
        
        if (GamePad.GetState(GamePad.Index.Two).Any())
        {
            return GamePad.Index.Two;
        }

        if (GamePad.GetState(GamePad.Index.Three).Any())
        {
            return GamePad.Index.Three;
        }

        if (GamePad.GetState(GamePad.Index.Four).Any())
        {
            return GamePad.Index.Four;
        }

        return GamePad.Index.Any;
    }

    public KeyCode PressedKeyOrButton()
    {
        foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(kcode) &&
                !(kcode.ToString().Contains("Joystick") || 
                  kcode.ToString().Contains("Mouse") || 
                  kcode == KeyCode.Escape))
            {
                return kcode;
            }
        }
        return KeyCode.None;
    }

    private void UpdateGUI()
    {
        PlayerLabel.text = "PLAYER " + CurrentPlayer;

        if (CurrentPhase == ConfigurationPhase.SlideUp)
        {
            KeyLabel.text = "PRESS SLIDE UP";
            KeyImage.sprite = KeyImages[0];
        }
        else if (CurrentPhase == ConfigurationPhase.SlideDown)
        {
            KeyLabel.text = "PRESS SLIDE DOWN";
            KeyImage.sprite = KeyImages[1];
        }
        else if (CurrentPhase == ConfigurationPhase.SlideLeft)
        {
            KeyLabel.text = "PRESS SLIDE LEFT";
            KeyImage.sprite = KeyImages[2];
        }
        else if (CurrentPhase == ConfigurationPhase.SlideRight)
        {
            KeyLabel.text = "PRESS SLIDE RIGHT";
            KeyImage.sprite = KeyImages[3];            
        }
        else if (CurrentPhase == ConfigurationPhase.RocketUp)
        {
            KeyLabel.text = "PRESS FIRE UP";
            KeyImage.sprite = KeyImages[4]; 
        }
        else if (CurrentPhase == ConfigurationPhase.RocketDown)
        {
            KeyLabel.text = "PRESS FIRE DOWN";
            KeyImage.sprite = KeyImages[5]; 
        }
        else if (CurrentPhase == ConfigurationPhase.RocketLeft)
        {
            KeyLabel.text = "PRESS FIRE LEFT";
            KeyImage.sprite = KeyImages[6]; 
        }
        else if (CurrentPhase == ConfigurationPhase.RocketRight)
        {
            KeyLabel.text = "PRESS FIRE UP";
            KeyImage.sprite = KeyImages[7]; 
        }
    }
}
