using UnityEngine;
using System.Collections.Generic;

namespace Tenkoku.Core
{
	public class ParticleStarfieldHandler : MonoBehaviour
	{
		[System.Serializable]
		public class ConstellationDebug
		{
			public string name;
			public bool isVisible;
		}

		// PUBLIC VARIABLES
		public bool starReset = false;
		public int numParticles = 9110;
		public float baseSize = 0.02f;
		public float setSize = 0.02f;
		public float constellationSize = 0.025f;
		public Color starBrightness = Color.white;
		public Vector3 offset = new Vector3(0f, 0f, 0f);
		public TenkokuStarData starData;
		public float starDistance = 1300f;

		// Custom variables
		public Color constellationColor = new Color(0.5f, 0f, 0.5f, 1f);
		public float constellationSizeMultiplier = 1.5f;
		public Color constellationHighlightColor = Color.yellow;

		// Debug constellation checkboxes
		[SerializeField]
		private List<ConstellationDebug> constellationDebug = new List<ConstellationDebug>
		{
			new ConstellationDebug { name = "Little Dipper", isVisible = false },
			new ConstellationDebug { name = "Big Dipper", isVisible = false },
			new ConstellationDebug { name = "Orion", isVisible = false },
			new ConstellationDebug { name = "Taurus", isVisible = false },
			new ConstellationDebug { name = "Scorpius", isVisible = false },
			new ConstellationDebug { name = "Pegasus", isVisible = false },
			new ConstellationDebug { name = "Cassiopeia", isVisible = false },
			new ConstellationDebug { name = "Pisces", isVisible = false },
			new ConstellationDebug { name = "Aquarius", isVisible = false },
			new ConstellationDebug { name = "Capricornus", isVisible = false },
			new ConstellationDebug { name = "Sagittarius", isVisible = false },
			new ConstellationDebug { name = "Libra", isVisible = false },
			new ConstellationDebug { name = "Virgo", isVisible = false },
			new ConstellationDebug { name = "Leo", isVisible = false },
			new ConstellationDebug { name = "Leo Minor", isVisible = false },
			new ConstellationDebug { name = "Cancer", isVisible = false },
			new ConstellationDebug { name = "Gemini", isVisible = false },
			new ConstellationDebug { name = "Draco", isVisible = false }
		};

		// PRIVATE VARIABLES
		private bool hasStarted = false;
		private ParticleSystem StarSystem;
		private ParticleSystem.Particle[] StarParticles;
		private Vector3 offsetC = new Vector3(0f, 0f, 0f);
		private float baseSizeC = 0.02f;
		private float constellationSizeC = 0.025f;
		private Color currStarBrightness = new Color(1f, 1f, 1f, 1f);
		private Renderer rendererComponent;

		// Collect for GC
		private float starDeclination = 0.0f;
		private float starAscension = 0.0f;
		private float h;
		private float m;
		private float s;
		private Color starColO = new Color(0.41f, 0.66f, 1.0f, 0.5f);
		private Color starColB = new Color(0.76f, 0.86f, 1.0f, 0.5f);
		private Color starColA = new Color(1.0f, 1.0f, 1.0f, 0.5f);
		private Color starColF = new Color(0.99f, 1.0f, 0.94f, 0.5f);
		private Color starColG = new Color(1.0f, 0.99f, 0.55f, 0.5f);
		private Color starColK = new Color(1.0f, 0.72f, 0.36f, 0.5f);
		private Color starColM = new Color(1.0f, 0.07f, 0.07f, 0.5f);
		private Color setColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
		private float starFactor = 0.0f;
		private float starMagnitude = 0.0f;
		private bool ConstellationStar = false;

		private int px = 0;
		private int sx = 0;

		private Vector3 setpos;
		private float useSize;

		private Vector3 particlePosition = Vector3.zero;
		private Color baseLerpColor = new Color(0.5f, 0.6f, 1.0f, 1.0f);

		void Start()
		{
			hasStarted = false;
			StarSystem = this.GetComponent<ParticleSystem>();
			rendererComponent = this.GetComponent<Renderer>();
			numParticles = 9110;
			StarSystem.Emit(numParticles);
		}

