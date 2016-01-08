<Query Kind="Program">
  <NuGetReference>EntityFramework</NuGetReference>
  <NuGetReference>MathNet.Numerics</NuGetReference>
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>MathNet.Numerics.Random</Namespace>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>Newtonsoft.Json.Linq</Namespace>
</Query>

void Main()
{
	ResGen().ToString().Dump();
}

private JToken ExGen1()
{
	var mt = new MersenneTwister();
	var l = new JArray();
	for (var i = 0; i < 200; i++)
	{
		var cc = resVal[mt.Next(resVal.Length)];
		var it = new JObject{
			{"id", $"R{i+1}"},
			{"value", cc},
			{"package", "RES40"}
		};
		l.Add(it);
	}

	var r = new JObject { { "components", l}};
	return r;
}

class ResInfo
{
	public ResInfo(double ipd, double w)
	{
		InterPinDist = ipd;
		Height = w;
	}

	public double InterPinDist { get; }
	public double Height { get; }
}

class CapInfo
{
	public CapInfo(double ip, double r)
	{
		InterPinDist = ip;
		CircleRadius = r;
	}

	public double InterPinDist { get; }
	public double CircleRadius { get; }
}

string[] resVal = new[]
		   {
				  "1R",  "10R", "100R", "1K",  "10K", "100K", "1M"
				, "1R1", "11R", "110R", "1K1", "11K", "110K", "1M1"
				, "1R2", "12R", "120R", "1K2", "12K", "120K", "1M2"
				, "1R3", "13R", "130R", "1K3", "13K", "130K", "1M3"
				, "1R5", "15R", "150R", "1K5", "15K", "150K", "1M5"
				, "1R6", "16R", "160R", "1K6", "16K", "160K", "1M6"
				, "1R8", "18R", "180R", "1K8", "18K", "180K", "1M8"
				, "2R",  "20R", "200R", "2K",  "20K", "200K", "2M"
				, "2R2", "22R", "220R", "2K2", "22K", "220K", "2M2"
				, "2R4", "24R", "240R", "2K4", "24K", "240K", "2M4"
				, "2R7", "27R", "270R", "2K7", "27K", "270K", "2M7"
				, "3R",  "30R", "300R", "3K",  "30K", "300K", "3M"
				, "3R3", "33R", "330R", "3K3", "33K", "330K", "3M3"
				, "3R6", "36R", "360R", "3K6", "36K", "360K", "3M6"
				, "3R9", "39R", "390R", "3K9", "39K", "390K", "3M9"
				, "4R3", "43R", "430R", "4K3", "43K", "430K", "4M3"
				, "4R7", "47R", "470R", "4K7", "47K", "470K", "4M7"
				, "5R1", "51R", "510R", "5K1", "51K", "510K", "5M1"
				, "5R6", "56R", "560R", "5K6", "56K", "560K", "5M6"
				, "6R2", "62R", "620R", "6K2", "62K", "620K", "6M2"
				, "6R8", "68R", "680R", "6K8", "68K", "680K", "6M8"
				, "7R5", "75R", "750R", "7K5", "75K", "750K", "7M5"
				, "8R2", "82R", "820R", "8K2", "82K", "820K", "8M2"
				, "9R1", "91R", "910R", "9K1", "91K", "910K", "9M1"
			};

Dictionary<string, ResInfo>  resSizes = new Dictionary<string, ResInfo>
{
	["RES40"] = new ResInfo(0.4, 0.05),
	["RES50"] = new ResInfo(0.5, 0.05),
	["RES60"] = new ResInfo(0.6, 0.1),
	["RES90"] = new ResInfo(0.9, 0.1)
};

