public class Characteristic
{
	public string      name;
	public Measurement measurement;
	public float       weight;

	public Characteristic()
	{
		name        = "";
		measurement = null;
		weight      = 0;
	}

	public Characteristic(string nName)
	{
		name        = nName;
		measurement = null;
		weight      = 0;
	}

	public Characteristic(string nName, Measurement nMeasurement)
	{
		name        = nName;
		measurement = nMeasurement;
		weight      = 0;
	}

	public Characteristic(string nName, Measurement nMeasurement, float nWeight)
	{
		name        = nName;
		measurement = nMeasurement;
		weight      = nWeight;
	}

	public float getMeasurementValue(Representation representation)
	{
		return measurement.Evaluate (representation);
	}
}
