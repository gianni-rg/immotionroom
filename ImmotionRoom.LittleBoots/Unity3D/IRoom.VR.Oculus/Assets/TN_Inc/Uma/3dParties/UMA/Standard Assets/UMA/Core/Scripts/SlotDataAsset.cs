using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;
using System.Collections.Generic;

namespace UMA
{
	/// <summary>
	/// Contains the immutable data shared between slots of the same type.
	/// </summary>
	[System.Serializable]
	public partial class SlotDataAsset : ScriptableObject, ISerializationCallbackReceiver
	{
		public string slotName;
		[System.NonSerialized]
		public int nameHash;

		/// <summary>
		/// The UMA material.
		/// </summary>
		/// <remarks>
		/// The UMA material contains both a reference to the Unity material
		/// used for drawing and information needed for matching the textures
		/// and colors to the various material properties.
		/// </remarks>
		[UMAAssetFieldVisible]
		public UMAMaterial material;

		/// <summary>
		/// Default overlay scale for slots using the asset.
		/// </summary>
		public float overlayScale = 1.0f;
		/// <summary>
		/// The animated bone names.
		/// </summary>
		/// <remarks>
		/// The animated bones array is required for cases where optimizations
		/// could remove transforms from the rig. Animated bones will always
		/// be preserved.
		/// </remarks>
		public string[] animatedBoneNames = new string[0];
		/// <summary>
		/// The animated bone name hashes.
		/// </summary>
		/// <remarks>
		/// The animated bones array is required for cases where optimizations
		/// could remove transforms from the rig. Animated bones will always
		/// be preserved.
		/// </remarks>
		[UnityEngine.HideInInspector]
		public int[] animatedBoneHashes = new int[0];

		/// <summary>
		/// Optional DNA converter specific to the slot.
		/// </summary>
		public DnaConverterBehaviour slotDNA;
		/// <summary>
		/// The mesh data.
		/// </summary>
		/// <remarks>
		/// The UMAMeshData contains all of the Unity mesh data and additional
		/// information needed for mesh manipulation while minimizing overhead
		/// from accessing Unity's managed memory.
		/// </remarks>
		public UMAMeshData meshData;
		public int subMeshIndex;
		/// <summary>
		/// Use this to identify slots that serves the same purpose
		/// Eg. ChestArmor, Helmet, etc.
		/// </summary>
		public string slotGroup;
		/// <summary>
		/// Use this to identify what kind of overlays fit this slotData
		/// Eg. BaseMeshSkin, BaseMeshOverlays, GenericPlateArmor01
		/// </summary>
		public string[] tags;

		/// <summary>
		/// Callback event when character update begins.
		/// </summary>
		public UMADataEvent CharacterBegun;
		/// <summary>
		/// Callback event when slot overlays are atlased.
		/// </summary>
		public UMADataSlotMaterialRectEvent SlotAtlassed;
		/// <summary>
		/// Callback event when character DNA is applied.
		/// </summary>
		public UMADataEvent DNAApplied;
		/// <summary>
		/// Callback event when character update is complete.
		/// </summary>
		public UMADataEvent CharacterCompleted;

		[UnityEngine.HideInInspector]
		[System.Obsolete("UMA 2.1 - SlotDataAsset.animatedBones array is obsolete use animatedBoneNames or animatedBoneHashes instead", false)]
		public Transform[] animatedBones = new Transform[0];


		public SlotDataAsset()
		{
            
		}

		public int GetTextureChannelCount(UMAGeneratorBase generator)
		{
			return material.channels.Length;
		}
        
		public override string ToString()
		{
			return "SlotData: " + slotName;
		}

		public void UpdateMeshData(SkinnedMeshRenderer meshRenderer)
		{
			meshData = new UMAMeshData();
			meshData.RetrieveDataFromUnityMesh(meshRenderer);
#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(this);
#endif
		}
#if UNITY_EDITOR
		
		public void UpdateMeshData()
		{
		}
#endif
		public void OnAfterDeserialize()
		{
			nameHash = UMAUtils.StringToHash(slotName);
		}
		public void OnBeforeSerialize() { }

		public void Assign(SlotDataAsset source)
		{
			slotName = source.slotName;
			nameHash = source.nameHash;
			material = source.material;
			overlayScale = source.overlayScale;
			animatedBoneNames = source.animatedBoneNames;
			animatedBoneHashes = source.animatedBoneHashes;
			meshData = source.meshData;
			subMeshIndex = source.subMeshIndex;
			slotGroup = source.slotGroup;
			tags = source.tags;
		}
	}
}
