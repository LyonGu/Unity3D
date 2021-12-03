//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2015 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Sprite is a textured element in the UI hierarchy.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/NGUI Sprite")]
public class UISprite
{
	// Cached and saved values
	[HideInInspector][SerializeField] UIAtlas mAtlas;
	[HideInInspector][SerializeField] string mSpriteName;

	// Deprecated, no longer used
	[HideInInspector][SerializeField] bool mFillCenter = true;

	[System.NonSerialized] protected UISpriteData mSprite;
	[System.NonSerialized] bool mSpriteSet = false;

	/// <summary>
	/// Retrieve the material used by the font.
	/// </summary>

	public  Material material { get { return (mAtlas != null) ? mAtlas.spriteMaterial : null; } }

	/// <summary>
	/// Atlas used by this widget.
	/// </summary>
 
	public UIAtlas atlas
	{
		get
		{
			return mAtlas;
		}
		set
		{
			if (mAtlas != value)
			{
				

				mAtlas = value;
				mSpriteSet = false;
				mSprite = null;

				// Automatically choose the first sprite
				if (string.IsNullOrEmpty(mSpriteName))
				{
					if (mAtlas != null && mAtlas.spriteList.Count > 0)
					{
						SetAtlasSprite(mAtlas.spriteList[0]);
						mSpriteName = mSprite.name;
					}
				}

				// Re-link the sprite
				if (!string.IsNullOrEmpty(mSpriteName))
				{
					string sprite = mSpriteName;
					mSpriteName = "";
					spriteName = sprite;
					
				}
			}
		}
	}

	/// <summary>
	/// Sprite within the atlas used to draw this widget.
	/// </summary>
 
