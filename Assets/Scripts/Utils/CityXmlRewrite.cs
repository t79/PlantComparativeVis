using UnityEngine;
using System.Collections;
using System.Xml;
using System.IO;

public class CityXmlRewrite : MonoBehaviour
{
	public string ensemblePath    = @"Ensemble.city\ensemble.xml";
	public string newEnsemblePath = @"Ensemble.city\ensemble3D.xml";


	private string xmlString = @"<?xml version='1.0'?>
<ensemble phenomenon='City emergence' dimension='3' type='mesh' test='city'>
	<parameters>
		<parameter name='density'  minValue='0.5' maxValue='0.9' count='5' />
		<parameter name='ratio'    minValue='1'   maxValue='1.8' count='5' />
		<parameter name='exponent' minValue='2'   maxValue='6'   count='5' />
	</parameters>
	<structures>";

	// Use this for initialization
	void Start ()
	{
		XmlDocument doc = new XmlDocument ();
		doc.Load (ensemblePath);
		XmlNode ensemble = doc.LastChild;
		
		if (ensemble != null)
		{
			foreach(XmlNode structureXml in ensemble.ChildNodes)
			{
				if(structureXml == ensemble.LastChild)
					continue;

				XmlNode positionNode = structureXml.FirstChild;
				
				float structureX   = System.Convert.ToSingle(positionNode.Attributes["x"].Value);
				float structureY   = System.Convert.ToSingle(positionNode.Attributes["y"].Value);
				float structureZ   = System.Convert.ToSingle(positionNode.Attributes["z"].Value);
				
				transform.position = new Vector3(structureX, structureY, structureZ);
				
				// city parameters
				XmlNode imageNode    = positionNode.NextSibling;
				string  imageStr     = imageNode.Attributes["src"].Value;
				string  imageDenStr  = imageNode.NextSibling.Attributes["src"].Value;
				
				string[] informationParts = imageStr.Split('_');
				informationParts[3] = informationParts[3].Substring(0, informationParts[3].IndexOf('.'));
				
				// city structure
				XmlNode stringNode = structureXml.LastChild;
				string stringValue = stringNode.Attributes["value"].Value;
				
				xmlString +=@"
		<structure id='" + informationParts[0] + @"'>
			<parameters>
				<parameter name='density'  value='" + informationParts[1] + @"' />
				<parameter name='ratio'    value='" + informationParts[2] + @"' />
				<parameter name='exponent' value='" + informationParts[3] + @"' />
			</parameters>
			<data>
				<vector3 name='position' x='" + structureX + @"' y='" + structureY + @"' z='" + structureZ + @"' />
				<image type='road'    src='" + imageStr    + @"' />
				<image type='density' src='" + imageDenStr + @"' />
				<string>
" + stringValue + @"
				</string>
			</data>
		</structure>";
			}
		}

		xmlString += @"
	</structures>
</ensemble>";

		StreamWriter writer; 
		FileInfo t = new FileInfo(newEnsemblePath);
		
		if(!t.Exists) 
		{ 
			writer = t.CreateText();
		} 
		else 
		{ 
			t.Delete(); 
			writer = t.CreateText(); 
		} 
		writer.Write(xmlString);
		writer.Close();
	}
}
