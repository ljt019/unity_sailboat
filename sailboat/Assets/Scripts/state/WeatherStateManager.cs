using UnityEngine;

/// <summary>
/// Manages weather-related state changes, coordinating with Weather, Ocean, and Sound controllers.
/// </summary>
public class WeatherStateManager
{
    private readonly WeatherController weatherController;
    private readonly OceanAdvanced oceanController;
    private readonly SoundController soundController;

    /// <summary>
    /// Initializes a new instance of the WeatherStateManager class.
    /// </summary>
    /// <param name="wc">WeatherController instance.</param>
    /// <param name="oc">OceanAdvanced instance.</param>
    /// <param name="sc">SoundController instance.</param>
    public WeatherStateManager(WeatherController wc, OceanAdvanced oc, SoundController sc)
    {
        weatherController = wc ?? throw new System.ArgumentNullException(nameof(wc));
        oceanController = oc ?? throw new System.ArgumentNullException(nameof(oc));
        soundController = sc ?? throw new System.ArgumentNullException(nameof(sc));
    }

    /// <summary>
    /// Sets the game weather to Calm.
    /// </summary>
    public void SetCalmWeather()
    {
        weatherController.SetCalmWeather();
        oceanController.setCalmWater();
        soundController.SetCalmSound();

        LogDebug("Weather set to Calm.");
    }

    /// <summary>
    /// Sets the game weather to Storm Incoming.
    /// </summary>
    public void SetStormIncomingWeather()
    {
        weatherController.SetStormApproachingWeather();
        oceanController.setChoppyWater();
        soundController.setStormySound();

        LogDebug("Weather set to Storm Incoming.");
    }

    /// <summary>
    /// Sets the game weather to Stormy.
    /// </summary>
    public void SetStormyWeather()
    {
        weatherController.SetStormyWeather();
        oceanController.setStormyWater();

        if (soundController.getState() != SoundController.SoundState.Stormy)
        {
            soundController.setStormySound();
        }

        LogDebug("Weather set to Stormy.");
    }

    /// <summary>
    /// Logs debug messages if in debug build.
    /// </summary>
    /// <param name="message">The debug message.</param>
    private void LogDebug(string message)
    {
        if (Debug.isDebugBuild)
        {
            Debug.Log(message);
        }
    }
}