		void LateUpdate()
		{
			if (offset != offsetC)
			{
				offsetC = offset;
				starReset = true;
			}

			if (setSize != baseSizeC)
			{
				baseSizeC = setSize;
				starReset = true;
			}

			if (constellationSize != constellationSizeC)
			{
				constellationSizeC = constellationSize;
				starReset = true;
			}

			//set overall material color
			if (currStarBrightness != starBrightness)
			{
				currStarBrightness = starBrightness;
				if (Application.isPlaying)
				{
					rendererComponent.material.SetColor("_TintColor", starBrightness);
				}
				else
				{
					rendererComponent.sharedMaterial.SetColor("_TintColor", starBrightness);
				}
			}

			if (!hasStarted)
			{
				hasStarted = true;
				starReset = true;
			}

			if (starReset)
			{
				StSystemUpdate();
			}
		}

		void StSystemUpdate()
		{
			starReset = false;

			StarParticles = new ParticleSystem.Particle[starData.starElements.Length];
			StarSystem.GetParticles(StarParticles);

			for (px = 0; px < StarParticles.Length; px++)
			{
				for (sx = 0; sx < (starData.starElements.Length); sx++)
				{
					h = starData.starElements[sx].ascH;
					m = starData.starElements[sx].ascM;
					s = starData.starElements[sx].ascS;
					starAscension = ((s / 60.0f) * 0.1f) + (m / 60.0f) + (h);

					h = Mathf.Abs(starData.starElements[sx].decH);
					m = starData.starElements[sx].decM;
					s = starData.starElements[sx].decS;
					starDeclination = ((s / 60.0f) * 0.1f) + (m / 60.0f) + (h);
					if (starData.starElements[sx].decH < 0.0f) starDeclination = 0.0f - starDeclination;

					particlePosition.x = 0f;
					particlePosition.y = 0f;
					particlePosition.z = starDistance;
					setpos = particlePosition;

					setpos = Quaternion.AngleAxis((90.0f - starDeclination), -Vector3.left) * setpos;
					StarParticles[px].position = setpos;

					setpos = Quaternion.AngleAxis(((starAscension * 15.0f)), Vector3.forward) * setpos;
					setpos.x = -setpos.x;
					setpos.y = -setpos.y;
					setpos.z = -setpos.z + 4.5f;
					StarParticles[px].position = setpos;

					if (starData.starElements[sx].color == 0) setColor = Color.Lerp(starColO, starColB, starFactor);
					if (starData.starElements[sx].color == 1) setColor = Color.Lerp(starColB, starColA, starFactor);
					if (starData.starElements[sx].color == 2) setColor = Color.Lerp(starColA, starColF, starFactor);
					if (starData.starElements[sx].color == 3) setColor = Color.Lerp(starColF, starColG, starFactor);
					if (starData.starElements[sx].color == 4) setColor = Color.Lerp(starColG, starColK, starFactor);
					if (starData.starElements[sx].color == 5) setColor = Color.Lerp(starColK, starColM, starFactor);
					if (starData.starElements[sx].color == 6) setColor = Color.Lerp(starColM, starColM, starFactor);

					starFactor = starData.starElements[sx].factor;
					if (starFactor > 0.0f) starFactor = starFactor / 9.0f;

					setColor = Color.Lerp(setColor, baseLerpColor, 0.5f);

					starMagnitude = starData.starElements[sx].magnitude;

					setColor.a = 1.0f;
					if (starMagnitude >= 2.0f) setColor.a = Mathf.Lerp(1.0f, 0.0f, (starMagnitude / 9.0f));
					if (starMagnitude < 2.0f) setColor *= 2.0f;
					setColor.a = Mathf.Lerp(1.0f, 0.0f, (starMagnitude / 8.0f));

					ConstellationStar = CheckConstellationStarData(starData.starElements[sx].starIndex);

					setColor.a = Mathf.Lerp(1.0f, 0.075f, (starMagnitude / 8.0f));
					if (setColor.a < 0.6f) setColor.a *= 0.1f;

					useSize = Mathf.Lerp(setSize * 1.4f, setSize, (starMagnitude / 8.0f));

					if (ConstellationStar)
					{
						string constellation = GetConstellationForStar(starData.starElements[sx].starIndex);
						ConstellationDebug debug = constellationDebug.Find(c => c.name == constellation);
						if (debug != null && debug.isVisible)
						{
							setColor = constellationHighlightColor;
						}
						else
						{
							setColor = constellationColor;
						}
						useSize *= constellationSizeMultiplier;
					}
					else
					{
						if (starData.starElements[sx].brightFactor == 4)
						{
							setColor.a *= 1.2f;
						}
						else if (starData.starElements[sx].brightFactor == 5)
						{
							setColor.a *= 1.0f;
						}
						else if (starData.starElements[sx].brightFactor == 3)
						{
							setColor.a *= 1.4f;
						}
						else if (starData.starElements[sx].brightFactor == 2)
						{
							useSize = useSize * 1.1f;
							setColor.a *= 1.6f;
						}
						else if (starData.starElements[sx].brightFactor == 1)
						{
							useSize = useSize * 1.2f;
							setColor.a *= 4.0f;
						}
					}

#if UNITY_5_3_OR_NEWER
					StarParticles[px].startSize = useSize;
					StarParticles[px].startColor = setColor;
#else
                    StarParticles[px].size = useSize;
                    StarParticles[px].color = setColor;
#endif

					px++;
				}
			}

			StarSystem.SetParticles(StarParticles, StarParticles.Length);
			StarSystem.Emit(StarParticles.Length);
			StarSystem.Play();
		}

