/************************************************************************************************************
 * 
 * Copyright (C) 2014-2016 ImmotionAR, a division of Beps Engineering. All rights reserved.
 * 
 * Licensed under the ImmotionAR ImmotionRoom SDK License (the "License");
 * you may not use the ImmotionAR ImmotionRoom SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 * 
 * You may obtain a copy of the License at
 * 
 * http://www.immotionar.com/legal/ImmotionRoomSDKLicense.PDF
 * 
 ************************************************************************************************************/
namespace ImmotionAR.ImmotionRoom.LittleBoots.Avateering.Uma.Generators
{
    using UnityEngine;
    using System.Collections;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using System.Collections.Generic;
    using UMA;
    using ImmotionAR.ImmotionRoom.LittleBoots.Avateering;

    /// <summary>
    /// Generates a UMA 2 avatar, with randomly extracted features from the provided UMA Slots, races and overlays
    /// </summary>
    /// <remarks>
    /// This class source code is heavily based on code used inside UMA package UMACrowd class
    /// </remarks>
    public class RandomUmaGenerator : UmaBodyGenerator
    {
        #region Constants

        /// <summary>
        /// Name of the resource containing a standard UMA Avatar in T pose
        /// </summary>
        private string TAvatarResourceName = "Uma_T_Avatar";

        #endregion

        #region Unity public properties

        /// <summary>
        /// Sets of randomizable UMA features to construct the random avatars
        /// </summary>
        [Tooltip("Sets of randomizable UMA features to construct the random avatars")]
        public UMACrowdRandomSet[] RandomFeaturesPool;

        /// <summary>
        /// Additional UMA Recipes to construct the random avatars (can be empty)
        /// </summary>
        [Tooltip("Additional UMA Recipes to construct the random avatars")]
        public UMARecipeBase[] AdditionalRecipes;

        #endregion

        #region Private fields

        /// <summary>
        /// UMA generator
        /// </summary>
        UMAGenerator m_umaGenerator;

        /// <summary>
        /// UMA context (slots, overlays, etc...)
        /// </summary>
        UMAContext m_umaContext;

        /// <summary>
        /// UMA current dynamic avatar data
        /// </summary>
        UMAData m_umaData;

        #endregion

        #region Behaviour methods

        protected new void Awake()
        {
            base.Awake();

            //check if UmaKit contains UmaGenerator and UmaContext. If not, throw an exception
            var generators = UmaKit.GetComponentsInChildren<UMAGenerator>();
            var contexts = UmaKit.GetComponentsInChildren<UMAContext>();

            if(generators == null || generators.Length == 0 || contexts == null || contexts.Length == 0)
            {
                if(Log.IsErrorEnabled)
                {
                    Log.Error("RandomUmaGenerator - Bad UmaKit provided");
                }
            }
            else
            {
                m_umaGenerator = generators[0];
                m_umaContext = contexts[0];
            }

            if (Log.IsDebugEnabled)
            {
                Log.Debug("RandomUmaGenerator - Correctly awaken");
            }

        }

        #endregion

        #region UmaBodyGenerator members

        /// <summary>
        /// Generates a UMA-like avatar
        /// </summary>
        /// <param name="umaAvatar">Out parameter, receiving the just created UMA-compatible avatar</param>
        /// <param name="jointsMapping">Out parameter, receiving the joint mappings for the created uma avatar</param>
        /// <param name="jointsGlobalTRotationMapping">Out parameter, receiving the joint to-T-rotation mappings for the created uma avatar</param>
        public override void GenerateAvatar(out GameObject umaAvatar, out IDictionary<UmaJointTypes, string> jointsMapping, out IDictionary<UmaJointTypes, Quaternion> jointsGlobalTRotationMapping)
        {  
            //create new gameobject for this avatar as child of this object
            umaAvatar = GenerateOneUMA(); 

            //generate the mappings (standard, because we are using a standard UMA avatar)
            jointsMapping = UmaBodyGenerator.StandardUmaJointMappings;

            //get rotational mappings from the standard T avatar
            //TODO: CREATE A STANDARD DICTIONARY OF MAPPINGS FOR STANDARD UMA POSES
            jointsGlobalTRotationMapping = UmaBodyGenerator.GetJointGlobalTRotationsMappings(Resources.Load<GameObject>(TAvatarResourceName), jointsMapping);

            if (Log.IsDebugEnabled)
            {
                Log.Debug("RandomUmaGenerator - Generated random avatar");
            }
        }

