﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour, IGameManager
{
    public ManagerStatus status { get; private set; }

    private NonePanelControll nonePanel;

    [Header("UI")]
    [SerializeField] private GameObject UIObject;

    [Header("Left panel")]
    [SerializeField] private GameObject leftPanelOwner;
    [SerializeField] private RealPanelControll leftPanel;

    [Header("Right panel")]
    [SerializeField] private GameObject rightPanelOwner;
    [SerializeField] private RealPanelControll rightPanel;

    [Header("Element selector")]
    [SerializeField] private GameObject elementBarOwner;
    [SerializeField] private ElementBar elementBar;

    [Header("Elements sprites")]
    [SerializeField] private Sprite fire;
    [SerializeField] private Sprite water;
    [SerializeField] private Sprite snow;

    [Header("PopUp display panel")]
    [SerializeField] private PopUpMessager popUpMessager;

    [Header("Episode time")]
    [SerializeField] private ValueBar episodeTimeBar;

    [Header("Generation label")]
    [SerializeField] private TMP_Text generationLabel;

    [Header("LockScrean")]
    [SerializeField] private LockScrean lockScrean;

    [Header("Escape menu")]
    [SerializeField] private GameObject escapeMenu;

    public void Startup()
    {
        Debug.Log("Starting UI manager");

        nonePanel = new NonePanelControll();
        leftPanel.gameObject.SetActive(false);
        rightPanel.gameObject.SetActive(false);

        elementBar.SetupBarSprites(fire, water, snow);
        elementBar.gameObject.SetActive(false);

        CloseEscapeMenu();

        status = ManagerStatus.Started;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            ActivateEscapeMenu();
        }
    }

    public void LockApp(string reason)
    {
        SetLockScreanActive(true);
        SetLockScreanReason(reason);
        enabled = false;
    }

    public PanelControll SetupUIPanelController(GameObject owner, PanelType type)
    {
        if(type == PanelType.LEFT)
        {
            if(leftPanelOwner == null)
            {
                SetLeftPanelOwner(owner);
                return leftPanel;
            }

            if(leftPanelOwner == owner)
            {
                return leftPanel;
            }
        }

        if(type == PanelType.RIGHT)
        {
            if(rightPanelOwner == null)
            {
                SetRightPanelOwner(owner);
                return rightPanel;
            }

            if(rightPanelOwner == owner)
            {
                return rightPanel;
            }
        }

        return nonePanel;
    }

    public ElementBar SetupElementBar(GameObject owner)
    {
        if(elementBarOwner == null)
        {
            SetElementBarOwner(owner);
            return elementBar;
        }

        if(elementBarOwner == owner)
        {
            return elementBar;
        }

        return null;
    }

    private void SetLeftPanelOwner(GameObject owner)
    {
        leftPanelOwner = owner;
        leftPanel.gameObject.SetActive(true);
    }

    private void SetRightPanelOwner(GameObject owner)
    {
        rightPanelOwner = owner;
        rightPanel.gameObject.SetActive(true);
    }

    private void SetElementBarOwner(GameObject owner)
    {
        elementBarOwner = owner;
        elementBar.gameObject.SetActive(true);
    }

    public void RemovePanelOwner(GameObject owner, PanelType panelType)
    {
        if(panelType == PanelType.LEFT && leftPanelOwner == owner)
        {
            leftPanelOwner = null;
            leftPanel.gameObject.SetActive(false);
        }

        if(panelType == PanelType.RIGHT && rightPanelOwner == owner)
        {
            rightPanelOwner = null;
            rightPanel.gameObject.SetActive(false);
        }
    }

    public void RemoveElementBarOwner(GameObject owner)
    {
        if(elementBarOwner == owner)
        {
            elementBar.gameObject.SetActive(false);
            elementBarOwner = null;
        }
    }

    public void DisplayPopUpMessage(string msg)
    {
        popUpMessager.DisplayMessage(msg);
    }

    public void DisplayPopUpMessageWithTime(string msg, float time)
    {
        popUpMessager.SetTimeOfDisplay(time);
        popUpMessager.DisplayMessage(msg);
    }

    public void SetupGenerationLabel(int value)
    {
        generationLabel.text = "Generation: " + value;
    }

    public void SetupEpisodeTimeBar(float maxValue, float value)
    {
        episodeTimeBar.SetupBar(maxValue, value);
    }

    public void ChangeEpisodeTimeBarValue(float newValue)
    {
        episodeTimeBar.ChangeValue(newValue);
    }

    public void SetLockScreanReason(string reason)
    {
        lockScrean.SetReason(reason);
    }

    public void SetLockScreanActive(bool active)
    {
        lockScrean.gameObject.SetActive(active);
    }

    public void ActivateEscapeMenu()
    {
        Managers.App.PauseGame();
        Managers.Level.LockPlayerLook(true);
        Managers.Level.LockSpectatorLook(true);
        LockCursor(false);
        escapeMenu.SetActive(true);
    }

    public void CloseEscapeMenu()
    {
        Managers.App.ResumeGame();
        Managers.Level.LockPlayerLook(false);
        Managers.Level.LockSpectatorLook(false);
        LockCursor(true);
        escapeMenu.SetActive(false);
    }

    public void LockCursor(bool locked)
    {
        Cursor.visible = !locked;
        if(locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
}