		private string GetConstellationForStar(int starIndex)
		{
			// Handle constellations with specific known ranges.
			if (starIndex == 8890 || starIndex == 153751 || starIndex == 131873 || starIndex == 137422 ||
				starIndex == 166205 || starIndex == 142105 || starIndex == 148048)
				return "Little Dipper";

			if (starIndex == 103287 || starIndex == 120315 || starIndex == 116842 || starIndex == 106591 ||
				starIndex == 95418 || starIndex == 95689 || starIndex == 112185)
				return "Big Dipper";

			if (starIndex == 39801 || starIndex == 36861 || starIndex == 35468 || starIndex == 34085 ||
				starIndex == 38771 || starIndex == 36486 || starIndex == 37742 || starIndex == 37128)
				return "Orion";

			if (starIndex == 35497 || starIndex == 29139 || starIndex == 28305 || starIndex == 28319 ||
				starIndex == 27371 || starIndex == 25204 || starIndex == 21120 || starIndex == 37202)
				return "Taurus";

			if (starIndex == 148478 || starIndex == 158926 || starIndex == 159532 || starIndex == 143275 ||
				starIndex == 151680 || starIndex == 160578 || starIndex == 144217 || starIndex == 158408 ||
				starIndex == 149438 || starIndex == 143018 || starIndex == 147165 || starIndex == 161471 ||
				starIndex == 151890 || starIndex == 161892 || starIndex == 155203 || starIndex == 151985 ||
				starIndex == 152334)
				return "Scorpius";

			if (starIndex == 206778 || starIndex == 217906 || starIndex == 218045 || starIndex == 886 ||
				starIndex == 215182 || starIndex == 214923 || starIndex == 216131 || starIndex == 210418 ||
				starIndex == 210027 || starIndex == 215665 || starIndex == 206901 || starIndex == 215648 ||
				starIndex == 210459 || starIndex == 224427)
				return "Pegasus";

			if (starIndex == 5394 || starIndex == 3712 || starIndex == 432 || starIndex == 8538 ||
				starIndex == 11415 || starIndex == 4514 || starIndex == 4614 || starIndex == 3360)
				return "Cassiopeia";

			if (starIndex == 219615 || starIndex == 220954 || starIndex == 9270 || starIndex == 224617 ||
				starIndex == 222368 || starIndex == 10761 || starIndex == 12446 || starIndex == 6186 ||
				starIndex == 220954 || starIndex == 224935 || starIndex == 4656 || starIndex == 10380 ||
				starIndex == 217891 || starIndex == 222603 || starIndex == 7106 || starIndex == 28 ||
				starIndex == 11559 || starIndex == 7087 || starIndex == 7318 || starIndex == 7964 ||
				starIndex == 9138 || starIndex == 224533)
				return "Pisces";

			if (starIndex == 204867 || starIndex == 209750 || starIndex == 216627 || starIndex == 213051 ||
				starIndex == 218594 || starIndex == 216386 || starIndex == 198001 || starIndex == 212061 ||
				starIndex == 220321 || starIndex == 213998 || starIndex == 216032 || starIndex == 211391 ||
				starIndex == 219215 || starIndex == 219449 || starIndex == 209819 || starIndex == 219688)
				return "Aquarius";

			if (starIndex == 192876 || starIndex == 192947 || starIndex == 193495 || starIndex == 194943 ||
				starIndex == 197692 || starIndex == 198542 || starIndex == 204075 || starIndex == 205637 ||
				starIndex == 206453 || starIndex == 207098 || starIndex == 206088 || starIndex == 203387 ||
				starIndex == 200761 || starIndex == 196662 || starIndex == 195094)
				return "Capricornus";

			if (starIndex == 169022 || starIndex == 175191 || starIndex == 176687 || starIndex == 168454 ||
				starIndex == 169916 || starIndex == 178524 || starIndex == 165135 || starIndex == 167618 ||
				starIndex == 173300 || starIndex == 177716 || starIndex == 175775 || starIndex == 177241 ||
				starIndex == 166937 || starIndex == 181577 || starIndex == 181454 || starIndex == 181869 ||
				starIndex == 188114 || starIndex == 181623 || starIndex == 189103 || starIndex == 189763 ||
				starIndex == 181615 || starIndex == 161592)
				return "Sagittarius";

			if (starIndex == 135742 || starIndex == 130841 || starIndex == 133216 || starIndex == 139063 ||
				starIndex == 139365 || starIndex == 138905)
				return "Libra";

			if (starIndex == 116658 || starIndex == 114330 || starIndex == 110379 || starIndex == 107259 ||
				starIndex == 102870 || starIndex == 102212 || starIndex == 104979 || starIndex == 112300)
				return "Virgo";

			if (starIndex == 107259 || starIndex == 102870 || starIndex == 102212 || starIndex == 104979 ||
				starIndex == 112300)
				return "Leo";

			return "Unknown";
		}

