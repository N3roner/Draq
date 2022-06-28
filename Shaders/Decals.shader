Shader "Custom/Decals" {
	Properties {

		//Diffuse
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Diffuse (RGB)", 2D) = "white" {}		//ostat ce
		_DecalTex("Decal", 2D) = "white" {}		 // dekal textura odvojena, ici ce umjesto emission tex
		
		//Specular
		_SpecTex("Specular (RGB)", 2D) = "white" {}		//po starom, bit ce Spec(R) Gloss(A) Emmission(B) kad se srede texture
		_Glossiness("Smoothness", Range(0,2)) = 0.0
		_Metallic("Spec Metallic", Range(0,2)) = 0.0

		//Emission
		_EmissTex("Emission (RGB)", 2D) = "white" {}	//posebna tex, ici ce na spec tex (B)
		_Emission("Emission illumination", Range(0,1)) = 0
		
	}
	SubShader {

		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex : TEXCOORD1;
		sampler2D _DecalTex : TEXCOORD2;
		sampler2D _SpecTex;
		sampler2D _EmissTex;


		struct Input {

			float2 uv_MainTex;
			float2 uv2_DecalTex;

		};


		half _Glossiness;
		half _Metallic;
		half _Emission;
		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutputStandard o) {

			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
			fixed4 d = tex2D (_DecalTex, IN.uv2_DecalTex);

			fixed4 s = tex2D(_SpecTex, IN.uv_MainTex);

			fixed4 e = tex2D(_EmissTex, IN.uv_MainTex);

			//o.Albedo = lerp(c.rgb * e.rgb, c.rgb * _Color, c.a * e.a);	//decal pod emission tex
			o.Albedo = lerp(c.rgb * d.rgb, c.rgb * _Color, c.a * d.a);	//dec zasebno tex

			// Metallic and smoothness come from slider variables
			o.Metallic = lerp(_Metallic, s.rgb, s.a);
			//o.Metallic = lerp(_Metallic, s.r, s.a);		//spec za tex po kanalima
			o.Smoothness = s.a * _Glossiness;

			//o.Emission = s.b * _Emission * c.rgb; //emission za spec (B) i boja od diffuse
			o.Emission = e.rgb * _Emission;
			
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