        /// <summary>
        /// Get a UMA Bridge object, capable of moving UMA Dna Sliders on the Avatar
        /// </summary>
        /// <param name="umaInstanceGo">Avatar instance, generated by this generator, which we want to match with user's body</param>
        /// <returns>UMA Bridge object</returns>
        public override IUmaPhysioMatchingBridge GetUmaMatchingBridge(GameObject umaInstanceGo)
        {
            return new UmaAvatarBridge(umaInstanceGo.GetComponent<UMADynamicAvatar>());
        }

        #endregion

        #region UMACrowd-based methods

        /// <summary>
        /// Generate a new Random UMA and attach it as child of this object
        /// </summary>
        /// <returns></returns>
        public GameObject GenerateOneUMA()
        {
            int sex = Random.Range(0, 2);

            GameObject newGO = new GameObject("Random Avatar");
            newGO.transform.SetParent(transform, false);

            UMADynamicAvatar umaDynamicAvatar = newGO.AddComponent<UMADynamicAvatar>();
            umaDynamicAvatar.Initialize();
            umaDynamicAvatar.context = m_umaContext;
            umaDynamicAvatar.umaGenerator = m_umaGenerator;
            m_umaData = umaDynamicAvatar.umaData;
           
            var umaRecipe = umaDynamicAvatar.umaData.umaRecipe;
            UMACrowdRandomSet.CrowdRaceData race = null;

            if (RandomFeaturesPool != null && RandomFeaturesPool.Length > 0)
            {
                int randomResult = Random.Range(0, RandomFeaturesPool.Length);
                race = RandomFeaturesPool[randomResult].data;
                umaRecipe.SetRace(m_umaContext.raceLibrary.GetRace(race.raceID));
            }
            else
            {
                if (sex == 0)
                {
                    umaRecipe.SetRace(m_umaContext.raceLibrary.GetRace("HumanMale"));
                }
                else
                {
                    umaRecipe.SetRace(m_umaContext.raceLibrary.GetRace("HumanFemale"));
                }
            }

            if (race != null && race.slotElements.Length > 0)
            {
                DefineSlots(race);
            }
            else
            {
                DefineSlots();
            }

            AddAdditionalSlots();

            GenerateUMAShapes();

            umaDynamicAvatar.Show();

            return newGO;
        }

        void DefineSlots()
        {
            Color skinColor = new Color(1, 1, 1, 1);
            float skinTone;

            skinTone = Random.Range(0.1f, 0.6f);
            skinColor = new Color(skinTone + Random.Range(0.35f, 0.4f), skinTone + Random.Range(0.25f, 0.4f), skinTone + Random.Range(0.35f, 0.4f), 1);

            Color HairColor = new Color(Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), 1);

