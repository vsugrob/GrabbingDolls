using UnityEngine;
using System.Collections;
using System.Linq;

public class ColorController : MonoBehaviour {
	public Color Color = new Color ( 0.5f, 0.75f, 1 );
	public string IncludeTag;
	public string ExcludeTag;
	private MeshRenderer [] meshRenderers;

	private Color prevColor;
	
	void Start () {
		meshRenderers = GetComponentsInChildren <MeshRenderer> ();

		if ( !string.IsNullOrEmpty ( IncludeTag ) )
			meshRenderers = meshRenderers.Where ( r => r.CompareTag ( IncludeTag ) ).ToArray ();

		if ( !string.IsNullOrEmpty ( ExcludeTag ) )
			meshRenderers = meshRenderers.Where ( r => !r.CompareTag ( ExcludeTag ) ).ToArray ();

		SetColor ( Color );
	}

	void Update () {
		if ( Color != prevColor ) {
			SetColor ( Color );
			prevColor = Color;
		}
	}

	private void SetColor ( Color color ) {
		foreach ( var renderer in meshRenderers ) {
			renderer.material.color = color;
		}
	}
}