	public string spriteName
	{
		get
		{
			return mSpriteName;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				// If the sprite name hasn't been set yet, no need to do anything
				if (string.IsNullOrEmpty(mSpriteName)) return;

				// Clear the sprite name and the sprite reference
				mSpriteName = "";
				mSprite = null;
				//mChanged = true;
				mSpriteSet = false;
			}
			else if (mSpriteName != value)
			{
				// If the sprite name changes, the sprite reference should also be updated
				mSpriteName = value;
				mSprite = null;
				//mChanged = true;
				mSpriteSet = false;
			}
		}
	}

	/// <summary>
	/// Is there a valid sprite to work with?
	/// </summary>

	public bool isValid { get { return GetAtlasSprite() != null; } }

	/// <summary>
	/// Whether the center part of the sprite will be filled or not. Turn it off if you want only to borders to show up.
	/// </summary>



	/// <summary>
	/// Sliced sprites generally have a border. X = left, Y = bottom, Z = right, W = top.
	/// </summary>

	public Vector4 border
	{
		get
		{
			UISpriteData sp = GetAtlasSprite();
			return new Vector4(sp.borderLeft, sp.borderBottom, sp.borderRight, sp.borderTop);
		}
	}

	/// <summary>
	/// Size of the pixel -- used for drawing.
	/// </summary>

	 public float pixelSize { get { return mAtlas != null ? mAtlas.pixelSize : 1f; } }

	/// <summary>
	/// Minimum allowed width for this widget.
	/// </summary>



	/// <summary>
	/// Minimum allowed height for this widget.
	/// </summary>

	 

	/// <summary>
	/// Sprite's dimensions used for drawing. X = left, Y = bottom, Z = right, W = top.
	/// This function automatically adds 1 pixel on the edge if the sprite's dimensions are not even.
	/// It's used to achieve pixel-perfect sprites even when an odd dimension sprite happens to be centered.
	/// </summary>

	

	/// <summary>
	/// Whether the texture is using a premultiplied alpha material.
	/// </summary>

	public  bool premultipliedAlpha { get { return (mAtlas != null) && mAtlas.premultipliedAlpha; } }

	/// <summary>
	/// Retrieve the atlas sprite referenced by the spriteName field.
	/// </summary>

	public UISpriteData GetAtlasSprite ()
	{
		if (!mSpriteSet) mSprite = null;

		if (mSprite == null && mAtlas != null)
		{
			if (!string.IsNullOrEmpty(mSpriteName))
			{
				UISpriteData sp = mAtlas.GetSprite(mSpriteName);
				if (sp == null) return null;
				SetAtlasSprite(sp);
			}

			if (mSprite == null && mAtlas.spriteList.Count > 0)
			{
				UISpriteData sp = mAtlas.spriteList[0];
				if (sp == null) return null;
				SetAtlasSprite(sp);

				if (mSprite == null)
				{
					Debug.LogError(mAtlas.name + " seems to have a null sprite!");
					return null;
				}
				mSpriteName = mSprite.name;
			}
		}
		return mSprite;
	}

	/// <summary>
	/// Set the atlas sprite directly.
	/// </summary>

	protected void SetAtlasSprite (UISpriteData sp)
	{
		//mChanged = true;
		mSpriteSet = true;

		if (sp != null)
		{
			mSprite = sp;
			mSpriteName = mSprite.name;
		}
		else
		{
			mSpriteName = (mSprite != null) ? mSprite.name : "";
			mSprite = sp;
		}
	}

	/// <summary>
	/// Adjust the scale of the widget to make it pixel-perfect.
	/// </summary>

	//public void MakePixelPerfect ()
	//{
	//	if (!isValid) return;
	//	base.MakePixelPerfect();
	//	if (mType == Type.Tiled) return;

	//	UISpriteData sp = GetAtlasSprite();
	//	if (sp == null) return;

	//	Texture tex = mainTexture;
	//	if (tex == null) return;

	//	if (mType == Type.Simple || mType == Type.Filled || !sp.hasBorder)
	//	{
	//		if (tex != null)
	//		{
	//			int x = Mathf.RoundToInt(pixelSize * (sp.width + sp.paddingLeft + sp.paddingRight));
	//			int y = Mathf.RoundToInt(pixelSize * (sp.height + sp.paddingTop + sp.paddingBottom));
				
	//			if ((x & 1) == 1) ++x;
	//			if ((y & 1) == 1) ++y;

	//			width = x;
	//			height = y;
	//		}
	//	}
	//}

	/// <summary>
	/// Auto-upgrade.
	/// </summary>

//	protected override void OnInit ()
//	{
//		if (!mFillCenter)
//		{
//			mFillCenter = true;
//			centerType = AdvancedType.Invisible;
//#if UNITY_EDITOR
//			NGUITools.SetDirty(this);
//#endif
//		}
//		base.OnInit();
//	}

	/// <summary>
	/// Update the UV coordinates.
	/// </summary>



	/// <summary>
	/// Virtual function called by the UIPanel that fills the buffers.
	/// </summary>

	//public override void OnFill (BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color32> cols)
	//{
	//	Texture tex = mainTexture;
	//	if (tex == null) return;

	//	if (mSprite == null) mSprite = atlas.GetSprite(spriteName);
	//	if (mSprite == null) return;

	//	Rect outer = new Rect(mSprite.x, mSprite.y, mSprite.width, mSprite.height);
	//	Rect inner = new Rect(mSprite.x + mSprite.borderLeft, mSprite.y + mSprite.borderTop,
	//		mSprite.width - mSprite.borderLeft - mSprite.borderRight,
	//		mSprite.height - mSprite.borderBottom - mSprite.borderTop);

	//	outer = NGUIMath.ConvertToTexCoords(outer, tex.width, tex.height);
	//	inner = NGUIMath.ConvertToTexCoords(inner, tex.width, tex.height);

	//	int offset = verts.size;
	//	Fill(verts, uvs, cols, outer, inner);

	//	if (onPostFill != null)
	//		onPostFill(this, offset, verts, uvs, cols);
	//}
}
