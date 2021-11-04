using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Slider xSlider, ySlider, sourceSlider, locksSlider, tunnelsSlider;
    public Toggle colorToggle;
    public TextMeshProUGUI xText, yText, sourceText, locksText, tunnelsText;

    private void Start()
    {
        LoadPlayerPrefs();
    }

    private void LoadPlayerPrefs()
    {
        if (PlayerPrefs.HasKey("X"))
        {
            int x = PlayerPrefs.GetInt("X");
            xText.text = x.ToString();
            xSlider.value = x;
        }
        else
        {
            PlayerPrefs.SetInt("X", (int)xSlider.value);
        }

        if (PlayerPrefs.HasKey("Y"))
        {
            int y = PlayerPrefs.GetInt("Y");
            yText.text = y.ToString();
            ySlider.value = y;
        }
        else
        {
            PlayerPrefs.SetInt("Y", (int)ySlider.value);
        }

        if (PlayerPrefs.HasKey("Sources"))
        {
            int sources = PlayerPrefs.GetInt("Sources");
            sourceText.text = sources.ToString();
            sourceSlider.value = sources;
        }
        else
        {
            PlayerPrefs.SetInt("Sources", (int)sourceSlider.value);
        }

        if (PlayerPrefs.HasKey("Locks"))
        {
            int locks = PlayerPrefs.GetInt("Locks");
            locksText.text = locks.ToString();
            locksSlider.value = locks;
        }
        else
        {
            PlayerPrefs.SetInt("Locks", (int)locksSlider.value);
        }

        if (PlayerPrefs.HasKey("Tunnels"))
        {
            int tunnels = PlayerPrefs.GetInt("Tunnels");
            tunnelsText.text = tunnels.ToString();
            tunnelsSlider.value = tunnels;
        }
        else
        {
            PlayerPrefs.SetInt("Tunnels", (int)tunnelsSlider.value);
        }

        if (PlayerPrefs.HasKey("SourceColors"))
        {
            colorToggle.isOn = PlayerPrefs.GetInt("SourceColors") == 1;
        }
    }

    public void OnStartButton()
    {
        SceneManager.LoadScene("Game");
    }

    public void OnXSliderChange(float value)
    {
        PlayerPrefs.SetInt("X", (int)value);
        xText.text = value.ToString();
    }

    public void OnYSliderChange(float value)
    {
        PlayerPrefs.SetInt("Y", (int)value);
        yText.text = value.ToString();
    }

    public void OnSourceSliderChange(float value)
    {
        PlayerPrefs.SetInt("Sources", (int)value);
        sourceText.text = value.ToString();
    }

    public void OnLocksSliderChange(float value)
    {
        PlayerPrefs.SetInt("Locks", (int)value);
        locksText.text = value.ToString();
    }

    public void OnTunnelsSliderChange(float value)
    {
        PlayerPrefs.SetInt("Tunnels", (int)value);
        tunnelsText.text = value.ToString();
    }

    public void OnColorToggleChange(bool value)
    {
        PlayerPrefs.SetInt("SourceColors",value ? 1 : 0);
    }

    public void OnSeedValueChange(string value)
    {
        if (string.IsNullOrEmpty(value))
            PlayerPrefs.DeleteKey("Seed");
        else
            PlayerPrefs.SetInt("Seed", int.Parse(value));
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }
}
