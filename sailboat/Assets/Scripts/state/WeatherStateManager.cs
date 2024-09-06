using UnityEngine;

public class WeatherStateManager
{
    private WeatherController weatherController;
    private OceanAdvanced oceanController;
    private SoundController soundController;

    public WeatherStateManager(WeatherController wc, OceanAdvanced oc, SoundController sc)
    {
        weatherController = wc;
        oceanController = oc;
        soundController = sc;
    }

    public void SetCalmWeather()
    {
        weatherController.SetCalmWeather();
        oceanController.setCalmWater();
        soundController.SetCalmSound();
        Debug.Log("Weather set to Calm.");
    }

    public void SetStormIncomingWeather()
    {
        weatherController.SetStormApproachingWeather();
        oceanController.setChoppyWater();
        soundController.setStormySound();
        Debug.Log("Weather set to Storm Incoming.");
    }

    public void SetStormyWeather()
    {
        weatherController.SetStormyWeather();
        oceanController.setStormyWater();
        if (soundController.getState() != SoundController.SoundState.Stormy)
        {
            soundController.setStormySound();
        }
        Debug.Log("Weather set to Stormy.");
    }
}