            if (m_umaData.umaRecipe.raceData.raceName == "HumanMale")
            {
                int randomResult = 0;
                //Male Avatar

                m_umaData.umaRecipe.slotDataList = new SlotData[15];

                m_umaData.umaRecipe.slotDataList[0] = m_umaContext.slotLibrary.InstantiateSlot("MaleEyes");
                m_umaData.umaRecipe.slotDataList[0].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("EyeOverlay"));
                m_umaData.umaRecipe.slotDataList[0].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("EyeOverlayAdjust", new Color(Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), 1)));

                randomResult = Random.Range(0, 2);
                if (randomResult == 0)
                {
                    m_umaData.umaRecipe.slotDataList[1] = m_umaContext.slotLibrary.InstantiateSlot("MaleFace");

                    randomResult = Random.Range(0, 2);

                    if (randomResult == 0)
                    {
                        m_umaData.umaRecipe.slotDataList[1].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("MaleHead01", skinColor));
                    }
                    else if (randomResult == 1)
                    {
                        m_umaData.umaRecipe.slotDataList[1].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("MaleHead02", skinColor));
                    }
                }
                else if (randomResult == 1)
                {
                    m_umaData.umaRecipe.slotDataList[1] = m_umaContext.slotLibrary.InstantiateSlot("MaleHead_Head");

                    randomResult = Random.Range(0, 2);
                    if (randomResult == 0)
                    {
                        m_umaData.umaRecipe.slotDataList[1].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("MaleHead01", skinColor));
                    }
                    else if (randomResult == 1)
                    {
                        m_umaData.umaRecipe.slotDataList[1].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("MaleHead02", skinColor));
                    }

                    m_umaData.umaRecipe.slotDataList[7] = m_umaContext.slotLibrary.InstantiateSlot("MaleHead_Eyes", m_umaData.umaRecipe.slotDataList[1].GetOverlayList());
                    m_umaData.umaRecipe.slotDataList[9] = m_umaContext.slotLibrary.InstantiateSlot("MaleHead_Mouth", m_umaData.umaRecipe.slotDataList[1].GetOverlayList());

                    randomResult = Random.Range(0, 2);
                    if (randomResult == 0)
                    {
                        m_umaData.umaRecipe.slotDataList[10] = m_umaContext.slotLibrary.InstantiateSlot("MaleHead_PigNose", m_umaData.umaRecipe.slotDataList[1].GetOverlayList());
                        m_umaData.umaRecipe.slotDataList[1].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("MaleHead_PigNose", skinColor));
                    }
                    else if (randomResult == 1)
                    {
                        m_umaData.umaRecipe.slotDataList[10] = m_umaContext.slotLibrary.InstantiateSlot("MaleHead_Nose", m_umaData.umaRecipe.slotDataList[1].GetOverlayList());
                    }

                    randomResult = Random.Range(0, 2);
                    if (randomResult == 0)
                    {
                        m_umaData.umaRecipe.slotDataList[8] = m_umaContext.slotLibrary.InstantiateSlot("MaleHead_ElvenEars");
                        m_umaData.umaRecipe.slotDataList[8].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("ElvenEars", skinColor));
                    }
                    else if (randomResult == 1)
                    {
                        m_umaData.umaRecipe.slotDataList[8] = m_umaContext.slotLibrary.InstantiateSlot("MaleHead_Ears", m_umaData.umaRecipe.slotDataList[1].GetOverlayList());
                    }
                }


                randomResult = Random.Range(0, 3);
                if (randomResult == 0)
                {
                    m_umaData.umaRecipe.slotDataList[1].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("MaleHair01", HairColor * 0.25f));
                }
                else if (randomResult == 1)
                {
                    m_umaData.umaRecipe.slotDataList[1].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("MaleHair02", HairColor * 0.25f));
                }
                else
                {

                }


                randomResult = Random.Range(0, 4);
                if (randomResult == 0)
                {
                    m_umaData.umaRecipe.slotDataList[1].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("MaleBeard01", HairColor * 0.15f));
                }
                else if (randomResult == 1)
                {
                    m_umaData.umaRecipe.slotDataList[1].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("MaleBeard02", HairColor * 0.15f));
                }
                else if (randomResult == 2)
                {
                    m_umaData.umaRecipe.slotDataList[1].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("MaleBeard03", HairColor * 0.15f));
                }
                else
                {

                }



                //Extra beard composition
                randomResult = Random.Range(0, 4);
                if (randomResult == 0)
                {
                    m_umaData.umaRecipe.slotDataList[1].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("MaleBeard01", HairColor * 0.15f));
                }
                else if (randomResult == 1)
                {
                    m_umaData.umaRecipe.slotDataList[1].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("MaleBeard02", HairColor * 0.15f));
                }
                else if (randomResult == 2)
                {
                    m_umaData.umaRecipe.slotDataList[1].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("MaleBeard03", HairColor * 0.15f));
                }
                else
                {

                }

                randomResult = Random.Range(0, 2);
                if (randomResult == 0)
                {
                    m_umaData.umaRecipe.slotDataList[1].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("MaleEyebrow01", HairColor * 0.05f));
                }
                else
                {
                    m_umaData.umaRecipe.slotDataList[1].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("MaleEyebrow02", HairColor * 0.05f));
                }

                m_umaData.umaRecipe.slotDataList[2] = m_umaContext.slotLibrary.InstantiateSlot("MaleTorso");

                randomResult = Random.Range(0, 2);
                if (randomResult == 0)
                {
                    m_umaData.umaRecipe.slotDataList[2].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("MaleBody01", skinColor));
                }
                else
                {
                    m_umaData.umaRecipe.slotDataList[2].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("MaleBody02", skinColor));
                }


                randomResult = Random.Range(0, 2);
                if (randomResult == 0)
                {
                    m_umaData.umaRecipe.slotDataList[2].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("MaleShirt01", new Color(Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), 1)));
                }

                m_umaData.umaRecipe.slotDataList[3] = m_umaContext.slotLibrary.InstantiateSlot("MaleHands", m_umaData.umaRecipe.slotDataList[2].GetOverlayList());

                m_umaData.umaRecipe.slotDataList[4] = m_umaContext.slotLibrary.InstantiateSlot("MaleInnerMouth");
                m_umaData.umaRecipe.slotDataList[4].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("InnerMouth"));


                randomResult = Random.Range(0, 2);
                if (randomResult == 0)
                {
                    m_umaData.umaRecipe.slotDataList[5] = m_umaContext.slotLibrary.InstantiateSlot("MaleLegs", m_umaData.umaRecipe.slotDataList[2].GetOverlayList());
                    m_umaData.umaRecipe.slotDataList[2].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("MaleUnderwear01", new Color(Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), 1)));
                }
                else
                {
                    m_umaData.umaRecipe.slotDataList[5] = m_umaContext.slotLibrary.InstantiateSlot("MaleJeans01");
                    m_umaData.umaRecipe.slotDataList[5].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("MaleJeans01", new Color(Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), 1)));
                }

                m_umaData.umaRecipe.slotDataList[6] = m_umaContext.slotLibrary.InstantiateSlot("MaleFeet", m_umaData.umaRecipe.slotDataList[2].GetOverlayList());
            }
            else if (m_umaData.umaRecipe.raceData.raceName == "HumanFemale")
            {
                int randomResult = 0;
                //Female Avatar

                //Example of dynamic list
                List<SlotData> tempSlotList = new List<SlotData>();

                tempSlotList.Add(m_umaContext.slotLibrary.InstantiateSlot("FemaleEyes"));
                tempSlotList[tempSlotList.Count - 1].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("EyeOverlay"));
                tempSlotList[tempSlotList.Count - 1].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("EyeOverlayAdjust", new Color(Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), 1)));

                int headIndex = 0;

                randomResult = Random.Range(0, 2);
                if (randomResult == 0)
                {

                    tempSlotList.Add(m_umaContext.slotLibrary.InstantiateSlot("FemaleFace"));
                    headIndex = tempSlotList.Count - 1;
                    tempSlotList[headIndex].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("FemaleHead01", skinColor));
                    tempSlotList[headIndex].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("FemaleEyebrow01", new Color(0.125f, 0.065f, 0.065f, 1.0f)));

                    randomResult = Random.Range(0, 2);
                    if (randomResult == 0)
                    {
                        tempSlotList[headIndex].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("FemaleLipstick01", new Color(skinColor.r + Random.Range(0.0f, 0.3f), skinColor.g, skinColor.b + Random.Range(0.0f, 0.2f), 1)));
                    }
                }
                else if (randomResult == 1)
                {
                    tempSlotList.Add(m_umaContext.slotLibrary.InstantiateSlot("FemaleHead_Head"));
                    headIndex = tempSlotList.Count - 1;
                    tempSlotList[headIndex].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("FemaleHead01", skinColor));
                    tempSlotList[headIndex].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("FemaleEyebrow01", new Color(0.125f, 0.065f, 0.065f, 1.0f)));

                    tempSlotList.Add(m_umaContext.slotLibrary.InstantiateSlot("FemaleHead_Eyes", tempSlotList[headIndex].GetOverlayList()));
                    tempSlotList.Add(m_umaContext.slotLibrary.InstantiateSlot("FemaleHead_Mouth", tempSlotList[headIndex].GetOverlayList()));
                    tempSlotList.Add(m_umaContext.slotLibrary.InstantiateSlot("FemaleHead_Nose", tempSlotList[headIndex].GetOverlayList()));


                    randomResult = Random.Range(0, 2);
                    if (randomResult == 0)
                    {
                        tempSlotList.Add(m_umaContext.slotLibrary.InstantiateSlot("FemaleHead_ElvenEars"));
                        tempSlotList[tempSlotList.Count - 1].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("ElvenEars", skinColor));
                    }
                    else if (randomResult == 1)
                    {
                        tempSlotList.Add(m_umaContext.slotLibrary.InstantiateSlot("FemaleHead_Ears", tempSlotList[headIndex].GetOverlayList()));
                    }

                    randomResult = Random.Range(0, 2);
                    if (randomResult == 0)
                    {
                        tempSlotList[headIndex].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("FemaleLipstick01", new Color(skinColor.r + Random.Range(0.0f, 0.3f), skinColor.g, skinColor.b + Random.Range(0.0f, 0.2f), 1)));
                    }
                }

                tempSlotList.Add(m_umaContext.slotLibrary.InstantiateSlot("FemaleEyelash"));
                tempSlotList[tempSlotList.Count - 1].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("FemaleEyelash", Color.black));

                tempSlotList.Add(m_umaContext.slotLibrary.InstantiateSlot("FemaleTorso"));
                int bodyIndex = tempSlotList.Count - 1;
                randomResult = Random.Range(0, 2);
                if (randomResult == 0)
                {
                    tempSlotList[bodyIndex].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("FemaleBody01", skinColor));
                } if (randomResult == 1)
                {
                    tempSlotList[bodyIndex].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("FemaleBody02", skinColor));
                }

                tempSlotList[bodyIndex].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("FemaleUnderwear01", new Color(Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), 1)));

                randomResult = Random.Range(0, 4);
                if (randomResult == 0)
                {
                    tempSlotList[bodyIndex].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("FemaleShirt01", new Color(Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), 1)));
                }
                else if (randomResult == 1)
                {
                    tempSlotList[bodyIndex].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("FemaleShirt02", new Color(Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), 1)));
                }
                else if (randomResult == 2)
                {
                    tempSlotList.Add(m_umaContext.slotLibrary.InstantiateSlot("FemaleTshirt01"));
                    tempSlotList[tempSlotList.Count - 1].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("FemaleTshirt01", new Color(Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), 1)));
                }
                else
                {

                }

                tempSlotList.Add(m_umaContext.slotLibrary.InstantiateSlot("FemaleHands", tempSlotList[bodyIndex].GetOverlayList()));

                tempSlotList.Add(m_umaContext.slotLibrary.InstantiateSlot("FemaleInnerMouth"));
                tempSlotList[tempSlotList.Count - 1].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("InnerMouth"));

                tempSlotList.Add(m_umaContext.slotLibrary.InstantiateSlot("FemaleFeet", tempSlotList[bodyIndex].GetOverlayList()));


                randomResult = Random.Range(0, 2);

                if (randomResult == 0)
                {
                    tempSlotList.Add(m_umaContext.slotLibrary.InstantiateSlot("FemaleLegs", tempSlotList[bodyIndex].GetOverlayList()));
                }
                else if (randomResult == 1)
                {
                    tempSlotList.Add(m_umaContext.slotLibrary.InstantiateSlot("FemaleLegs", tempSlotList[bodyIndex].GetOverlayList()));
                    tempSlotList[bodyIndex].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("FemaleJeans01", new Color(Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), 1)));
                }

                randomResult = Random.Range(0, 3);
                if (randomResult == 0)
                {
                    tempSlotList.Add(m_umaContext.slotLibrary.InstantiateSlot("FemaleShortHair01", tempSlotList[headIndex].GetOverlayList()));
                    tempSlotList[headIndex].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("FemaleShortHair01", HairColor));
                }
                else if (randomResult == 1)
                {
                    tempSlotList.Add(m_umaContext.slotLibrary.InstantiateSlot("FemaleLongHair01", tempSlotList[headIndex].GetOverlayList()));
                    tempSlotList[headIndex].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("FemaleLongHair01", HairColor));
                }
                else
                {
                    tempSlotList.Add(m_umaContext.slotLibrary.InstantiateSlot("FemaleLongHair01", tempSlotList[headIndex].GetOverlayList()));
                    tempSlotList[headIndex].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("FemaleLongHair01", HairColor));

                    tempSlotList.Add(m_umaContext.slotLibrary.InstantiateSlot("FemaleLongHair01_Module"));
                    tempSlotList[tempSlotList.Count - 1].AddOverlay(m_umaContext.overlayLibrary.InstantiateOverlay("FemaleLongHair01_Module", HairColor));
                }

                m_umaData.SetSlots(tempSlotList.ToArray());
            }
        }

        private void DefineSlots(UMACrowdRandomSet.CrowdRaceData race)
        {
            float skinTone = Random.Range(0.1f, 0.6f);
            Color skinColor = new Color(skinTone + Random.Range(0.35f, 0.4f), skinTone + Random.Range(0.25f, 0.4f), skinTone + Random.Range(0.35f, 0.4f), 1);
            Color HairColor = new Color(Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.5f));
            var keywordsLookup = new HashSet<string>();
            UMACrowdRandomSet.Apply(m_umaData, race, skinColor, HairColor, keywordsLookup, m_umaContext.slotLibrary, m_umaContext.overlayLibrary);
        }

        private void AddAdditionalSlots()
        {
            m_umaData.AddAdditionalRecipes(AdditionalRecipes, m_umaContext);
        }

        protected virtual void GenerateUMAShapes()
        {
            UMADnaHumanoid umaDna = m_umaData.umaRecipe.GetDna<UMADnaHumanoid>();
            if (umaDna == null)
            {
                umaDna = new UMADnaHumanoid();
                m_umaData.umaRecipe.AddDna(umaDna);
            }

            umaDna.height = Random.Range(0.3f, 0.5f);
            umaDna.headSize = Random.Range(0.485f, 0.515f);
            umaDna.headWidth = Random.Range(0.4f, 0.6f);

            umaDna.neckThickness = Random.Range(0.495f, 0.51f);

            if (m_umaData.umaRecipe.raceData.raceName == "HumanMale")
            {
                umaDna.handsSize = Random.Range(0.485f, 0.515f);
                umaDna.feetSize = Random.Range(0.485f, 0.515f);
                umaDna.legSeparation = Random.Range(0.4f, 0.6f);
                umaDna.waist = 0.5f;
            }
            else
            {
                umaDna.handsSize = Random.Range(0.485f, 0.515f);
                umaDna.feetSize = Random.Range(0.485f, 0.515f);
                umaDna.legSeparation = Random.Range(0.485f, 0.515f);
                umaDna.waist = Random.Range(0.3f, 0.8f);
            }

            umaDna.armLength = Random.Range(0.485f, 0.515f);
            umaDna.forearmLength = Random.Range(0.485f, 0.515f);
            umaDna.armWidth = Random.Range(0.3f, 0.8f);
            umaDna.forearmWidth = Random.Range(0.3f, 0.8f);

            umaDna.upperMuscle = Random.Range(0.0f, 1.0f);
            umaDna.upperWeight = Random.Range(-0.2f, 0.2f) + umaDna.upperMuscle;
            if (umaDna.upperWeight > 1.0) { umaDna.upperWeight = 1.0f; }
            if (umaDna.upperWeight < 0.0) { umaDna.upperWeight = 0.0f; }

            umaDna.lowerMuscle = Random.Range(-0.2f, 0.2f) + umaDna.upperMuscle;
            if (umaDna.lowerMuscle > 1.0) { umaDna.lowerMuscle = 1.0f; }
            if (umaDna.lowerMuscle < 0.0) { umaDna.lowerMuscle = 0.0f; }

            umaDna.lowerWeight = Random.Range(-0.1f, 0.1f) + umaDna.upperWeight;
            if (umaDna.lowerWeight > 1.0) { umaDna.lowerWeight = 1.0f; }
            if (umaDna.lowerWeight < 0.0) { umaDna.lowerWeight = 0.0f; }

            umaDna.belly = umaDna.upperWeight;
            umaDna.legsSize = Random.Range(0.4f, 0.6f);
            umaDna.gluteusSize = Random.Range(0.4f, 0.6f);

            umaDna.earsSize = Random.Range(0.3f, 0.8f);
            umaDna.earsPosition = Random.Range(0.3f, 0.8f);
            umaDna.earsRotation = Random.Range(0.3f, 0.8f);

            umaDna.noseSize = Random.Range(0.3f, 0.8f);

            umaDna.noseCurve = Random.Range(0.3f, 0.8f);
            umaDna.noseWidth = Random.Range(0.3f, 0.8f);
            umaDna.noseInclination = Random.Range(0.3f, 0.8f);
            umaDna.nosePosition = Random.Range(0.3f, 0.8f);
            umaDna.nosePronounced = Random.Range(0.3f, 0.8f);
            umaDna.noseFlatten = Random.Range(0.3f, 0.8f);

            umaDna.chinSize = Random.Range(0.3f, 0.8f);
            umaDna.chinPronounced = Random.Range(0.3f, 0.8f);
            umaDna.chinPosition = Random.Range(0.3f, 0.8f);

            umaDna.mandibleSize = Random.Range(0.45f, 0.52f);
            umaDna.jawsSize = Random.Range(0.3f, 0.8f);
            umaDna.jawsPosition = Random.Range(0.3f, 0.8f);

            umaDna.cheekSize = Random.Range(0.3f, 0.8f);
            umaDna.cheekPosition = Random.Range(0.3f, 0.8f);
            umaDna.lowCheekPronounced = Random.Range(0.3f, 0.8f);
            umaDna.lowCheekPosition = Random.Range(0.3f, 0.8f);

            umaDna.foreheadSize = Random.Range(0.3f, 0.8f);
            umaDna.foreheadPosition = Random.Range(0.15f, 0.65f);

            umaDna.lipsSize = Random.Range(0.3f, 0.8f);
            umaDna.mouthSize = Random.Range(0.3f, 0.8f);
            umaDna.eyeRotation = Random.Range(0.3f, 0.8f);
            umaDna.eyeSize = Random.Range(0.3f, 0.8f);
            umaDna.breastSize = Random.Range(0.3f, 0.8f);
        }

        #endregion
    }
}

