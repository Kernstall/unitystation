﻿using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraZoomHandler : MonoBehaviour
{
    private int zoomLevel = 2;
    public int ZoomLevel => zoomLevel;
    private bool scrollWheelzoom = false;
    public bool ScrollWheelZoom => scrollWheelzoom;

    void Start()
    {
        if (PlayerPrefs.HasKey(PlayerPrefKeys.CamZoomKey))
        {
            zoomLevel = PlayerPrefs.GetInt(PlayerPrefKeys.CamZoomKey);
            scrollWheelzoom = PlayerPrefs.GetInt(PlayerPrefKeys.ScrollWheelZoom) == 1;
        }
        else
        {
            zoomLevel = 2;
            PlayerPrefs.SetInt(PlayerPrefKeys.CamZoomKey, 2);
            PlayerPrefs.SetInt(PlayerPrefKeys.ScrollWheelZoom, 0);
            PlayerPrefs.Save();
        }

        Refresh();
    }

    void Update()
    {
        //Process any scroll wheel zooming:
        if (scrollWheelzoom && !EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.mouseScrollDelta.y > 0f)
            {
                if (!MouseOutside()) IncreaseZoomLevel();
            }

            if (Input.mouseScrollDelta.y < 0f)
            {
                if (!MouseOutside()) DecreaseZoomLevel(true);
            }
        }
    }

    bool MouseOutside()
    {
        var view = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        return view.x < 0 || view.x > 1 || view.y < 0 || view.y > 1;
    }

    // Refreshes after setting zoom level.
    public void Refresh()
    {
        // Calculate ratio.
        double ratio = Camera.main.pixelHeight / (double) Camera.main.pixelWidth;

        // Calculate scaling factor. 409600 is a magic number.
        double scaleFactor = Math.Sqrt(Camera.main.pixelHeight * Camera.main.pixelWidth / (409600 * ratio));

        // Automatically set zoom level if it's less than zero.
        if (zoomLevel < 1)
        {
            zoomLevel = Mathf.RoundToInt((float) scaleFactor);
        }

        // Calculate orthographic size with full precision and then convert to float precision.
        Camera.main.orthographicSize = Convert.ToSingle(ratio * 10 * scaleFactor / zoomLevel);

        // Recenter camera.
        DisplayManager.Instance.SetCameraFollowPos();
    }

    public void SetZoomLevel(float zoomLevel)
    {
        this.zoomLevel = Mathf.Clamp((int) zoomLevel, 0, 10);
        Refresh();
        PlayerPrefs.SetInt(PlayerPrefKeys.CamZoomKey, this.zoomLevel);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// A convenient way to increase zoom level
    /// <summary>
    public void IncreaseZoomLevel()
    {
        zoomLevel++;
        if (zoomLevel >= 10) zoomLevel = 10;
        SetZoomLevel(zoomLevel);
    }

    /// <summary>
    /// A convenient way to increase zoom level
    /// ZoomLevel of 0 = Auto Zoom
    /// <summary>
    public void DecreaseZoomLevel(bool preventAuto = false)
    {
        zoomLevel--;
        if (zoomLevel <= 0) zoomLevel = 0;
        if (preventAuto && zoomLevel == 0) zoomLevel = 1;
        SetZoomLevel(zoomLevel);
    }

    public void ToggleScrollWheelZoom(bool activeState)
    {
        scrollWheelzoom = activeState;
        if (activeState)
        {
            PlayerPrefs.SetInt(PlayerPrefKeys.ScrollWheelZoom, 1);
        }
        else
        {
            PlayerPrefs.SetInt(PlayerPrefKeys.ScrollWheelZoom, 1);
        }
        PlayerPrefs.Save();
    }

    public void ResetDefaults()
    {
        ToggleScrollWheelZoom(false);
        SetZoomLevel(2);
    }
}