using UnityEngine;
using System.Collections;
using System;

public class SunMovement : MonoBehaviour {
	private GameTime _timeMgr;

	//	public Transform sun;
	public float dayCycleInMinutes = 1;

	public double Altitude = 0;
	public double Azimuth = 0;
	public double longitude = 41.647453;
	public double latitude = -0.888630;
	public bool AutoUpdate = true;

	private const float SECOND = 1f;
	private const float MINUTE = 60 * SECOND;
	private const float HOUR = 60 * MINUTE;
	private const float DAY = 24 * HOUR;

	private const float DEGREES_PER_SECOND = 360 / DAY;

	private float _degreeRotation;
	private float _timeOfDay;

	private const double Deg2Rad = Math.PI / 180.0;
	private const double Rad2Deg = 180.0 / Math.PI;

	private Renderer[] _renderers;

	/// current day phase
	public DayPhase currentPhase;

	public Color nightColor;
	public Color dawnColor;
	public Color dayColor;
	public Color duskColor;

	public Color nightAmbience;
	public Color dayAmbience;

	public float nightMetallic;
	public float duskMetallic;
	public float dayMetallic;

	public Material daySkybox;
	public Material nightSkybox;

	public Light light;

	public enum DayPhase {
		Night = 0,
		Dawn = 1,
		Day = 2,
		Dusk = 3
	}

	// Use this for initialization
	void Start() {
		_timeMgr = GameTime.GetInstance();

		AutoUpdate = true;
		_timeOfDay = 0;
		_degreeRotation = DEGREES_PER_SECOND * DAY / (dayCycleInMinutes * MINUTE);
		//longitude = 41.647453;
		//latitude = -0.888630;

		_renderers = (Renderer[])Resources.FindObjectsOfTypeAll(typeof(Renderer));

		light = GetComponent<Light>();
	}

	// Update is called once per frame
	void Update() {
		if(AutoUpdate) {
			CalculateSunPosition(_timeMgr.GetDateTime(), latitude, longitude);
		}

		transform.localRotation = Quaternion.Euler((float)Altitude, (float)Azimuth, 0);

		UpdateAmbience(Altitude);
	}

	//
	void OnDestroy() {
		SetMetallic(0);
	}

	// Rudementary phase-check algorithm
	public void UpdateAmbience(double altitude) {
		if(altitude < 0) {
			light.color = nightColor + (light.color - nightColor) * 0.95f;
			RenderSettings.skybox = nightSkybox;
			RenderSettings.ambientLight = nightAmbience;
			if(currentPhase != DayPhase.Night) { SetMetallic(nightMetallic); }
			currentPhase = DayPhase.Night;
		} else if(altitude >= 0 && altitude <= 45) {
			light.color = dawnColor + (light.color - dawnColor) * 0.95f;
			if(currentPhase != DayPhase.Dawn) { SetMetallic(duskMetallic); }
			currentPhase = DayPhase.Dawn;
		} else {
			RenderSettings.skybox = daySkybox;
			light.color = dayColor + (light.color - dayColor) * 0.95f;
			RenderSettings.ambientLight = dayAmbience;
			if(currentPhase != DayPhase.Day) { SetMetallic(dayMetallic); }
			currentPhase = DayPhase.Day;
		}
	}