		public bool CheckConstellationStarData(int starIndex)
		{

			bool isConstellationStar = false;

			//little dipper
			if (starIndex == 8890) isConstellationStar = true;  //polaris
			if (starIndex == 153751) isConstellationStar = true;  //urodelus
			if (starIndex == 131873) isConstellationStar = true;  //kochab
			if (starIndex == 137422) isConstellationStar = true;  //pherkad
			if (starIndex == 166205) isConstellationStar = true;  //yildun
			if (starIndex == 142105) isConstellationStar = true;  //zeta ursae minoris
			if (starIndex == 148048) isConstellationStar = true;  //eta ursae minoris
																  //big dipper
			if (starIndex == 103287) isConstellationStar = true;  //phekda
			if (starIndex == 120315) isConstellationStar = true;  //elkeid
			if (starIndex == 116842) isConstellationStar = true;  //alcor
			if (starIndex == 106591) isConstellationStar = true;  //megrez
			if (starIndex == 95418) isConstellationStar = true;  //merak
			if (starIndex == 95689) isConstellationStar = true;  //dubhe
			if (starIndex == 112185) isConstellationStar = true;  //alioth
																  // orion
			if (starIndex == 39801) isConstellationStar = true;  //betelgeuse
			if (starIndex == 36861) isConstellationStar = true;  //meissa
			if (starIndex == 35468) isConstellationStar = true;  //bellatrix
			if (starIndex == 34085) isConstellationStar = true;  //rigel
			if (starIndex == 38771) isConstellationStar = true;  //saiph
			if (starIndex == 36486) isConstellationStar = true;  //mintaka
			if (starIndex == 37742) isConstellationStar = true;  //alnitak
			if (starIndex == 37128) isConstellationStar = true;  //alnilam
																 //taurus
			if (starIndex == 35497) isConstellationStar = true;  //elnath
			if (starIndex == 29139) isConstellationStar = true;  //aldebaran
			if (starIndex == 28305) isConstellationStar = true;  //e taur
			if (starIndex == 28319) isConstellationStar = true;  //
			if (starIndex == 27371) isConstellationStar = true;  //
			if (starIndex == 25204) isConstellationStar = true;  //
			if (starIndex == 21120) isConstellationStar = true;  //
			if (starIndex == 37202) isConstellationStar = true;  //
																 //scorpius
			if (starIndex == 148478) isConstellationStar = true;  //
			if (starIndex == 158926) isConstellationStar = true;  //
			if (starIndex == 159532) isConstellationStar = true;  //
			if (starIndex == 143275) isConstellationStar = true;  //
			if (starIndex == 151680) isConstellationStar = true;  //
			if (starIndex == 160578) isConstellationStar = true;  //
			if (starIndex == 144217) isConstellationStar = true;  //
			if (starIndex == 158408) isConstellationStar = true;  //
			if (starIndex == 149438) isConstellationStar = true;  //
			if (starIndex == 143018) isConstellationStar = true;  //
			if (starIndex == 147165) isConstellationStar = true;  //
			if (starIndex == 161471) isConstellationStar = true;  //
			if (starIndex == 151890) isConstellationStar = true;  //
			if (starIndex == 161892) isConstellationStar = true;  //
			if (starIndex == 155203) isConstellationStar = true;  //
			if (starIndex == 151985) isConstellationStar = true;  //
			if (starIndex == 152334) isConstellationStar = true;  //
																  //pegasus
			if (starIndex == 206778) isConstellationStar = true;  //
			if (starIndex == 217906) isConstellationStar = true;  //
			if (starIndex == 218045) isConstellationStar = true;  //
			if (starIndex == 886) isConstellationStar = true;  //
			if (starIndex == 215182) isConstellationStar = true;  //
			if (starIndex == 214923) isConstellationStar = true;  //
			if (starIndex == 216131) isConstellationStar = true;  //
			if (starIndex == 210418) isConstellationStar = true;  //
			if (starIndex == 210027) isConstellationStar = true;  //
			if (starIndex == 215665) isConstellationStar = true;  //
			if (starIndex == 206901) isConstellationStar = true;  //
			if (starIndex == 215648) isConstellationStar = true;  //
			if (starIndex == 210459) isConstellationStar = true;  //
			if (starIndex == 224427) isConstellationStar = true;  //
																  //cassiopeia
			if (starIndex == 5394) isConstellationStar = true;  //
			if (starIndex == 3712) isConstellationStar = true;  //
			if (starIndex == 432) isConstellationStar = true;  //
			if (starIndex == 8538) isConstellationStar = true;  //
			if (starIndex == 11415) isConstellationStar = true;  //
			if (starIndex == 4514) isConstellationStar = true;  //
			if (starIndex == 4614) isConstellationStar = true;  //
			if (starIndex == 3360) isConstellationStar = true;  //
																//pisces
			if (starIndex == 219615) isConstellationStar = true;  //
			if (starIndex == 220954) isConstellationStar = true;  //
			if (starIndex == 9270) isConstellationStar = true;  //
			if (starIndex == 224617) isConstellationStar = true;  //
			if (starIndex == 222368) isConstellationStar = true;  //
			if (starIndex == 10761) isConstellationStar = true;  //
			if (starIndex == 12446) isConstellationStar = true;  //
			if (starIndex == 6186) isConstellationStar = true;  //
			if (starIndex == 220954) isConstellationStar = true;  //
			if (starIndex == 224935) isConstellationStar = true;  //
			if (starIndex == 4656) isConstellationStar = true;  //
			if (starIndex == 10380) isConstellationStar = true;  //
			if (starIndex == 217891) isConstellationStar = true;  //
			if (starIndex == 222603) isConstellationStar = true;  //
			if (starIndex == 7106) isConstellationStar = true;  //
			if (starIndex == 28) isConstellationStar = true;  //
			if (starIndex == 11559) isConstellationStar = true;  //
			if (starIndex == 7087) isConstellationStar = true;  //
			if (starIndex == 7318) isConstellationStar = true;  //
			if (starIndex == 7964) isConstellationStar = true;  //
			if (starIndex == 9138) isConstellationStar = true;  //
			if (starIndex == 224533) isConstellationStar = true;  //
																  //aquarius
			if (starIndex == 204867) isConstellationStar = true;  //
			if (starIndex == 209750) isConstellationStar = true;  //
			if (starIndex == 216627) isConstellationStar = true;  //
			if (starIndex == 213051) isConstellationStar = true;  //
			if (starIndex == 218594) isConstellationStar = true;  //
			if (starIndex == 216386) isConstellationStar = true;  //
			if (starIndex == 198001) isConstellationStar = true;  //
			if (starIndex == 212061) isConstellationStar = true;  //
			if (starIndex == 220321) isConstellationStar = true;  //
			if (starIndex == 213998) isConstellationStar = true;  //
			if (starIndex == 216032) isConstellationStar = true;  //
			if (starIndex == 211391) isConstellationStar = true;  //
			if (starIndex == 219215) isConstellationStar = true;  //
			if (starIndex == 219449) isConstellationStar = true;  //
			if (starIndex == 209819) isConstellationStar = true;  //
			if (starIndex == 219688) isConstellationStar = true;  //
																  //capricornus
			if (starIndex == 192876) isConstellationStar = true;  //
			if (starIndex == 192947) isConstellationStar = true;  //
			if (starIndex == 193495) isConstellationStar = true;  //
			if (starIndex == 194943) isConstellationStar = true;  //
			if (starIndex == 197692) isConstellationStar = true;  //
			if (starIndex == 198542) isConstellationStar = true;  //
			if (starIndex == 204075) isConstellationStar = true;  //
			if (starIndex == 205637) isConstellationStar = true;  //
			if (starIndex == 206453) isConstellationStar = true;  //
			if (starIndex == 207098) isConstellationStar = true;  //
			if (starIndex == 206088) isConstellationStar = true;  //
			if (starIndex == 203387) isConstellationStar = true;  //
			if (starIndex == 200761) isConstellationStar = true;  //
			if (starIndex == 196662) isConstellationStar = true;  //
			if (starIndex == 195094) isConstellationStar = true;  //
																  //sagittarius
			if (starIndex == 169022) isConstellationStar = true;  //
			if (starIndex == 175191) isConstellationStar = true;  //
			if (starIndex == 176687) isConstellationStar = true;  //
			if (starIndex == 168454) isConstellationStar = true;  //
			if (starIndex == 169916) isConstellationStar = true;  //
			if (starIndex == 178524) isConstellationStar = true;  //
			if (starIndex == 165135) isConstellationStar = true;  //
			if (starIndex == 167618) isConstellationStar = true;  //
			if (starIndex == 173300) isConstellationStar = true;  //
			if (starIndex == 177716) isConstellationStar = true;  //
			if (starIndex == 175775) isConstellationStar = true;  //
			if (starIndex == 177241) isConstellationStar = true;  //
			if (starIndex == 166937) isConstellationStar = true;  //
			if (starIndex == 181577) isConstellationStar = true;  //
			if (starIndex == 181454) isConstellationStar = true;  //
			if (starIndex == 181869) isConstellationStar = true;  //
			if (starIndex == 188114) isConstellationStar = true;  //
			if (starIndex == 181623) isConstellationStar = true;  //
			if (starIndex == 189103) isConstellationStar = true;  //
			if (starIndex == 189763) isConstellationStar = true;  //
			if (starIndex == 181615) isConstellationStar = true;  //
			if (starIndex == 161592) isConstellationStar = true;  //
																  //libra
			if (starIndex == 135742) isConstellationStar = true;  //
			if (starIndex == 130841) isConstellationStar = true;  //
			if (starIndex == 133216) isConstellationStar = true;  //
			if (starIndex == 139063) isConstellationStar = true;  //
			if (starIndex == 139365) isConstellationStar = true;  //
			if (starIndex == 138905) isConstellationStar = true;  //
																  //virgo
			if (starIndex == 116658) isConstellationStar = true;  //
			if (starIndex == 114330) isConstellationStar = true;  //
			if (starIndex == 110379) isConstellationStar = true;  //
			if (starIndex == 107259) isConstellationStar = true;  //
			if (starIndex == 102870) isConstellationStar = true;  //
			if (starIndex == 102212) isConstellationStar = true;  //
			if (starIndex == 104979) isConstellationStar = true;  //
			if (starIndex == 112300) isConstellationStar = true;  //
			if (starIndex == 113226) isConstellationStar = true;  //
			if (starIndex == 118098) isConstellationStar = true;  //
			if (starIndex == 122408) isConstellationStar = true;  //
			if (starIndex == 124850) isConstellationStar = true;  //
			if (starIndex == 129502) isConstellationStar = true;  //
																  //leo
			if (starIndex == 87901) isConstellationStar = true;  //regulus
			if (starIndex == 87737) isConstellationStar = true;  //
			if (starIndex == 89484) isConstellationStar = true;  //algieba
			if (starIndex == 89025) isConstellationStar = true;  //
			if (starIndex == 85503) isConstellationStar = true;  //
			if (starIndex == 84441) isConstellationStar = true;  //
			if (starIndex == 97603) isConstellationStar = true;  //
			if (starIndex == 102647) isConstellationStar = true;  //
			if (starIndex == 97633) isConstellationStar = true;  //
																 //leo minor
			if (starIndex == 94264) isConstellationStar = true;  //
			if (starIndex == 90537) isConstellationStar = true;  //
			if (starIndex == 87696) isConstellationStar = true;  //
			if (starIndex == 82635) isConstellationStar = true;  //
			if (starIndex == 92125) isConstellationStar = true;  //
			if (starIndex == 90277) isConstellationStar = true;  //
																 //cancer
			if (starIndex == 69267) isConstellationStar = true;  //
			if (starIndex == 42911) isConstellationStar = true;  //
			if (starIndex == 74198) isConstellationStar = true;  //
			if (starIndex == 74739) isConstellationStar = true;  //
			if (starIndex == 76756) isConstellationStar = true;  //
																 //gemini
			if (starIndex == 62509) isConstellationStar = true;  //
			if (starIndex == 60179) isConstellationStar = true;  //
			if (starIndex == 62345) isConstellationStar = true;  //
			if (starIndex == 45542) isConstellationStar = true;  //
			if (starIndex == 58207) isConstellationStar = true;  //
			if (starIndex == 56986) isConstellationStar = true;  //
			if (starIndex == 56537) isConstellationStar = true;  //
			if (starIndex == 52973) isConstellationStar = true;  //
			if (starIndex == 31681) isConstellationStar = true;  //
			if (starIndex == 48737) isConstellationStar = true;  //
			if (starIndex == 54719) isConstellationStar = true;  //
			if (starIndex == 50019) isConstellationStar = true;  //
			if (starIndex == 48329) isConstellationStar = true;  //
			if (starIndex == 257937) isConstellationStar = true;  //
			if (starIndex == 44478) isConstellationStar = true;  //
																 //draco
			if (starIndex == 164058) isConstellationStar = true;  //
			if (starIndex == 159181) isConstellationStar = true;  //
			if (starIndex == 163588) isConstellationStar = true;  //
			if (starIndex == 182564) isConstellationStar = true;  //
			if (starIndex == 188119) isConstellationStar = true;  //
			if (starIndex == 175306) isConstellationStar = true;  //
			if (starIndex == 170153) isConstellationStar = true;  //
			if (starIndex == 160922) isConstellationStar = true;  //
			if (starIndex == 155763) isConstellationStar = true;  //
			if (starIndex == 148387) isConstellationStar = true;  //
			if (starIndex == 144284) isConstellationStar = true;  //
			if (starIndex == 137759) isConstellationStar = true;  //
			if (starIndex == 123299) isConstellationStar = true;  //
			if (starIndex == 109387) isConstellationStar = true;  //
			if (starIndex == 100029) isConstellationStar = true;  //
			if (starIndex == 85819) isConstellationStar = true;  //

			//----
			if (starIndex == 4128) isConstellationStar = true;  //deneb kaitos
			if (starIndex == 14386) isConstellationStar = true;  //mira
			if (starIndex == 18884) isConstellationStar = true;  //menkar
			if (starIndex == 12929) isConstellationStar = true;  //hamal



			return isConstellationStar;

		}







	}
}