private JToken ResGen()
{
	var compValues = resVal;

	var packs = new JArray();
	foreach (var size in resSizes)
	{
		var ip = Math.Round(size.Value.InterPinDist * 25.4, 4);
		var h = Math.Round(size.Value.Height * 25.4, 4);
		var ir = Math.Round((size.Value.InterPinDist - 0.2) * 25.4, 4);
		var il = Math.Round((size.Value.InterPinDist - 0.1) * 25.4, 4);

		var hip = 0.5 * ip;
		var hh = 0.5 * h;
		var hir = 0.5 * ir;
		var hil = 0.5 * il;

		var bounds = new JArray(hh, hip, -hh, -hip);
		var bl = new JArray(-hir, -hh);
		var draws = new JArray(new object[]
		{
					new JObject
					{
						{"type", "rectangle"},
						{"position",  bl},
						{"width", ir },
						{"height", h }
					},
					new JObject
					{
						{"type", "line"},
						{"p1", new JArray {-hil, 0 }},
						{"p2", new JArray {-hir, 0 }}
					},
					new JObject
					{
						{"type", "line" },
						{"p1", new JArray(hil, 0) },
						{"p2", new JArray(hir, 0) }
					}
		});

		var pins = new JArray(
			new JObject
			{
						{"name", "P1" },
						{"position", new JArray(-hip, 0) }
			},
			new JObject
			{
						{"name", "P2" },
						{"position", new JArray(hip, 0) }
			}
		);

		var pack = new JObject
				{
					{"name", size.Key},
					{"boundaries", bounds },
					{"footprint", draws },
					{"pins", pins }
				};


		packs.Add(pack);
	}

	var o = new JObject
			{
				{"name", new JValue("resistors")},
				{"packages", packs }
			};

	return o;
}

private JToken CapGen()
{
	var vals = new[]
			{
				"10P", "100P", "1N",  "10N", "100N", "1U",  "10U", "100U", "1000U",
				"12P", "120P", "1N2", "12N", "120N", "1U2", "12U", "120U", "1200U",
				"15P", "150P", "1N5", "15N", "150N", "1U5",
				"18P", "180P", "1N8", "18N", "180N", "1U8",
				"22P", "220P", "2N2", "22N", "220N", "2U2", "22U", "220U", "2200U",
				"27P", "270P", "2N7", "27N", "270N", "2U7",
				"33P", "330P", "3N3", "33N", "330N", "3U3", "33U", "330U", "3300U",
				"39P", "390P", "3N9", "39N", "390N", "3U9",
				"47P", "470P", "4N7", "47N", "470N", "4U7", "47U", "470U", "4700U",
				"56P", "560P", "5N6", "56N", "560N", "5U6",
				"68P", "680P", "6N8", "68N", "680N", "6U8", "68U", "680U", "6800U",
				"82P", "820P", "8N2", "82N", "820N", "8U2"
			};

	var sizes = new Dictionary<string, CapInfo>
	{
		["ELEC-RAD10"] = new CapInfo(0.1, 0.1),
		["ELEC-RAD13"] = new CapInfo(0.13, 0.13),
		["ELEC-RAD20"] = new CapInfo(0.2, 0.2),
		["ELEC-RAD25"] = new CapInfo(0.25, 0.25),
		["ELEC-RAD30"] = new CapInfo(0.3, 0.3),
		["ELEC-RAD40"] = new CapInfo(0.4, 0.5)
	};

	var packs = new JArray();
	foreach (var size in sizes)
	{
		var ip = Math.Round(size.Value.InterPinDist * 25.4, 4);
		var r = Math.Round(size.Value.CircleRadius * 25.4, 4);
		var hip = 0.5 * ip;


		var bounds = new JArray(r, r, -r, -r);
		var draws = new JArray(new object[]
		{
					new JObject
					{
						{"type", "circle"},
						{"center", new JArray(0, 0)},
						{"radius", r }
					}
		});

		var pins = new JArray(
			new JObject
			{
						{"name", "P+" },
						{"position", new JArray(-hip, 0) }
			},
			new JObject
			{
						{"name", "P-" },
						{"position", new JArray(hip, 0) }
			}
		);

		var pack = new JObject
				{
					{"name", size.Key},
					{"boundaries", bounds },
					{"footprint", draws },
					{"pins", pins }
				};


		packs.Add(pack);
	}

	var o = new JObject
			{
				{"name", new JValue("electrolitic_capacitors")},
				{"packages", packs }
			};

	return o;
}