	/*! 
	* \brief Calculates the sun light. 
	* 
	* CalcSunPosition calculates the suns "position" based on a 
	* given date and time in local time, latitude and longitude 
	* expressed in decimal degrees. It is based on the method 
	* found here: 
	* http://www.astro.uio.no/~bgranslo/aares/calculate.html 
	* The calculation is only satisfiably correct for dates in 
	* the range March 1 1900 to February 28 2100. 
	* \param dateTime Time and date in local time. 
	* \param latitude Latitude expressed in decimal degrees. 
	* \param longitude Longitude expressed in decimal degrees. 
	*/
	public void CalculateSunPosition(DateTime dateTime, double latitude, double longitude) {
		// Convert to UTC  
		//  dateTime = dateTime.ToUniversalTime();  

		// Number of days from J2000.0.  
		double julianDate = 367 * dateTime.Year -
			(int)((7.0 / 4.0) * (dateTime.Year +
				(int)((dateTime.Month + 9.0) / 12.0))) +
			(int)((275.0 * dateTime.Month) / 9.0) +
			dateTime.Day - 730531.5;

		double julianCenturies = julianDate / 36525.0;

		// Sidereal Time  
		double siderealTimeHours = 6.6974 + 2400.0513 * julianCenturies;

		double siderealTimeUT = siderealTimeHours +
			(366.2422 / 365.2422) * (double)dateTime.TimeOfDay.TotalHours;

		double siderealTime = siderealTimeUT * 15 + longitude;

		// Refine to number of days (fractional) to specific time.  
		julianDate += (double)dateTime.TimeOfDay.TotalHours / 24.0;
		julianCenturies = julianDate / 36525.0;

		// Solar Coordinates  
		double meanLongitude = CorrectAngle(Deg2Rad *
			(280.466 + 36000.77 * julianCenturies));

		double meanAnomaly = CorrectAngle(Deg2Rad *
			(357.529 + 35999.05 * julianCenturies));

		double equationOfCenter = Deg2Rad * ((1.915 - 0.005 * julianCenturies) *
			Math.Sin(meanAnomaly) + 0.02 * Math.Sin(2 * meanAnomaly));

		double elipticalLongitude =
			CorrectAngle(meanLongitude + equationOfCenter);

		double obliquity = (23.439 - 0.013 * julianCenturies) * Deg2Rad;

		// Right Ascension  
		double rightAscension = Math.Atan2(
			Math.Cos(obliquity) * Math.Sin(elipticalLongitude),
			Math.Cos(elipticalLongitude));

		double declination = Math.Asin(
			Math.Sin(rightAscension) * Math.Sin(obliquity));

		// Horizontal Coordinates  
		double hourAngle = CorrectAngle(siderealTime * Deg2Rad) - rightAscension;

		if(hourAngle > Math.PI) {
			hourAngle -= 2 * Math.PI;
		}

		double altitude = Math.Asin(Math.Sin(latitude * Deg2Rad) *
			Math.Sin(declination) + Math.Cos(latitude * Deg2Rad) *
			Math.Cos(declination) * Math.Cos(hourAngle));

		// Nominator and denominator for calculating Azimuth  
		// angle. Needed to test which quadrant the angle is in.  
		double aziNom = -Math.Sin(hourAngle);
		double aziDenom =
			Math.Tan(declination) * Math.Cos(latitude * Deg2Rad) -
			Math.Sin(latitude * Deg2Rad) * Math.Cos(hourAngle);

		double azimuth = Math.Atan(aziNom / aziDenom);

		if(aziDenom < 0) // In 2nd or 3rd quadrant  
		{
			azimuth += Math.PI;
		} else if(aziNom < 0) // In 4th quadrant  
		  {
			azimuth += 2 * Math.PI;
		}

		// Altitude  
		Altitude = altitude * Rad2Deg;

		// Azimut  
		Azimuth = azimuth * Rad2Deg;
	}

	/*! 
    * \brief Corrects an angle. 
    * 
    * \param angleInRadians An angle expressed in radians. 
    * \return An angle in the range 0 to 2*PI. 
    */
	private double CorrectAngle(double angleInRadians) {
		if(angleInRadians < 0) {
			return 2 * Math.PI - (Math.Abs(angleInRadians) % (2 * Math.PI));
		} else if(angleInRadians > 2 * Math.PI) {
			return angleInRadians % (2 * Math.PI);
		} else {
			return angleInRadians;
		}
	}

	//
	public void SetMetallic(float metallic) {
		foreach(Renderer rend in _renderers) {
			foreach(Material mat in rend.sharedMaterials) {
				if(mat && mat.HasProperty("_Metallic")) {
					mat.SetFloat("_Metallic", metallic);
				}
			}
		}
	}
}
